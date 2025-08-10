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

/// <summary>
/// Utilities for <see cref="IConfig"/>.
/// </summary>
public static class ConfigUtils
{
	/// <summary>
	/// Gets the file inside the bot directory.
	/// </summary>
	/// <param name="config"></param>
	/// <param name="fileName">The name of the file without the bot directory.</param>
	/// <returns></returns>
	public static FileInfo GetFile(this IConfig config, string fileName)
		=> new(Path.Combine(config.BaseBotDirectory.FullName, fileName));
}