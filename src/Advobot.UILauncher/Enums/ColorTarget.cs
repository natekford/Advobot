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
		ButtonForeground = (1U << 4),
		ButtonBorder = (1U << 5),
		ButtonDisabledBackground = (1U << 6),
		ButtonDisabledForeground = (1U << 7),
		ButtonDisabledBorder = (1U << 8),
		ButtonMouseOverBackground = (1U << 9),
		JsonDigits = (1U << 10),
		JsonValue = (1U << 11),
		JsonParamName = (1U << 12)
	}
}
