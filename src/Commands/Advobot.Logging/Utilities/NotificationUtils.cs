using System.Threading.Tasks;

using Advobot.Classes;
using Advobot.Logging.Models;
using Advobot.Utilities;

using AdvorangesUtils;

using Discord;

using static Advobot.Resources.Responses;

namespace Advobot.Logging.Utilities
{
	public static class NotificationUtils
	{
		public const string USER_MENTION = "%USERMENTION%";
		public const string USER_STRING = "%USER%";

		public static EmbedWrapper BuildWrapper(this Models.CustomEmbed custom)
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

		public static async Task<IUserMessage?> SendAsync(
			this CustomNotification notification,
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
			var embed = notification.EmbedEmpty() ? null : notification.BuildWrapper();
			return await channel.SendMessageAsync(new SendMessageArgs
			{
				Content = content,
				Embed = embed,
				AllowedMentions = AllowedMentions.All,
			}).CAF();
		}

		public static SendMessageArgs ToMessageArgs(this EmbedWrapper embed)
		{
			return new SendMessageArgs
			{
				Embed = embed,
				AllowedMentions = new AllowedMentions(AllowedMentionTypes.Users),
			};
		}
	}
}