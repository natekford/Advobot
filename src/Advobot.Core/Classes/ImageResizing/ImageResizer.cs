using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Advobot.Interfaces;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using ImageMagick;
using Context = Advobot.Classes.AdvobotCommandContext;

namespace Advobot.Classes.ImageResizing
{
	/// <summary>
	/// Runs image resizing in background threads. The arguments get enqueued, then the image is resized, and finally the callback is invoked in order to use the resized image.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public abstract class ImageResizer<T> where T : IImageResizerArguments
	{
		private static readonly string FfmpegLocation = FindFfmpeg();
		private const long MaxDownloadLengthInBytes = 10000000;

		private readonly ConcurrentQueue<ImageCreationArguments<T>> _Args = new ConcurrentQueue<ImageCreationArguments<T>>();
		private readonly ConcurrentDictionary<ulong, object> _CurrentlyWorkingGuilds = new ConcurrentDictionary<ulong, object>();
		private readonly SemaphoreSlim _SemaphoreSlim;
		private readonly string _Type;

		/// <summary>
		/// Returns true if there are any available threads to run.
		/// </summary>
		public bool CanStart => _SemaphoreSlim.CurrentCount > 0;
		/// <summary>
		/// Returns the current amount of items in the queue.
		/// </summary>
		public int QueueCount => _Args.Count;

		/// <summary>
		/// Resizes images and then uses the callback.
		/// </summary>
		/// <param name="threads">How many threads to run in the background.</param>
		/// <param name="type">Guild icon, bot icon, emote, webhook icon, etc. (only used in response messages, nothing important)</param>
		public ImageResizer(int threads, string type)
		{
			_SemaphoreSlim = new SemaphoreSlim(threads);
			_Type = type;
		}

		/// <summary>
		/// Returns true if the image is already being processed.
		/// </summary>
		/// <param name="guild"></param>
		/// <returns></returns>
		public bool IsGuildAlreadyProcessing(IGuild guild)
			=> _Args.Any(x => x.Context.Guild.Id == guild.Id) || _CurrentlyWorkingGuilds.Any(x => x.Key == guild.Id);
		/// <summary>
		/// Add the arguments to the queue in order to be resized.
		/// </summary>
		/// <param name="context">The current context.</param>
		/// <param name="args">The arguments to use when resizing the image.</param>
		/// <param name="uri">The url to download from.</param>
		/// <param name="options">The request options to use in the callback.</param>
		/// <param name="nameOrId">The name for an emote, or the id for a webhook.</param>
		public void EnqueueArguments(Context context, T args, Uri uri, RequestOptions options, string nameOrId = null)
		{
			_Args.Enqueue(new ImageCreationArguments<T>
			{
				Context = context,
				Args = args,
				Uri = uri,
				Options = options,
				NameOrId = nameOrId,
			});
		}
		/// <summary>
		/// If there are any threads available, this will start another thread processing image resizing and callback usage.
		/// </summary>
		public void StartProcessing()
		{
			//Store it as a variable to get rid of the warning and allow it to run on its own
			Task.Run(async () =>
			{
				//Lock since only a few threads should be processing this at once
				await _SemaphoreSlim.WaitAsync().CAF();
				while (_Args.TryDequeue(out var d))
				{
					_CurrentlyWorkingGuilds.AddOrUpdate(d.Context.Guild.Id, new object(), (k, v) => new object());
					using (var resp = await ResizeImageAsync(d.Uri, d.Context, d.Args).CAF())
					{
						if (!resp.IsSuccess)
						{
							await MessageUtils.SendErrorMessageAsync(d.Context, new Error($"Failed to create the {_Type}. Reason: {resp.Error}.")).CAF();
						}
						else if (await UseResizedImageStream(d.Context, resp.Stream, resp.Format, d.NameOrId, d.Options).CAF() is Error error)
						{
							await MessageUtils.SendErrorMessageAsync(d.Context, error).CAF();
						}
						else
						{
							await MessageUtils.MakeAndDeleteSecondaryMessageAsync(d.Context, $"Successfully created the {_Type}.").CAF();
						}
					}
					_CurrentlyWorkingGuilds.TryRemove(d.Context.Guild.Id, out var removed);
				}
				_SemaphoreSlim.Release();
			}).CAF();
		}
		/// <summary>
		/// Attempts to use the resized image stream for something. Returns an error if there is failure, otherwise returns null.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="stream"></param>
		/// <param name="format"></param>
		/// <param name="name"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		protected abstract Task<Error> UseResizedImageStream(Context context, MemoryStream stream, MagickFormat format, string name, RequestOptions options);
		/// <summary>
		/// Uses the image stream for the function passed into the constructor.
		/// Returns null if successful, returns an error string otherwise.
		/// The stream will be set to a position of 0 before the callback is invoked.
		/// </summary>
		/// <param name="uri">The uri to download the file from.</param>
		/// <param name="context">The current context.</param>
		/// <param name="args">The arguments to use on the file.</param>
		/// <returns></returns>
		private async Task<ResizedImageResult> ResizeImageAsync(Uri uri, Context context, IImageResizerArguments args)
		{
			var req = (HttpWebRequest)WebRequest.Create(uri.ToString().Replace(".gifv", ".mp4"));
			req.UserAgent = "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2228.0 Safari/537.36";
			req.Credentials = CredentialCache.DefaultCredentials;
			req.Timeout = 5000;
			req.ReadWriteTimeout = 5000;
			//Not sure why the proxy has to be set, but with it being null I would get a PlatformNotSupportedException
			req.Proxy = new WebProxy();
			//For Imgur to redirect to correct page
			req.AllowAutoRedirect = true;

			var message = await context.Channel.SendMessageAsync("Starting to download the file.").CAF();
			WebResponse response = null;
			MemoryStream stream = null;
			try
			{
				response = await req.GetResponseAsync().CAF();
				if (args.ResizeTries < 1 && response.ContentLength > args.MaxAllowedLengthInBytes) //Max size without resize tries
				{
					return new ResizedImageResult(stream, default, $"file is bigger than the max allowed size of {(double)args.MaxAllowedLengthInBytes / 1000 * 1000:0.0}MB");
				}
				if (response.ContentLength > MaxDownloadLengthInBytes) //Utter max size, even with resize tries
				{
					return new ResizedImageResult(stream, default, $"file is bigger than the max allowed size of {(double)MaxDownloadLengthInBytes / 1000 * 1000:0.0}MB");
				}
				if (!Enum.TryParse<MagickFormat>(response.ContentType.Split('/').Last(), true, out var format) || !args.ValidFormats.Contains(format))
				{
					return new ResizedImageResult(stream, default, $"invalid file format supplied");
				}
				switch (format)
				{
					case MagickFormat.Jpg:
					case MagickFormat.Jpeg:
					case MagickFormat.Png:
						if (context.Guild.Emotes.Where(x => !x.Animated).Count() >= 50)
						{
							return new ResizedImageResult(stream, format, "there are already 50 non animated emotes");
						}
						break;
					case MagickFormat.Mp4:
						if (string.IsNullOrWhiteSpace(FfmpegLocation))
						{
							return new ResizedImageResult(stream, format, "mp4 is an invalid file format if ffmpeg is not installed");
						}
						goto case MagickFormat.Gif;
					case MagickFormat.Gif:
						if (context.Guild.Emotes.Where(x => x.Animated).Count() >= 50)
						{
							return new ResizedImageResult(stream, format, "there are already 50 animated emotes");
						}
						break;
					default:
						return new ResizedImageResult(stream, format, "link must lead to a png, jpg, gif, or mp4");
				}

				//Copy the response stream to a new variable so it can be seeked on
				await response.GetResponseStream().CopyToAsync(stream = new MemoryStream()).CAF();
				if (format == MagickFormat.Mp4) //Convert mp4 to gif so it can be used in animated gifs
				{
					await message.ModifyAsync(x => x.Content = $"Converting mp4 to gif.").CAF();
					await ConvertMp4ToGif(stream, (EmoteResizerArguments)args).CAF();
					format = MagickFormat.Gif;
				}
				if (stream.Length < args.MaxAllowedLengthInBytes)
				{
					return new ResizedImageResult(stream, format, null);
				}

				//Getting to this point has already checked resize tries, so this image needs to be resized if it's too big
				for (int i = 0; i < args.ResizeTries && stream.Length > args.MaxAllowedLengthInBytes; ++i)
				{
					await message.ModifyAsync(x => x.Content = $"Attempting to resize {i + 1}/{args.ResizeTries}.").CAF();
					if (ResizeFile(stream, args, format, out var width, out var height)) //Acceptable size
					{
						break;
					}
					else if (width < 35 || height < 35) //Too small, will look like shit
					{
						return new ResizedImageResult(stream, format, $"during resizing the file has been made too small. Manually resize instead");
					}
					else if (i == args.ResizeTries - 1) //Too many attempts
					{
						return new ResizedImageResult(stream, format, $"failed to shrink the file to the max allowed size of {(double)args.MaxAllowedLengthInBytes / 1000 * 1000:0.0}MB");
					}
				}
				if (stream.Length < 1) //Stream somehow got empty, will result in error if callback is attempted
				{
					return new ResizedImageResult(stream, format, $"file is empty after shrinking");
				}

				return new ResizedImageResult(stream, format, null);
			}
			catch (Exception e)
			{
				return new ResizedImageResult(stream, default, e.Message);
			}
			//Not using using blocks because they cause the code to become too indented.
			finally
			{
				//Get rid of the update message
				await MessageUtils.DeleteMessageAsync(message, ClientUtils.CreateRequestOptions("image stream used")).CAF();
				response?.Dispose();
				//Stream isn't disposed here cause it's returned
			}
		}
		/// <summary>
		/// Converts the memory stream from an Mp4 to Gif.
		/// </summary>
		/// <param name="ms"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		private async Task ConvertMp4ToGif(MemoryStream ms, EmoteResizerArguments args)
		{
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
				Arguments = $@"-f mp4 -i \\.\pipe\in -ss {args.StartInSeconds} -t {args.LengthInSeconds} -vf fps=12,scale=256:256 -f gif pipe:1",
			};
			using (var process = new Process { StartInfo = info, })
			//Have to use this pipe and not StandardInput b/c StandardInput hangs
			using (var inPipe = new NamedPipeServerStream("in", PipeDirection.Out, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous, (int)ms.Length, (int)ms.Length))
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
		/// <summary>
		/// Shrinks the image's width/height.
		/// </summary>
		/// <param name="ms"></param>
		/// <param name="args"></param>
		/// <param name="format"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <returns></returns>
		private bool ResizeFile(MemoryStream ms, IImageResizerArguments args, MagickFormat format, out int width, out int height)
		{
			//Make sure at start
			ms.Seek(0, SeekOrigin.Begin);
			var shrinkFactor = Math.Sqrt((double)ms.Length / args.MaxAllowedLengthInBytes) * 1.1;
			switch (format)
			{
				case MagickFormat.Gif:
					using (var gif = new MagickImageCollection(ms))
					{
						//Determine the new width and height to give these frames
						var geo = new MagickGeometry
						{
							IgnoreAspectRatio = true, //Ignore aspect ratio so all the frames keep the same dimensions
							Width = width = (int)Math.Min(128, gif[0].Width / shrinkFactor),
							Height = height = (int)Math.Min(128, gif[0].Height / shrinkFactor),
						};
						foreach (var frame in gif)
						{
							frame.ColorFuzz = args.ColorFuzzing;
							frame.Scale(geo);
						}

						//Clear the stream and overwrite it
						ms.SetLength(0);
						gif.Write(ms);
					}
					return ms.Length < args.MaxAllowedLengthInBytes;
				case MagickFormat.Jpg:
				case MagickFormat.Jpeg:
				case MagickFormat.Png:
					using (var image = new MagickImage(ms))
					{
						image.ColorFuzz = args.ColorFuzzing;
						image.Scale(new MagickGeometry
						{
							IgnoreAspectRatio = true,
							Width = width = (int)Math.Min(128, image.Width / shrinkFactor),
							Height = height = (int)Math.Min(128, image.Height / shrinkFactor),
						});

						ms.SetLength(0);
						image.Write(ms);
					}
					return ms.Length < args.MaxAllowedLengthInBytes;
				default:
					throw new InvalidOperationException("This method only works on gif, png, and jpg formats.");
			}
		}
		/// <summary>
		/// Attempts to find the location of Ffmpeg. Returns null if it cannot be found.
		/// </summary>
		/// <returns></returns>
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
