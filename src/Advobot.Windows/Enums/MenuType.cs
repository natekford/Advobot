using System;

namespace Advobot.Windows.Enums
{
	/// <summary>
	/// Used in determining what menu to open, and if to close the menu instead.
	/// </summary>
	[Flags]
	internal enum MenuType : uint
	{
		Main = (1U << 0),
		Info = (1U << 1),
		Settings = (1U << 2),
		Colors = (1U << 3),
	}
}