using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Advobot.Modules;
using Advobot.Utilities;

using AdvorangesUtils;

using Discord.Commands;

using ImageMagick;

namespace Advobot.Services.ImageResizing
{
	/// <summary>
	/// Runs image resizing in background threads. The arguments get enqueued, then the image is resized, and finally the callback is invoked in order to use the resized image.
	/// </summary>
	internal sealed class ImageResizer : IImageResizer
	{
		private static readonly string? _FfmpegLocation = FindFfmpeg();
		private const long _MaxDownloadLengthInBytes = 10000000;

		private readonly ConcurrentQueue<IImageContext> _Args = new ConcurrentQueue<IImageContext>();
		private readonly ConcurrentDictionary<ulong, byte> _CurrentlyProcessing = new ConcurrentDictionary<ulong, byte>();
		private readonly SemaphoreSlim _SemaphoreSlim;
		private readonly HttpClient _Client;

		/// <inheritdoc />
		public int QueueCount => _Args.Count;

		/// <summary>
		/// Creates an instance of <see cref="ImageResizer"/>.
		/// </summary>
		/// <param name="client"></param>
		public ImageResizer(HttpClient client) : this(client, 10) { }

		/// <summary>
		/// Creates an instance of <see cref="ImageResizer"/>.
		/// </summary>
		/// <param name="client"></param>
		/// <param name="threads"></param>
		public ImageResizer(HttpClient client, int threads)
		{
			_Client = client;
			_SemaphoreSlim = new SemaphoreSlim(threads);
		}

		/// <inheritdoc />
		public IEnumerable<IImageContext> GetQueuedArguments()
			=> _Args.ToArray();

		/// <inheritdoc />
		public bool IsGuildAlreadyProcessing(ulong guildId)
			=> _CurrentlyProcessing.ContainsKey(guildId);

		/// <inheritdoc />
		public void Enqueue(IImageContext context)
		{
			_Args.Enqueue(context);
			_CurrentlyProcessing.AddOrUpdate(context.GuildId, 0, (_, __) => 0);
			if (_SemaphoreSlim.CurrentCount <= 0)
			{
				return;
			}

			Task.Run(async () =>
			{
				//Lock since only a few threads should be processing this at once
				await _SemaphoreSlim.WaitAsync().CAF();
				while (_Args.TryDequeue(out var args))
				{
					try
					{
						await args.ReportAsync("Starting to download the file.").CAF();

						using var ms = new MemoryStream();
						var result = await ResizeImageAsync(ms, _Client, args).CAF();
						if (!result.IsSuccess)
						{
							await args.SendFinalResponseAsync(result).CAF();
							continue;
						}

						var used = await args.UseStream(ms).CAF();
						await args.SendFinalResponseAsync(used).CAF();
					}
					finally
					{
						_CurrentlyProcessing.TryRemove(args.GuildId, out var removed);
					}
				}
				_SemaphoreSlim.Release();
			}).CAF();
		}

		private static async Task<IResult> ResizeImageAsync(MemoryStream ms, HttpClient client, IImageContext context)
		{
			static string TooLarge(long value)
				=> $"File is bigger than the max allowed size of {(double)value / 1000 * 1000:0.0}MB";

			MagickFormat format;
			using (var req = await client.GetAsync(context.Url).CAF())
			{
				var contentLength = req.Content.Headers.ContentLength;
				var mediaType = req.Content.Headers.ContentType.MediaType;

				//Max size without resize tries
				if (context.Args.ResizeTries < 1 && contentLength > context.MaxAllowedLengthInBytes)
				{
					return AdvobotResult.Failure(TooLarge(context.MaxAllowedLengthInBytes));
				}
				//Utter max size, even with resize tries
				else if (contentLength > _MaxDownloadLengthInBytes)
				{
					return AdvobotResult.Failure(TooLarge(_MaxDownloadLengthInBytes));
				}
				//Unknown media type
				else if (!Enum.TryParse(mediaType.Split('/').Last(), true, out format))
				{
					return AdvobotResult.Failure("Unknown image format supplied.");
				}
				var result = context.CanUseFormat(format);
				if (!result.IsSuccess)
				{
					return result;
				}

				//Copy the response stream to a new variable so it can be seeked on
				await req.Content.CopyToAsync(ms).CAF();
			}

			if (format == MagickFormat.Mp4) //Convert mp4 to gif so it can be used in animated gifs
			{
				await context.ReportAsync("Converting mp4 to gif.").CAF();
				await ConvertMp4ToGifAsync(ms, context).CAF();
				format = MagickFormat.Gif;
			}

			//Getting to this point has already checked resize tries, so this image needs to be resized if it's too big
			for (var i = 0; i < context.Args.ResizeTries && ms.Length > context.MaxAllowedLengthInBytes; ++i)
			{
				await context.ReportAsync($"Attempting to resize {i + 1}/{context.Args.ResizeTries}.").CAF();
				if (ResizeFile(ms, context, format)) //Acceptable size
				{
					break;
				}
				if (i == context.Args.ResizeTries - 1) //Too many attempts
				{
					return AdvobotResult.Failure(TooLarge(context.MaxAllowedLengthInBytes));
				}
			}
			if (ms.Length < 1) //Stream somehow got empty, will result in error if callback is attempted
			{
				return AdvobotResult.Failure("File is empty after shrinking.");
			}
			return AdvobotResult.IgnoreSuccess;
		}

