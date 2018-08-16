using System;

namespace Advobot.NetFrameworkUI.Enums
{
	[Flags]
	internal enum Modal : uint
	{
		OutputSearch = (1U << 1),
		FileViewing = (1U << 2)
	}
}
