using System;

namespace Advobot.Core.Enums
{
	/// <summary>
	/// Specifies which category a command is in.
	/// </summary>
	[Flags]
	public enum CommandCategory : uint
	{
		BotSettings = (1U << 0),
		GuildSettings = (1U << 1),
		Logs = (1U << 2),
		BanPhrases = (1U << 3),
		SelfRoles = (1U << 4),
		UserModeration = (1U << 5),
		RoleModeration = (1U << 6),
		ChannelModeration = (1U << 7),
		GuildModeration = (1U << 8),
		Miscellaneous = (1U << 9),
		SpamPrevention = (1U << 10),
		InviteModeration = (1U << 11),
		GuildList = (1U << 12),
		NicknameModeration = (1U << 13),
		Quotes = (1U << 14),
		Rules = (1U << 15),
		Gets = (1U << 16),
	}
}
