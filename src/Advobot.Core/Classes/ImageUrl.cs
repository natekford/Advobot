using Discord.Commands;
using System;
using System.Linq;
using System.Net;

namespace Advobot.Core.Classes
{
	/// <summary>
	/// Makes sure a url is a valid url and a valid image.
	/// </summary>
	public class ImageUrl
	{
		public Uri Url { get; private set; }
		public string FileType { get; private set; }

		public bool HasErrors { get; private set; }
		public ErrorReason ErrorReason { get; private set; }

		public ImageUrl(ICommandContext context, string text)
		{
			GetUrl(context, text);
			VerifyUrl();
			this.HasErrors = this.ErrorReason != null;
		}

		private void GetUrl(ICommandContext context, string text)
		{
			if (text != null)
			{
				if (!Uri.IsWellFormedUriString(text, UriKind.Absolute))
				{
					this.ErrorReason = new ErrorReason("Invalid Url provided.");
				}
				else
				{
					this.Url = new Uri(text);
				}
			}

			if (this.Url == null)
			{
				var attach = context.Message.Attachments.Where(x => x.Width != null && x.Height != null).Select(x => x.Url);
				var embeds = context.Message.Embeds.Where(x => x.Image.HasValue).Select(x => x.Image?.Url);
				var imageUrls = attach.Concat(embeds);
				if (!imageUrls.Any())
				{
					this.Url = null;
				}
				else if (imageUrls.Count() == 1)
				{
					this.Url = new Uri(imageUrls.First());
				}
				else
				{
					this.ErrorReason = new ErrorReason("Too many attached or embedded images.");
				}
			}
		}
		private void VerifyUrl()
		{
			if (this.Url != null)
			{
				var req = WebRequest.Create(this.Url);
				req.Method = WebRequestMethods.Http.Head;
				using (var resp = req.GetResponse())
				{
					if (!Constants.VALID_IMAGE_EXTENSIONS.Contains(this.FileType = "." + resp.Headers.Get("Content-Type").Split('/').Last()))
					{
						this.ErrorReason = new ErrorReason("Image must be a png or jpg.");
					}
					else if (!int.TryParse(resp.Headers.Get("Content-Length"), out int ContentLength))
					{
						this.ErrorReason = new ErrorReason("Unable to get the image's file size.");
					}
					else if (ContentLength > Constants.MAX_ICON_FILE_SIZE)
					{
						var maxSize = (double)Constants.MAX_ICON_FILE_SIZE / 1000 * 1000;
						this.ErrorReason = new ErrorReason($"Image is bigger than {maxSize:0.0}MB. Manually upload instead.");
					}
					else
					{
						this.ErrorReason = null;
					}
				}
			}
		}
	}
}
