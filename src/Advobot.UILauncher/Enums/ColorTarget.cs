using System;

namespace Advobot.UILauncher.Enums
{
	[Flags]
	internal enum ColorTarget : uint
	{
		BaseBackground = (1U << 0),
		BaseForeground = (1U << 1),
		BaseBorder = (1U << 2),
		ButtonBackground = (1U << 3),
		ButtonBorder = (1U << 4),
		ButtonDisabledBackground = (1U << 5),
		ButtonDisabledForeground = (1U << 6),
		ButtonDisabledBorder = (1U << 7),
		ButtonMouseOverBackground = (1U << 8),
	}
}
