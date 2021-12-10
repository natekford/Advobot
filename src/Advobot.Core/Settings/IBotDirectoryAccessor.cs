namespace Advobot.Settings;

/// <summary>
/// Abstraction for something holding the paths to where the bot files are stored.
/// </summary>
public interface IBotDirectoryAccessor
{
	/// <summary>
	/// Where all the files of the bot are stored.
	/// </summary>
	DirectoryInfo BaseBotDirectory { get; }
}