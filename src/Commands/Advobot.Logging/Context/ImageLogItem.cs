using AdvorangesUtils;

using Discord;

namespace Advobot.Logging.Context;

public readonly struct ImageLogItem
{
	public string Footer { get; }
	public string? ImageUrl { get; }
	public string Url { get; }

	private ImageLogItem(string footer, string url, string? imageUrl)
	{
		Footer = footer;
		Url = url;
		ImageUrl = imageUrl;
	}

	public static ImageLogItem FromAttachment(IAttachment attachment)
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
		return new(footer, url, imageUrl);
	}

	public static ImageLogItem? FromEmbed(IEmbed embed)
	{
		if (embed.Video is EmbedVideo video)
		{
			var thumb = embed.Thumbnail?.Url ?? GetVideoThumbnail(video.Url);
			return new("Video", embed.Url, thumb);
		}

		var img = embed.Image?.Url ?? embed.Thumbnail?.Url;
		if (img == null)
		{
			return null;
		}
		return new("Image", embed.Url, img);
	}

	public static IEnumerable<ImageLogItem> GetAllImages(IMessage message)
	{
		foreach (var group in message.Attachments.GroupBy(x => x.Url))
		{
			yield return FromAttachment(group.First());
		}
		foreach (var group in message.Embeds.GroupBy(x => x.Url))
		{
			var item = FromEmbed(group.First());
			if (item.HasValue)
			{
				yield return item.Value;
			}
		}
	}

	private static string GetVideoThumbnail(string url)
	{
		var replaced = url.Replace("//cdn.discordapp.com/", "//media.discordapp.net/");
		return replaced + "?format=jpeg&width=241&height=241";
	}
}