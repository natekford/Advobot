using Advobot.Core.Classes;
using ImageMagick;
using System;
using System.IO;
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
		public static async Task<string> UseImageStream(this Uri uri, ImageResizerArgs args, Func<Stream, Task> update)
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
						case MagickFormat.Gif:
							break;
						default:
							return "link must lead to a png or jpg.";
					}
					if (args.ResizeTries < 1 && resp.ContentLength > args.MaxSize)
					{
						return $"file is bigger than the max allowed size of {(double)args.MaxSize / 1000 * 1000:0.0}MB.";
					}

					using (var s = resp.GetResponseStream())
					using (var ms = new MemoryStream())
					{
						await s.CopyToAsync(ms).CAF();
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
		private static bool ResizeEmote(MemoryStream ms, ImageResizerArgs args, MagickFormat format, bool firstIter, out int width, out int height)
		{
			//Make sure at start
			ms.Seek(0, SeekOrigin.Begin);
			var shrinkFactor = Math.Sqrt((double)ms.Length / args.MaxSize);
			if (format == MagickFormat.Gif)
			{
				using (var gif = new MagickImageCollection(ms))
				{
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
					}

					//Optimize before, not after, so all the frames will keep the same dimension when resized
					gif.Optimize();
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
						frame.GifDisposeMethod = GifDisposeMethod.Background;
						frame.AnimationDelay = args.AnimationDelay;
						frame.Scale(geo);
					}

					//Clear the stream and overwrite it
					ms.SetLength(0);
					gif.Write(ms);
				}
			}
			else
			{
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
			}
			return ms.Length < args.MaxSize;
		}
	}
}