		private static bool ResizeFile(MemoryStream ms, IImageContext context, MagickFormat format)
		{
			var shrinkFactor = Math.Sqrt((double)ms.Length / context.MaxAllowedLengthInBytes) * 1.1;
			void ProcessImage(IMagickImage image)
			{
				image.ColorFuzz = context.Args.ColorFuzzing;
				//Determine the new width and height to give these frames
				image.Scale(new MagickGeometry
				{
					IgnoreAspectRatio = true, //Ignore aspect ratio so all the frames keep the same dimensions
					Width = (int)Math.Min(128, image.Width / shrinkFactor),
					Height = (int)Math.Min(128, image.Height / shrinkFactor),
				});
			}
			void Overwrite(Action<MemoryStream> func)
			{
				ms.Position = 0;
				func(ms);
			}

			//Make sure at start
			ms.Position = 0;
			switch (format)
			{
				case MagickFormat.Gif:
					using (var gif = new MagickImageCollection(ms))
					{
						foreach (var frame in gif)
						{
							ProcessImage(frame);
						}
						Overwrite(x => gif.Write(x));
					}
					return ms.Length < context.MaxAllowedLengthInBytes;

				case MagickFormat.Jpg:
				case MagickFormat.Jpeg:
				case MagickFormat.Png:
					using (var image = new MagickImage(ms))
					{
						ProcessImage(image);
						Overwrite(x => image.Write(x));
					}
					return ms.Length < context.MaxAllowedLengthInBytes;

				default:
					throw new InvalidOperationException("Invalid image format supplied.");
			}
		}

		private static async Task ConvertMp4ToGifAsync(MemoryStream ms, IImageContext context)
		{
			const string Name = "in";
			var info = new ProcessStartInfo
			{
#if DEBUG
				CreateNoWindow = false,
#else
				CreateNoWindow = true,
#endif
				UseShellExecute = false,
				LoadUserProfile = false,
				RedirectStandardInput = true,
				RedirectStandardOutput = true,
				FileName = _FfmpegLocation,
				Arguments = $@"-f mp4 -i \\.\pipe\{Name} -ss {context.Args.StartInSeconds} -t {context.Args.LengthInSeconds} -vf fps=12,scale=256:256 -f gif pipe:1",
			};
			using var process = new Process { StartInfo = info, };
			//Have to use this pipe and not StandardInput b/c StandardInput hangs
			using var inPipe = new NamedPipeServerStream(Name, PipeDirection.Out, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous, (int)ms.Length, (int)ms.Length);

			process.Start();

			//Make sure the pipe is connected
			await inPipe.WaitForConnectionAsync().CAF();
			//Make sure to start at the beginning of the data to not get a "moov atom not found" error
			ms.Seek(0, SeekOrigin.Begin);
			await ms.CopyToAsync(inPipe).CAF();
			//Flush and close, otherwise hangs
			inPipe.Flush();
			inPipe.Close();

			//Clear and overwrite
			ms.SetLength(0);
			await process.StandardOutput.BaseStream.CopyToAsync(ms).CAF();
		}

		private static string? FindFfmpeg()
		{
			var windows = Environment.OSVersion.Platform.ToString().CaseInsContains("win");
			var ffmpeg = windows ? "ffmpeg.exe" : "ffmpeg";

			//Start with every special folder
			var directories = AdvobotUtils.GetValues<Environment.SpecialFolder>().Select(e =>
		   {
			   var p = Path.Combine(Environment.GetFolderPath(e), "ffmpeg");
			   return Directory.Exists(p) ? new DirectoryInfo(p) : null;
		   }).Where(x => x != null).ToList();
			//Look through where the program is stored
			if (Assembly.GetExecutingAssembly().Location is string assembly)
			{
				directories.Add(new DirectoryInfo(Path.GetDirectoryName(assembly)));
			}
			//Check path variables
			foreach (var part in (Environment.GetEnvironmentVariable("PATH") ?? "").Split(windows ? ';' : ':'))
			{
				if (!string.IsNullOrWhiteSpace(part))
				{
					directories.Add(new DirectoryInfo(part.Trim()));
				}
			}
			//Look through every directory and any subfolders they have called bin
			foreach (var dir in directories.SelectMany(x => new[] { x, new DirectoryInfo(Path.Combine(x?.FullName, "bin")) }))
			{
				if (dir?.Exists != true)
				{
					continue;
				}

				var files = dir.GetFiles(ffmpeg, SearchOption.TopDirectoryOnly);
				if (files.Length > 0)
				{
					return files[0].FullName;
				}
			}
			return null;
		}
	}
}