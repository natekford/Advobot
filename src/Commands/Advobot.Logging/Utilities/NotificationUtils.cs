using Advobot.Embeds;
using Advobot.Utilities;

using Discord;

namespace Advobot.Logging.Utilities;

public static class NotificationUtils
{
	public const string USER_MENTION = "%USERMENTION%";
	public const string USER_STRING = "%USER%";
	public static AllowedMentions UserMentions { get; } = new(AllowedMentionTypes.Users);

	public static EmbedWrapper BuildWrapper(this Models.CustomEmbed custom)
	{
		var embed = new EmbedWrapper
		{
			Title = custom.Title,
			Description = custom.Description,
			Color = new(custom.Color),
			ImageUrl = custom.ImageUrl,
			Url = custom.Url,
			ThumbnailUrl = custom.ThumbnailUrl,
		};
		embed.TryAddAuthor(custom.AuthorName, custom.AuthorUrl, custom.AuthorIconUrl, out _);
		embed.TryAddFooter(custom.Footer, custom.FooterIconUrl, out _);

		/*
		foreach (var field in FieldInfo)
		{
			embed.TryAddField(field.Name, field.Text, field.Inline, out _);
		}*/
		return embed;
	}

	public static bool EmbedEmpty(this Models.CustomEmbed custom)
	{
		return custom.AuthorIconUrl == null
			&& custom.AuthorName == null
			&& custom.AuthorUrl == null
			&& custom.Color == 0
			&& custom.Description == null
			&& custom.Footer == null
			&& custom.FooterIconUrl == null
			&& custom.ImageUrl == null
			&& custom.ThumbnailUrl == null
			&& custom.Title == null
			&& custom.Url == null;
	}

	public static SendMessageArgs ToMessageArgs(this EmbedWrapper embed)
	{
		return new(embed)
		{
			AllowedMentions = UserMentions,
		};
	}
}