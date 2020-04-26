using System;
using System.Collections.Generic;

using Advobot.Services.GuildSettings.Settings;

using Discord;

namespace Advobot.AutoMod.ReadOnlyModels
{
	public interface IReadOnlySpamPrevention : IReadOnlyPunishment
	{
		bool Enabled { get; }
		TimeSpan Interval { get; }
		int Size { get; }
		SpamType SpamType { get; }

		bool IsSpam(IMessage message);

		bool ShouldPunish(IEnumerable<ulong> messages);
	}
}