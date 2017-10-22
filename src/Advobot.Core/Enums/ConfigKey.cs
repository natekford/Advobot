using System;

namespace Advobot.Core.Enums
{
	/// <summary>
	/// Keys to be used in <see cref="Config.ConfigDict"/>.
	/// </summary>
	[Flags]
	public enum ConfigKey : uint
	{
		SavePath = (1U << 0),
		BotKey = (1U << 1),
		BotId = (1U << 2),
	}
}
