using System;

namespace Advobot.UILauncher.Enums
{
	[Flags]
	internal enum ColorTarget : uint
	{
		Base_Background = (1U << 0),
		Base_Foreground = (1U << 1),
		Base_Border = (1U << 2),
		Button_Background = (1U << 3),
		Button_Border = (1U << 4),
		Button_Disabled_Background = (1U << 5),
		Button_Disabled_Foreground = (1U << 6),
		Button_Disabled_Border = (1U << 7),
		Button_Mouse_Over_Background = (1U << 8),
	}
}
