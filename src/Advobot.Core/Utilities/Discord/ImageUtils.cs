using Advobot.Core.Classes;
using Advobot.Core.Interfaces;
using Discord;
using ImageMagick;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

namespace Advobot.Core.Utilities
{
	public static class ImageUtils
	{
		public const long MaxDownloadLengthInBytes = 10000000;
		public static readonly string FfmpegLocation = FindFfmpeg();

		/// <summary>
		/// Uses the image stream for <paramref name="callback"/>. Returns null if successful, returns an error otherwise.
		/// </summary>
		/// <param name="uri">The uri to download the file from.</param>
		/// <param name="guild">The guild to check emotes from.</param>
		/// <param name="args">The arguments to use on the file.</param>
		/// <param name="callback">What do to with the resized file.</param>
		/// <returns></returns>
		public static async Task<string> UseImageStream(this Uri uri, IGuild guild, IImageResizerArgs args, Func<MagickFormat, MemoryStream, Task> callback)
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

			try
			{
				using (var resp = await req.GetResponseAsync().CAF())
				{
					//Max size without resize tries
					if (args.ResizeTries < 1 && resp.ContentLength > args.MaxAllowedLengthInBytes)
					{
						return $"file is bigger than the max allowed size of {(double)args.MaxAllowedLengthInBytes / 1000 * 1000:0.0}MB";
					}
					//Utter max size
					if (resp.ContentLength > MaxDownloadLengthInBytes)
					{
						return $"file is bigger than the max allowed size of {(double)MaxDownloadLengthInBytes / 1000 * 1000:0.0}MB";
					}
					//Make sure the content type is an image content type
					if (!Enum.TryParse<MagickFormat>(resp.ContentType.Split('/').Last(), true, out var format) || !args.ValidFormats.Contains(format))
					{
						return $"invalid file format supplied";
					}
					switch (format)
					{
						case MagickFormat.Jpg:
						case MagickFormat.Jpeg:
						case MagickFormat.Png:
							if (guild.Emotes.Where(x => !x.Animated).Count() >= 50)
							{
								return "there are already 50 non animated emotes";
							}
							break;
						case MagickFormat.Mp4:
							if (String.IsNullOrWhiteSpace(FfmpegLocation))
							{
								return "mp4 is an invalid file format if ffmpeg is not installed";
							}
							goto case MagickFormat.Gif;
						case MagickFormat.Gif:
							if (guild.Emotes.Where(x => x.Animated).Count() >= 50)
							{
								return "there are already 50 animated emotes";
							}
							break;
						default:
							return "link must lead to a png, jpg, gif, or mp4";
					}

					using (var s = resp.GetResponseStream())
					using (var ms = new MemoryStream())
					{
						await s.CopyToAsync(ms).CAF();
						//Convert mp4 to gif so it can be used in animated gifs
						if (format == MagickFormat.Mp4)
						{
							await ConvertMp4ToGif(ms, (EmoteResizerArgs)args).CAF();
							format = MagickFormat.Gif;
						}
						if (ms.Length > args.MaxAllowedLengthInBytes)
						{
							//Getting to this point has already checked resize tries, so this image needs to be resized if it's too big
							for (int i = 0; i < args.ResizeTries && ms.Length > args.MaxAllowedLengthInBytes; ++i)
							{
								//If the emote gets small enough that it's acceptable, don't bother continuing
								if (ResizeFile(ms, args, format, i == 0, out var width, out var height))
								{
									break;
								}
								//If the emote gets too small, stop and return an error
								if (width < 35 || height < 35)
								{
									return $"during resizing the file has been made too small. Manually resize instead";
								}
								//Too many attempts 
								if (i == args.ResizeTries - 1)
								{
									return $"failed to shrink the file to the max allowed size of {(double)args.MaxAllowedLengthInBytes / 1000 * 1000:0.0}MB";
								}
							}
						}
						if (ms.Length < 1)
						{
							return $"file is empty after shrinking";
						}

						//Make sure the stream is at the beginning
						ms.Seek(0, SeekOrigin.Begin);
						try
						{
							await callback(format, ms).CAF();
						}
						catch (Exception e)
						{
							return e.Message;
						}
						return null;
					}
				}
			}
			catch (WebException we)
			{
				return we.Message;
			}
		}
		private static async Task ConvertMp4ToGif(MemoryStream ms, EmoteResizerArgs args)
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
				Arguments = $@"-f mp4 -i \\.\pipe\in -ss {args.StartInSeconds} -t {args.LengthInSeconds} -vf fps={(int)(100.0 / args.AnimationDelay)},scale=256:256 -f gif pipe:1",
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
		private static bool ResizeFile(MemoryStream ms, IImageResizerArgs args, MagickFormat format, bool firstIter, out int width, out int height)
		{
			//Make sure at start
			ms.Seek(0, SeekOrigin.Begin);
			var shrinkFactor = Math.Sqrt((double)ms.Length / args.MaxAllowedLengthInBytes) * 1.1;
			switch (format)
			{
				case MagickFormat.Gif:
					var gifArgs = (EmoteResizerArgs)args;
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
							frame.AnimationDelay = gifArgs.AnimationDelay;
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
						image.Scale(new MagickGeometry
						{
							IgnoreAspectRatio = true,
							Width = width = (int)Math.Min(128, image.Width / shrinkFactor),
							Height = height = (int)Math.Min(128, image.Height / shrinkFactor),
						});

						//Clear the stream and overwrite it
						ms.SetLength(0);
						image.Write(ms);
					}
					return ms.Length < args.MaxAllowedLengthInBytes;
				default:
					throw new InvalidOperationException("This method only works on gif, png, and jpg formats.");
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
				if (!String.IsNullOrWhiteSpace(part))
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
