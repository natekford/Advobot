using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Advobot.Interfaces;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord.Commands;
using ImageMagick;

namespace Advobot.Classes.ImageResizing
{
	/// <summary>
	/// Runs image resizing in background threads. The arguments get enqueued, then the image is resized, and finally the callback is invoked in order to use the resized image.
	/// </summary>
	public sealed class ImageResizer : IImageResizer
	{
		private static readonly string FfmpegLocation = FindFfmpeg();
		private const long MaxDownloadLengthInBytes = 10000000;

		private readonly ConcurrentQueue<IImageArgs> _Args = new ConcurrentQueue<IImageArgs>();
		private readonly ConcurrentDictionary<ulong, object> _CurrentlyProcessing = new ConcurrentDictionary<ulong, object>();
		private readonly SemaphoreSlim _SemaphoreSlim;
		private readonly HttpClient _Client;

		/// <inheritdoc />
		public int QueueCount => _Args.Count;

		/// <summary>
		/// Creates an instance of <see cref="ImageResizer"/>.
		/// </summary>
		/// <param name="threads"></param>
		public ImageResizer(int threads)
		{
			_SemaphoreSlim = new SemaphoreSlim(threads);
#warning replace with ImageDL client?
			_Client = new HttpClient(new HttpClientHandler
			{
				AllowAutoRedirect = true,
				Credentials = CredentialCache.DefaultCredentials,
				Proxy = new WebProxy(),
				AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
			});
		}

		/// <inheritdoc />
		public IEnumerable<IImageArgs> GetQueuedArguments()
			=> _Args;
		/// <inheritdoc />
		public bool IsGuildAlreadyProcessing(ulong guildId)
			=> _CurrentlyProcessing.ContainsKey(guildId);
		/// <inheritdoc />
		public void Process(IImageArgs arguments)
		{
			_Args.Enqueue(arguments);
			_CurrentlyProcessing.AddOrUpdate(arguments.Context.Guild.Id, new object(), (k, v) => new object());
			if (_SemaphoreSlim.CurrentCount <= 0)
			{
				return;
			}

			//Store it as a variable to get rid of the warning and allow it to run on its own
			Task.Run(async () =>
			{
				//Lock since only a few threads should be processing this at once
				await _SemaphoreSlim.WaitAsync().CAF();
				while (_Args.TryDequeue(out var args))
				{
					using (var resp = await ResizeImageAsync(_Client, args).CAF())
					{
#warning error then error then timed
						var t = args.Type.ToLower();
						if (!resp.IsSuccess)
						{
							await MessageUtils.SendMessageAsync(args.Context.Channel, $"Failed to create the {t}. Reason: {resp.ErrorReason}.").CAF();
							continue;
						}

						var used = await args.UseStream(resp.Stream, resp.Format).CAF();
						if (!used.IsSuccess)
						{
							await MessageUtils.SendMessageAsync(args.Context.Channel, $"Failed to create the {t}. Reason: {used.ErrorReason}.").CAF();
						}
						else
						{
							await MessageUtils.SendMessageAsync(args.Context.Channel, $"Successfully created the {t}.").CAF();
						}
					}
					_CurrentlyProcessing.TryRemove(args.Context.Guild.Id, out var removed);
				}
				_SemaphoreSlim.Release();
			}).CAF();
		}

