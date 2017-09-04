using System;

namespace Advobot.Graphics
{
	[Flags]
	internal enum MenuType : uint
	{
		Main = (1U << 0),
		Info = (1U << 1),
		Settings = (1U << 2),
		Colors = (1U << 3),
		Files = (1U << 4),
	}

	[Flags]
	internal enum ToolTipReason : uint
	{
		FileSavingFailure = (1U << 0),
		FileSavingSuccess = (1U << 1),
		InvalidFilePath = (1U << 2),
	}
}
