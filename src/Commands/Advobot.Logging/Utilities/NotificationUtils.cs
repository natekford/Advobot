using System.Threading.Tasks;

using Advobot.Classes;
using Advobot.Logging.ReadOnlyModels;
using Advobot.Utilities;

using AdvorangesUtils;

using Discord;

using static Advobot.Logging.Resources.Responses;

namespace Advobot.Logging.Utilities
{
	public static class NotificationUtils
	{
		public const string USER_MENTION = "%USERMENTION%";
		public const string USER_STRING = "%USER%";

		public static EmbedWrapper BuildWrapper(this IReadOnlyCustomEmbed custom)
		{
			var embed = new EmbedWrapper
			{
				Title = custom.Title,
				Description = custom.Description,
				Color = new Color(custom.Color),
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

		public static bool HasAtleastOneNonNullProperty(this IReadOnlyCustomEmbed custom)
		{
			return custom.AuthorIconUrl != null
				|| custom.AuthorName != null
				|| custom.AuthorUrl != null
				|| custom.Color != 0
				|| custom.Description != null
				|| custom.Footer != null
				|| custom.FooterIconUrl != null
				|| custom.ImageUrl != null
				|| custom.ThumbnailUrl != null
				|| custom.Title != null
				|| custom.Url != null;
		}

		public static async Task<IUserMessage?> SendAsync(
			this IReadOnlyCustomNotification notification,
			IGuild guild,
			IUser? user)
		{
			var channel = await guild.GetTextChannelAsync(notification.ChannelId).CAF();
			if (channel == null)
			{
				return null;
			}

			var content = notification.Content
				?.CaseInsReplace(USER_MENTION, user?.Mention ?? VariableInvalidUser)
				?.CaseInsReplace(USER_STRING, user?.Format() ?? VariableInvalidUser);
			var embed = notification.BuildWrapper();
			return await MessageUtils.SendMessageAsync(channel, content, embed).CAF();
		}
	}
}