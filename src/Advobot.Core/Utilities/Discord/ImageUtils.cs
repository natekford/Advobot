using Advobot.Core.Classes;
using Discord;
using ImageMagick;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Advobot.Core.Utilities
{
	public static class ImageUtils
	{
		/// <summary>
		/// Uses the image stream for the required function.
		/// </summary>
		/// <param name="uri"></param>
		/// <param name="update"></param>
		/// <returns></returns>
		public static async Task<string> UseImageStream(this Uri uri, IGuild guild, ImageResizerArgs args, Func<Stream, Task> update)
		{
			var editedUri = new Uri(uri.ToString().Replace(".gifv", ".gif"));

			var req = (HttpWebRequest)WebRequest.Create(editedUri);
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
					if (!Enum.TryParse<MagickFormat>(resp.ContentType.Split('/').Last(), true, out var format))
					{
						return $"invalid file format supplied.";
					}
					switch (format)
					{
						case MagickFormat.Jpg:
						case MagickFormat.Jpeg:
						case MagickFormat.Png:
							if (guild.Emotes.Where(x => !x.Animated).Count() >= 50)
							{
								return "there are already 50 non animated emotes.";
							}
							break;
						case MagickFormat.Mp4:
						case MagickFormat.Gif:
							if (guild.Emotes.Where(x => x.Animated).Count() >= 50)
							{
								return "there are already 50 animated emotes.";
							}
							break;
						default:
							return "link must lead to a png, jpg, or gif.";
					}
					if (args.ResizeTries < 1 && resp.ContentLength > args.MaxSize)
					{
						return $"file is bigger than the max allowed size of {(double)args.MaxSize / 1000 * 1000:0.0}MB.";
					}

					using (var s = resp.GetResponseStream())
					using (var ms = new MemoryStream())
					{
						await s.CopyToAsync(ms).CAF();
						//Convert mp4 to gif so it can be used in animated gifs
						if (format == MagickFormat.Mp4)
						{
							await ConvertMp4ToGif(ms).CAF();
							format = MagickFormat.Gif;
						}
						if (ms.Length < args.MaxSize)
						{
							ms.Seek(0, SeekOrigin.Begin);
							await update(ms).CAF();
							return null;
						}

						//Getting to this point has already checked resize tries, so this image needs to be resized if it's too big
						for (int i = 0; i < args.ResizeTries && ms.Length > args.MaxSize; ++i)
						{
							//If the emote gets small enough that it's acceptable, don't bother continuing
							if (ResizeEmote(ms, args, format, i == 0, out var width, out var height))
							{
								ms.Seek(0, SeekOrigin.Begin);
								await update(ms).CAF();
								return null;
							}
							//If the emote gets too small, stop and return an error
							if (width < 35 || height < 35)
							{
								return $"during resizing the image has been made too small. Manually resize instead.";
							}
						}
						return $"unable to shrink the file to the max allowed size of {(double)args.MaxSize / 1000 * 1000:0.0}MB.";
					}
				}
			}
			catch (WebException we)
			{
				return we.Message;
			}
		}
		private static async Task ConvertMp4ToGif(MemoryStream ms)
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
				FileName = "ffmpeg",
				Arguments = @"-f mp4 -i \\.\pipe\in -vf fps=10,scale=128:128 -f gif pipe:1",

			};
			using (var process = new Process { StartInfo = info, })
			//Have to use this pipe and not StandardInput b/c StandardInput hangs
			using (var pipe = new NamedPipeServerStream("in", PipeDirection.Out, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous, (int)ms.Length, (int)ms.Length))
			{
				process.Start();

				//Make sure the pipe is connected
				await pipe.WaitForConnectionAsync().CAF();
				//Make sure to start at the beginning of the data to not get a "moov atom not found" error
				ms.Seek(0, SeekOrigin.Begin);
				await ms.CopyToAsync(pipe).CAF();
				//Flush and close, otherwise hangs
				pipe.Flush();
				pipe.Close();

				//Clear and overwrite
				ms.SetLength(0);
				await process.StandardOutput.BaseStream.CopyToAsync(ms).CAF();
			}
		}
		private static bool ResizeEmote(MemoryStream ms, ImageResizerArgs args, MagickFormat format, bool firstIter, out int width, out int height)
		{
			//Make sure at start
			ms.Seek(0, SeekOrigin.Begin);
			var shrinkFactor = Math.Sqrt((double)ms.Length / args.MaxSize);
			switch (format)
			{
				case MagickFormat.Gif:
					using (var gif = new MagickImageCollection(ms))
					{
						/*
						if (firstIter && args.FrameSkip > 0)
						{
							//Trim the file size down by removing every x frames
							for (int i = gif.Count - 1; i >= 0; --i)
							{
								if (i % args.FrameSkip == 0)
								{
									gif.RemoveAt(i);
								}
							}
						}*/

						//TODO: figure out when to coalesce and optimize correctly

						//Optimize before, not after, so all the frames will keep the same dimension when resized
						//gif.Optimize();
						//Determine the new width and height to give these frames
						var geo = new MagickGeometry
						{
							IgnoreAspectRatio = true, //Ignore aspect ratio so all the frames keep the same dimensions
							Width = width = (int)Math.Min(128, gif[0].Width / shrinkFactor),
							Height = height = (int)Math.Min(128, gif[0].Height / shrinkFactor),
						};
						foreach (var frame in gif)
						{
							frame.ColorFuzz = args.ColorFuzzingPercentage;
							frame.AnimationDelay = args.AnimationDelay;
							frame.Scale(geo);
						}

						//Clear the stream and overwrite it
						ms.SetLength(0);
						gif.Write(ms);
					}
					return ms.Length < args.MaxSize;
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
					return ms.Length < args.MaxSize;
				default:
					throw new InvalidOperationException("This method only works on gif, png, and jpg formats.");
			}
		}
	}
}
