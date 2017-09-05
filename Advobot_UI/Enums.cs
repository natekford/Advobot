using System;

namespace Advobot.Graphics
{
	/// <summary>
	/// Used in determining what menu to open, and if to close the menu instead.
	/// </summary>
	[Flags]
	internal enum MenuType : uint
	{
		Main							= (1U << 0),
		Info							= (1U << 1),
		Settings						= (1U << 2),
		Colors							= (1U << 3),
		Files							= (1U << 4),
	}

	/// <summary>
	/// Used to know what text to display on the temporary tooltip.
	/// </summary>
	[Flags]
	internal enum ToolTipReason : uint
	{
		FileSavingFailure				= (1U << 0),
		FileSavingSuccess				= (1U << 1),
		InvalidFilePath					= (1U << 2),
	}

	/// <summary>
	/// Specifies what file to open in the guild file search menu.
	/// </summary>
	[Flags]
	internal enum FileType : uint
	{
		GuildSettings					= (1U << 0),
	}
}
