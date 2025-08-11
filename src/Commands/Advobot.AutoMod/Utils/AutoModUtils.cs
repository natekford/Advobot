using Advobot.AutoMod.Models;
using Advobot.Utilities;

using Discord;

using System.Text.RegularExpressions;

namespace Advobot.AutoMod.Utils;

public static class AutoModUtils
{
	public static bool IsMatch(this BannedPhrase phrase, string content)
	{
		if (phrase.IsRegex)
		{
			return Regex.IsMatch(content, phrase.Phrase);
		}
		else if (phrase.IsContains)
		{
			return content.CaseInsContains(phrase.Phrase);
		}
		return content.CaseInsEquals(phrase.Phrase);
	}

	public static ValueTask<bool> ShouldScanMessageAsync(
		this AutoModSettings settings,
		IMessage message,
		TimeSpan ts)
	{
		if (message.Author is not IGuildUser user)
		{
			return new(false);
		}
		else if (settings.IgnoreAdmins && user.GuildPermissions.Administrator)
		{
			return new(false);
		}
		else if (settings.CheckDuration && ts > settings.Duration)
		{
			return new(false);
		}
		else if (!settings.IgnoreHigherHierarchy)
		{
			return new(false);
		}

		static async ValueTask<bool> CheckHierarchyAsync(IGuildUser user)
		{
			var bot = await user.Guild.GetCurrentUserAsync().ConfigureAwait(false);
			return bot.CanModify(user);
		}

		return CheckHierarchyAsync(user);
	}
}