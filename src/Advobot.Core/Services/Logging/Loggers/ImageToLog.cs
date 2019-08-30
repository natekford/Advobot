using System.IO;

using AdvorangesUtils;

using Discord;

namespace Advobot.Services.Logging.Loggers
{
	internal readonly struct ImageToLog
	{
		private const string ANIMATED = nameof(ILogService.Animated);
		private const string FILES = nameof(ILogService.Files);
		private const string IMAGES = nameof(ILogService.Images);

		public string Footer { get; }

		public string? ImageUrl { get; }

		public string Name { get; }

		public string Url { get; }

		private ImageToLog(string name, string footer, string url, string? imageUrl)
		{
			Name = name;
			Footer = footer;
			Url = url;
			ImageUrl = imageUrl;
		}

		public static ImageToLog FromAttachment(IAttachment attachment)
		{
			var url = attachment.Url;
			var ext = MimeTypes.MimeTypeMap.GetMimeType(Path.GetExtension(url));
			var (name, footer, imageUrl) = ext switch
			{
				string s when s.CaseInsContains("/gif") => (ANIMATED, "Gif", GetVideoThumbnail(url)),
				string s when s.CaseInsContains("video/") => (ANIMATED, "Video", GetVideoThumbnail(url)),
				string s when s.CaseInsContains("image/") => (IMAGES, "Image", url),
				_ => (FILES, "File", null),
			};
			return new ImageToLog(name, footer, url, imageUrl);
		}

		public static ImageToLog? FromEmbed(IEmbed embed)
		{
			if (embed.Video is EmbedVideo video)
			{
				var thumb = embed.Thumbnail?.Url ?? GetVideoThumbnail(video.Url);
				return new ImageToLog(ANIMATED, "Video", embed.Url, thumb);
			}

			var img = embed.Image?.Url ?? embed.Thumbnail?.Url;
			if (img == null)
			{
				return null;
			}
			return new ImageToLog(IMAGES, "Image", embed.Url, img);
		}

		private static string GetVideoThumbnail(string url)
		{
			var replaced = url.Replace("//cdn.discordapp.com/", "//media.discordapp.net/");
			return replaced + "?format=jpeg&width=241&height=241";
		}
	}
}