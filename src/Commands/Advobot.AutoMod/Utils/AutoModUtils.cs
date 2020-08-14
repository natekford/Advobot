using System;
using System.Collections.Generic;
using System.Linq;

using Advobot.AutoMod.ReadOnlyModels;
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

		public static bool IsSpam(this IReadOnlySpamPrevention prevention, IMessage message)
		{
			return prevention.SpamType switch
			{
				SpamType.Message => int.MaxValue,
				SpamType.LongMessage => message.Content?.Length ?? 0,
				SpamType.Link => message.GetLinkCount(),
				SpamType.Image => message.GetImageCount(),
				SpamType.Mention => message.MentionedUserIds.Distinct().Count(),
				_ => throw new ArgumentOutOfRangeException(nameof(prevention.SpamType)),
			} > prevention.Size;
		}

		public static bool ShouldPunish(this IReadOnlySpamPrevention prevention, IEnumerable<ulong> messages)
			=> messages.CountItemsInTimeFrame(prevention.Interval) > prevention.Instances;
	}
}