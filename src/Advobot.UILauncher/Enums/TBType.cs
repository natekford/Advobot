using System;

namespace Advobot.UILauncher.Enums
{
	[Flags]
	public enum TBType : uint
	{
		Nothing = 0,
		Title = (1U << 0),
		RightCentered = (1U << 1),
		LeftCentered = (1U << 2),
		CenterCentered = (1U << 3),
		Background = (1U << 4),
	}
}
