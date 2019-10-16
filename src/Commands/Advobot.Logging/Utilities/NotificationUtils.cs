using System.Threading.Tasks;
using Advobot.Classes;
using Advobot.Logging.ReadOnlyModels;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;

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
				?.CaseInsReplace(USER_MENTION, user?.Mention ?? "Invalid User")
				?.CaseInsReplace(USER_STRING, user?.Format() ?? "Invalid User");
			var embed = notification.BuildWrapper();
			return await MessageUtils.SendMessageAsync(channel, content, embed).CAF();
		}
	}
}