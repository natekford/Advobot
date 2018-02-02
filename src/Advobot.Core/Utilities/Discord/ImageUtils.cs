using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using ImageMagick;

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
		public static async Task<string> UseImageStream(this Uri uri, long maxSizeInBits, bool resizeIfTooBig, Func<Stream, Task> update)
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
					if (!resizeIfTooBig && resp.ContentLength > maxSizeInBits)
					{
						return $"file is bigger than the max allowed size of {(double)maxSizeInBits / 1000 * 1000:0.0}MB.";
					}

					using (var s = resp.GetResponseStream())
					using (var ms = new MemoryStream())
					{
						await s.CopyToAsync(ms).CAF();

						if (resizeIfTooBig)
						{
							while (ms.Length > maxSizeInBits)
							{
								await ResizeEmote(format, ms, maxSizeInBits).CAF();
							}
						}

						ms.Seek(0, SeekOrigin.Begin);
						await update(ms).CAF();
						return null;
					}
				}
			}
			catch (WebException we)
			{
				return we.Message;
			}
		}
		private static async Task ResizeEmote(MagickFormat format, MemoryStream ms, long maxSize)
		{
			//Make sure at start
			ms.Seek(0, SeekOrigin.Begin);
			//Find how much to shrink the image
			var shrinkFactor = Math.Sqrt((double)ms.Length / maxSize);

			if (format == MagickFormat.Gif)
			{
#if true
				var working =
					@"-i C:\Users\User\Downloads\tumblr_ooe75jcU6T1w8q6ufo1_400.gif " + //Specify the input file
					"-vf fps=20,scale=40:40 " + //Filtergraph arguments, fps, scale etc https://ffmpeg.org/ffmpeg.html#Simple-filtergraphs
					"-f gif " +                 //Force output file type to gif https://ffmpeg.org/ffmpeg.html#Main-options
					"pipe:1";                   //Output to stdout https://ffmpeg.org/ffmpeg-protocols.html#pipe
				var notWorking = "-f gif " +          //Accept unknown file types https://trac.ffmpeg.org/wiki/Slideshow
						//"-i " +                     //Specifies input https://ffmpeg.org/ffmpeg.html#Main-options
						"-i pipe:0 " +                 //Input from stdin https://ffmpeg.org/ffmpeg-protocols.html#pipe
						"-vf fps=20,scale=40:40 " + //Filtergraph arguments, fps, scale etc https://ffmpeg.org/ffmpeg.html#Simple-filtergraphs
						"-f gif " +                 //Force output file type to gif https://ffmpeg.org/ffmpeg.html#Main-options
						"pipe:1";                   //Output to stdout https://ffmpeg.org/ffmpeg-protocols.html#pipe
				var test = @"-f gif -i pipe:0 -f gif pipe:1";

				var info = new ProcessStartInfo
				{
					CreateNoWindow = false,
					UseShellExecute = false,
					LoadUserProfile = false,
					RedirectStandardInput = true,
					RedirectStandardOutput = true,
					FileName = "ffmpeg",
					Arguments = test,
						
				};
				using (var process = new Process { StartInfo = info, })
				{
					process.Start();

					var text = new StreamReader(ms).ReadToEnd();

					//Not sure why this gives pipe error
					process.StandardInput.Write(text);
					ms.SetLength(0);
					await process.StandardOutput.BaseStream.CopyToAsync(ms).CAF();
				}
#else
				using (var gif = new MagickImageCollection(ms))
				{
					//Assumes every image has the same width/height
					var width = (int)Math.Min(128, gif.First().Width / shrinkFactor);
					var height = (int)Math.Min(128, gif.First().Height / shrinkFactor);

					gif.Coalesce();
					foreach (var image in gif)
					{
						image.Resize(width, height);
						image.AnimationDelay = 100;
						image.ColorFuzz = new Percentage(.02);
					}
					gif.Optimize();
					gif.OptimizeTransparency();
					//Clear the stream
					ms.SetLength(0);
					//Then reset it
					gif.Write(ms);
				}
#endif
			}
			else
			{
				using (var image = new MagickImage(ms))
				{
					var width = (int)Math.Min(128, image.Width / shrinkFactor);
					var height = (int)Math.Min(128, image.Height / shrinkFactor);
					image.Resize(width, height);

					//Clear the stream
					ms.SetLength(0);
					//Then reset it
					image.Write(ms);
				}
			}
		}
	}
}
