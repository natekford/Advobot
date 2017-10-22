using System;

namespace Advobot.UILauncher.Enums
{
	/// <summary>
	/// Used to know what text to display on the temporary tooltip.
	/// </summary>
	[Flags]
	internal enum ToolTipReason : uint
	{
		FileSavingFailure = (1U << 0),
		FileSavingSuccess = (1U << 1),
		InvalidFilePath = (1U << 2),
	}
}
