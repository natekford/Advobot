using System;

namespace Advobot.UILauncher.Enums
{
	[Flags]
    internal enum Modal : uint
    {
		FileSearch = (1U << 0),
		OutputSearch = (1U << 1),
    }
}