		private static async Task<ImageResult> ResizeImageAsync(HttpClient client, IImageArgs args)
		{
			var message = await args.Context.Channel.SendMessageAsync("Starting to download the file.").CAF();
			var stream = new MemoryStream();
			var format = default(MagickFormat);

			using (var req = await client.GetAsync(args.Url).CAF())
			{
				var contentLength = req.Content.Headers.ContentLength;
				var mediaType = req.Content.Headers.ContentType.MediaType;

				if (args.UserArgs.ResizeTries < 1 && contentLength > args.MaxAllowedLengthInBytes) //Max size without resize tries
				{
					return ImageResult.FromError(CommandError.UnmetPrecondition, MaxLength(args.MaxAllowedLengthInBytes));
				}
				if (contentLength > MaxDownloadLengthInBytes) //Utter max size, even with resize tries
				{
					return ImageResult.FromError(CommandError.UnmetPrecondition, MaxLength(MaxDownloadLengthInBytes));
				}
				if (!Enum.TryParse(mediaType.Split('/').Last(), true, out format) || !args.ValidFormats.Contains(format))
				{
					return ImageResult.FromError(CommandError.UnmetPrecondition, "Invalid file format supplied.");
				}
				var result = GetImageFormatResult(args, format);
				if (!result.IsSuccess)
				{
					return result;
				}

				//Copy the response stream to a new variable so it can be seeked on
				await req.Content.CopyToAsync(stream).CAF();
			}

			var dontDispose = false;
			try
			{
				if (format == MagickFormat.Mp4) //Convert mp4 to gif so it can be used in animated gifs
				{
					await message.ModifyAsync(x => x.Content = $"Converting mp4 to gif.").CAF();
					await ConvertMp4ToGif(stream, args).CAF();
					format = MagickFormat.Gif;
				}

				if (stream.Length > args.MaxAllowedLengthInBytes)
				{
					//Getting to this point has already checked resize tries, so this image needs to be resized if it's too big
					for (int i = 0; i < args.UserArgs.ResizeTries && stream.Length > args.MaxAllowedLengthInBytes; ++i)
					{
						await message.ModifyAsync(x => x.Content = $"Attempting to resize {i + 1}/{args.UserArgs.ResizeTries}.").CAF();
						if (ResizeFile(stream, args, format, out var width, out var height)) //Acceptable size
						{
							break;
						}
						if (width < 35 || height < 35) //Too small, will look bad
						{
							return ImageResult.FromError(CommandError.ParseFailed, "During resizing the file has been made too small.");
						}
						if (i == args.UserArgs.ResizeTries - 1) //Too many attempts
						{
							return ImageResult.FromError(CommandError.ParseFailed, MaxLength(args.MaxAllowedLengthInBytes));
						}
					}
				}
				if (stream.Length < 1) //Stream somehow got empty, will result in error if callback is attempted
				{
					return ImageResult.FromError(CommandError.Exception, "File is empty after shrinking.");
				}

				dontDispose = true;
				return ImageResult.FromSuccess(stream, format);
			}
			finally
			{
				await message.DeleteAsync(ClientUtils.CreateRequestOptions("image stream used")).CAF();
				if (!dontDispose)
				{
					stream?.Dispose();
				}
			}
		}
		private static string MaxLength(long value)
			=> $"File is bigger than the max allowed size of {(double)value / 1000 * 1000:0.0}MB";
		private static ImageResult GetImageFormatResult(IImageArgs args, MagickFormat format)
		{
			switch (format)
			{
				case MagickFormat.Jpg:
				case MagickFormat.Jpeg:
				case MagickFormat.Png:
					var imageResult = args.CanUseImage();
					if (!imageResult.IsSuccess)
					{
						return ImageResult.FromError(imageResult);
					}
					//if (args.Context.Guild.Emotes.Count(x => !x.Animated) >= 50)
					break;
				case MagickFormat.Mp4:
					if (string.IsNullOrWhiteSpace(FfmpegLocation))
					{
						return ImageResult.FromError(CommandError.ParseFailed, "MP4 is an invalid file format if ffmpeg is not installed.");
					}
					goto case MagickFormat.Gif;
				case MagickFormat.Gif:
					var gifResult = args.CanUseImage();
					if (!gifResult.IsSuccess)
					{
						return ImageResult.FromError(gifResult);
					}
					//if (args.Context.Guild.Emotes.Count(x => x.Animated) >= 50)
					break;
				default:
					throw new InvalidOperationException("Invalid image format supplied.");
			}
			return ImageResult.FromSuccess(null, format);
		}
		private static async Task ConvertMp4ToGif(MemoryStream ms, IImageArgs args)
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
				FileName = FfmpegLocation,
				Arguments = $@"-f mp4 -i \\.\pipe\{Name} -ss {args.UserArgs.StartInSeconds} -t {args.UserArgs.LengthInSeconds} -vf fps=12,scale=256:256 -f gif pipe:1",
			};
			using (var process = new Process { StartInfo = info, })
			//Have to use this pipe and not StandardInput b/c StandardInput hangs
			using (var inPipe = new NamedPipeServerStream(Name, PipeDirection.Out, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous, (int)ms.Length, (int)ms.Length))
			{
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
		}
		private static bool ResizeFile(MemoryStream ms, IImageArgs args, MagickFormat format, out int width, out int height)
		{
			MagickGeometry CreateGeo(IMagickImage image, double shrink, out int w, out int h)
			{
				return new MagickGeometry
				{
					IgnoreAspectRatio = true, //Ignore aspect ratio so all the frames keep the same dimensions
					Width = w = (int)Math.Min(128, image.Width / shrink),
					Height = h = (int)Math.Min(128, image.Height / shrink),
				};
			}
			void Overwrite(Action<MemoryStream> func, MemoryStream stream)
			{
				stream.SetLength(0);
				func(stream);
			}

			//Make sure at start
			ms.Seek(0, SeekOrigin.Begin);
			var shrinkFactor = Math.Sqrt((double)ms.Length / args.MaxAllowedLengthInBytes) * 1.1;
			switch (format)
			{
				case MagickFormat.Gif:
					using (var gif = new MagickImageCollection(ms))
					{
						//Determine the new width and height to give these frames
						var geo = CreateGeo(gif[0], shrinkFactor, out width, out height);
						foreach (var frame in gif)
						{
							frame.ColorFuzz = args.UserArgs.ColorFuzzing;
							frame.Scale(geo);
						}
						Overwrite(x => gif.Write(x), ms);
					}
					return ms.Length < args.MaxAllowedLengthInBytes;
				case MagickFormat.Jpg:
				case MagickFormat.Jpeg:
				case MagickFormat.Png:
					using (var image = new MagickImage(ms))
					{
						image.ColorFuzz = args.UserArgs.ColorFuzzing;
						image.Scale(CreateGeo(image, shrinkFactor, out width, out height));
						Overwrite(x => image.Write(x), ms);
					}
					return ms.Length < args.MaxAllowedLengthInBytes;
				default:
					throw new InvalidOperationException("Invalid image format supplied.");
			}
		}
		private static string FindFfmpeg()
		{
			var windows = Environment.OSVersion.Platform.ToString().CaseInsContains("win");
			var ffmpeg = windows ? "ffmpeg.exe" : "ffmpeg";

			//Start with every special folder
			var directories = Enum.GetValues(typeof(Environment.SpecialFolder)).Cast<Environment.SpecialFolder>().Select(e =>
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
			foreach (var dir in directories.Select(x => new[] { x, new DirectoryInfo(Path.Combine(x.FullName, "bin")) }).SelectMany(x => x))
			{
				if (!dir.Exists)
				{
					continue;
				}

				var files = dir.GetFiles(ffmpeg, SearchOption.TopDirectoryOnly);
				if (files.Any())
				{
					return files[0].FullName;
				}
			}
			return null;
		}
	}
}
