using System.IO;

using AdvorangesUtils;

using Discord;

namespace Advobot.Logging.Context
{
	public sealed class ImageLoggingContext
	{
		public string Footer { get; }
		public string? ImageUrl { get; }
		public string Url { get; }

		private ImageLoggingContext(string footer, string url, string? imageUrl)
		{
			Footer = footer;
			Url = url;
			ImageUrl = imageUrl;
		}

		public static ImageLoggingContext FromAttachment(IAttachment attachment)
		{
			var url = attachment.Url;
			var ext = MimeTypes.MimeTypeMap.GetMimeType(Path.GetExtension(url));
			var (footer, imageUrl) = ext switch
			{
				string s when s.CaseInsContains("/gif") => ("Gif", GetVideoThumbnail(url)),
				string s when s.CaseInsContains("video/") => ("Video", GetVideoThumbnail(url)),
				string s when s.CaseInsContains("image/") => ("Image", url),
				_ => ("File", null),
			};
			return new ImageLoggingContext(footer, url, imageUrl);
		}

		public static ImageLoggingContext? FromEmbed(IEmbed embed)
		{
			if (embed.Video is EmbedVideo video)
			{
				var thumb = embed.Thumbnail?.Url ?? GetVideoThumbnail(video.Url);
				return new ImageLoggingContext("Video", embed.Url, thumb);
			}

			var img = embed.Image?.Url ?? embed.Thumbnail?.Url;
			if (img == null)
			{
				return null;
			}
			return new ImageLoggingContext("Image", embed.Url, img);
		}

		private static string GetVideoThumbnail(string url)
		{
			var replaced = url.Replace("//cdn.discordapp.com/", "//media.discordapp.net/");
			return replaced + "?format=jpeg&width=241&height=241";
		}
	}
}