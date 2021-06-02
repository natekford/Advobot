using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Advobot.AutoMod.Models;
using Advobot.Utilities;

using AdvorangesUtils;

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

		public static bool IsMatch(this BannedPhrase phrase, string content)
		{
			if (phrase.IsRegex)
			{
				return RegexUtils.IsMatch(content, phrase.Phrase);
			}
			else if (phrase.IsContains)
			{
				return content.CaseInsContains(phrase.Phrase);
			}
			return content.CaseInsEquals(phrase.Phrase);
		}

		public static bool IsSpam(this SpamPrevention prevention, IMessage message)
		{
			return prevention.SpamType switch
			{
				SpamType.Message => int.MaxValue,
				SpamType.LongMessage => message.Content?.Length ?? 0,
				SpamType.Link => message.GetLinkCount(),
				SpamType.Image => message.GetImageCount(),
				SpamType.Mention => message.MentionedUserIds.Distinct().Count(),
				_ => throw new ArgumentOutOfRangeException(nameof(prevention)),
			} > prevention.Size;
		}

		public static bool ShouldPunish(this SpamPrevention prevention, IEnumerable<ulong> messages)
			=> messages.CountItemsInTimeFrame(prevention.Interval) > prevention.Instances;

		public static ValueTask<bool> ShouldScanMessageAsync(
			this AutoModSettings settings,
			IMessage message,
			TimeSpan ts)
		{
			if (message.Author is not IGuildUser user)
			{
				return new ValueTask<bool>(false);
			}
			else if (settings.IgnoreAdmins && user.GuildPermissions.Administrator)
			{
				return new ValueTask<bool>(false);
			}
			else if (settings.CheckDuration && ts > settings.Duration)
			{
				return new ValueTask<bool>(false);
			}
			else if (!settings.IgnoreHigherHierarchy)
			{
				return new ValueTask<bool>(false);
			}

			static async ValueTask<bool> CheckHierarchyAsync(IGuildUser user)
			{
				var bot = await user.Guild.GetCurrentUserAsync().CAF();
				return bot.CanModify(user);
			}

			return CheckHierarchyAsync(user);
		}
	}
}