using System;
using System.Linq;
using System.Threading.Tasks;

using Advobot.AutoMod.ReadOnlyModels;
using Advobot.Classes;
using Advobot.Utilities;

using Discord;

namespace Advobot.AutoMod.Utils
{
	public static class AutoModUtils
	{
		public static int GetImageCount(this IMessage message)
		{
			var attachments = message.Attachments
				.Count(x => x.Height != null || x.Width != null);
			var embeds = message.Embeds
				.Count(x => x.Image != null || x.Video != null);
			return attachments + embeds;
		}

		public static int GetLinkCount(this IMessage message)
		{
			if (string.IsNullOrWhiteSpace(message.Content))
			{
				return 0;
			}

			return message.Content
				.Split(' ')
				.Where(x => Uri.IsWellFormedUriString(x, UriKind.RelativeOrAbsolute))
				.Distinct()
				.Count();
		}

		public static Task GiveAsync(
			this PunishmentManager punisher,
			IReadOnlyPunishment punishment,
			AmbiguousUser user,
			RequestOptions? options = null)
		{
			var args = new PunishmentArgs
			{
				Time = punishment.Length,
				Options = options,
				Role = punisher.Guild.GetRole(punishment.RoleId ?? 0),
			};
			return punisher.GiveAsync(punishment.PunishmentType, user, args);
		}
	}
}