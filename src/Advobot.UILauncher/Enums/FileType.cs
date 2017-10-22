using System;

namespace Advobot.UILauncher.Enums
{
	/// <summary>
	/// Specifies what file to open in the guild file search menu.
	/// </summary>
	[Flags]
	public enum FileType : uint
	{
		GuildSettings = (1U << 0),
	}
}
