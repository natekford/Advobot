namespace Advobot;

/// <summary>
/// Base interface for a bot configuration object.
/// </summary>
public interface IConfig
{
	/// <summary>
	/// Where all the files of the bot are stored.
	/// </summary>
	DirectoryInfo BaseBotDirectory { get; }

	/// <summary>
	/// Arguments to use when the bot is restart.
	/// </summary>
	string RestartArguments { get; }
}