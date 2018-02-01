using Advobot.Core.Classes;
using Advobot.Core.Interfaces;
using Discord;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Image = SixLabors.ImageSharp.Image;

namespace Advobot.Core.Utilities
{
	public static class ImageUtils
	{
		/// <summary>
		/// Attempts to gather an image url from the message content or embeds. Returns true if no error occurs and a url is found.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="text"></param>
		/// <param name="url"></param>
		/// <param name="error"></param>
		/// <returns></returns>
		public static bool TryGetUri(IUserMessage message, string text, out Uri url, out IError error)
		{
			url = null;
			error = default;
			if (text != null && !Uri.TryCreate(text, UriKind.Absolute, out url))
			{
				error = new Error("Invalid Url provided.");
				return false;
			}
			if (url == null)
			{
				var attach = message.Attachments.Where(x => x.Width != null && x.Height != null).Select(x => x.Url);
				var embeds = message.Embeds.Where(x => x.Image.HasValue).Select(x => x.Image?.Url);
				var imageUrls = attach.Concat(embeds).ToList();
				if (imageUrls.Count == 1)
				{
					url = new Uri(imageUrls.First());
				}
				else if (imageUrls.Count > 1)
				{
					error = new Error("Too many attached or embedded images.");
					return false;
				}
			}
			return url != null;
		}
		/// <summary>
		/// Creates a webrequest for the given uri and sets the user agent, credentials, and timeout.
		/// </summary>
		/// <param name="uri"></param>
		/// <returns></returns>
		public static HttpWebRequest CreateWebRequest(this Uri uri)
		{
			var req = (HttpWebRequest)WebRequest.Create(uri);
			req.UserAgent = "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2228.0 Safari/537.36";
			req.Credentials = CredentialCache.DefaultCredentials;
			req.Timeout = 5000;
			req.ReadWriteTimeout = 5000;
			return req;
		}
		/// <summary>
		/// Uses the image stream for the required function.
		/// </summary>
		/// <param name="uri"></param>
		/// <param name="update"></param>
		/// <returns></returns>
		public static async Task<string> UseImageStream(this Uri uri, long maxSizeInBits, bool resizeIfTooBig, Func<Stream, Task> update)
		{
			try
			{
				using (var resp = await uri.CreateWebRequest().GetResponseAsync().CAF())
				{
					switch (resp.ContentType.Split('/').Last())
					{
						case "jpg":
						case "jpeg":
						case "png":
							break;
						default:
							return "Link must lead to a png or jpg.";
					}
					if (!resizeIfTooBig && resp.ContentLength > maxSizeInBits)
					{
						return $"Image is bigger than the max allowed size: {(double)maxSizeInBits / 1000 * 1000:0.0}MB.";
					}

					using (var s = resp.GetResponseStream())
					using (var ms = new MemoryStream())
					{
						await s.CopyToAsync(ms).CAF();

						if (resizeIfTooBig && resp.ContentLength > maxSizeInBits)
						{
							ResizeImage(ms, maxSizeInBits);
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
		private static void ResizeImage(Stream s, long maxSize)
		{
			//Make sure at start
			s.Seek(0, SeekOrigin.Begin);
			using (var image = Image.Load(s))
			{
				var shrinkFactor = Math.Sqrt((double)s.Length / maxSize);
				image.Mutate(x =>
				{
					x.Resize(new ResizeOptions
					{
						Mode = ResizeMode.Min,
						Size = new Size
						{
							Height = (int)Math.Min(128, image.Height / shrinkFactor),
							Width = (int)Math.Min(128, image.Width / shrinkFactor),
						}
					});
				});

				//Clear the stream
				s.SetLength(0);
				//Then reset it
				image.Save(s, ImageFormats.Jpeg);
			}
		}
	}
}
