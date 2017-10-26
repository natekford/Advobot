using System;

namespace Advobot.Core.Enums
{
	[Flags]
	public enum BotSetting : uint
	{
		AlwaysDownloadUsers = (1U << 0),
		Prefix = (1U << 1),
		Game = (1U << 2),
		Stream = (1U << 3),
		ShardCount = (1U << 4),
		MessageCacheCount = (1U << 5),
		MaxUserGatherCount = (1U << 6),
		MaxMessageGatherSize = (1U << 7),
		LogLevel = (1U << 8),
		TrustedUsers = (1U << 9),
		UsersUnableToDMOwner = (1U << 10),
		UsersIgnoredFromCommands = (1U << 11),
	}
}
