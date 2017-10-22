using System;

namespace Advobot.Core.Enums
{
	/// <summary>
	/// Specifies what to target with a command. Discord entities, names, prefix, etc.
	/// </summary>
	[Flags]
	public enum Target : uint
	{
		Guild = (1U << 0),
		Channel = (1U << 1),
		Role = (1U << 2),
		User = (1U << 3),
		Emote = (1U << 4),
		Invite = (1U << 5),
		Bot = (1U << 6),
		Name = (1U << 7),
		Nickname = (1U << 8),
		Game = (1U << 9),
		Stream = (1U << 10),
		Topic = (1U << 11),
		Prefix = (1U << 12),
		Regex = (1U << 13),
		RuleCategory = (1U << 14),
		Rule = (1U << 15),
	}
}
