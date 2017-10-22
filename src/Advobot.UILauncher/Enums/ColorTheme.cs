using System;

namespace Advobot.UILauncher.Enums
{
	[Flags]
	internal enum ColorTheme : uint
	{
		Classic = (1U << 0),
		Dark_Mode = (1U << 1),
		User_Made = (1U << 2),
	}
}
