using System;

namespace Advobot.UILauncher.Enums
{
	[Flags]
	internal enum ColorTheme : uint
	{
		Classic = (1U << 0),
		DarkMode = (1U << 1),
		UserMade = (1U << 2),
	}
}
