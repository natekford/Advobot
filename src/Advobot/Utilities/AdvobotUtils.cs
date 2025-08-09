using System.Diagnostics;
using System.Globalization;
using System.Resources;

namespace Advobot.Utilities;

/// <summary>
/// Random utilities.
/// </summary>
public static class AdvobotUtils
{
	/// <summary>
	/// The time the bot was started in UTC.
	/// </summary>
	public static DateTime StartTime { get; } = Process.GetCurrentProcess().StartTime.ToUniversalTime();

	/// <summary>
	/// Gets the file inside the bot directory.
	/// </summary>
	/// <param name="config"></param>
	/// <param name="fileName">The name of the file without the bot directory.</param>
	/// <returns></returns>
	public static FileInfo GetFile(this IConfig config, string fileName)
		=> new(Path.Combine(config.BaseBotDirectory.FullName, fileName));

	/// <summary>
	/// Calls <see cref="ResourceManager.GetString(string)"/> and throws an exception if it does not exist.
	/// </summary>
	/// <param name="resources"></param>
	/// <param name="name"></param>
	/// <returns></returns>
	/// <exception cref="ArgumentException"></exception>
	public static string GetStringEnsured(this ResourceManager resources, string name)
	{
		var r = resources.GetString(name);
		if (r != null)
		{
			return r;
		}
		var culture = CultureInfo.CurrentUICulture;
		var message = $"{name} does not have an associated string in the {culture} culture.";
		throw new ArgumentException(message, name);
	}
}