using System;

namespace Advobot.Core.Enums
{
	/// <summary>
	/// Specifies which log channel to modify.
	/// </summary>
	[Flags]
	public enum LogChannelType : uint
	{
		Server = (1U << 0),
		Mod = (1U << 1),
		Image = (1U << 2)
	}
}
