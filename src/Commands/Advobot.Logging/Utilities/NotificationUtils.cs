using Advobot.Embeds;
using Advobot.Logging.Database.Models;
using Advobot.Utilities;

using Discord;

namespace Advobot.Logging.Utilities;

public static class NotificationUtils
{
	public const string USER_MENTION = "%USERMENTION%";
	public const string USER_STRING = "%USER%";
	public static AllowedMentions UserMentions { get; } = new(AllowedMentionTypes.Users);

	public static EmbedWrapper BuildWrapper(this CustomEmbed custom)
	{
		var embed = new EmbedWrapper
		{
			Title = custom.Title,
			Description = custom.Description,
			Color = new(custom.Color),
			ImageUrl = custom.ImageUrl?.ToString(),
			Url = custom.Url?.ToString(),
			ThumbnailUrl = custom.ThumbnailUrl?.ToString(),
		};
		embed.TrySetAuthor(custom.AuthorName, custom.AuthorUrl?.ToString(), custom.AuthorIconUrl?.ToString(), out _);
		embed.TrySetFooter(custom.Footer, custom.FooterIconUrl?.ToString(), out _);

		return embed;
	}

	public static bool EmbedEmpty(this CustomEmbed custom)
	{
		return custom.AuthorIconUrl is null
			&& custom.AuthorName is null
			&& custom.AuthorUrl is null
			&& custom.Color.RawValue is 0
			&& custom.Description is null
			&& custom.Footer is null
			&& custom.FooterIconUrl is null
			&& custom.ImageUrl is null
			&& custom.ThumbnailUrl is null
			&& custom.Title is null
			&& custom.Url is null;
	}

	public static SendMessageArgs ToMessageArgs(this EmbedWrapper embed)
	{
		return new(embed)
		{
			AllowedMentions = UserMentions,
		};
	}
}