using System.Collections.Generic;

using Advobot.Services.GuildSettings.Settings;

using Discord;

namespace Advobot.AutoMod.ReadOnlyModels
{
	public interface IReadOnlySpamPrevention : IReadOnlyTimedPrevention
	{
		SpamType SpamType { get; }

		bool IsSpam(IMessage message);

		bool ShouldPunish(IEnumerable<ulong> messages);
	}
}