using Advobot.Services;
using Advobot.Settings;

using Microsoft.Extensions.DependencyInjection;

using System.Globalization;
using System.Resources;

namespace Advobot.Utilities;

/// <summary>
/// Random utilities.
/// </summary>
public static class AdvobotUtils
{
	/// <summary>
	/// Adds a default options setter.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="services"></param>
	/// <returns></returns>
	public static IServiceCollection AddDefaultOptionsSetter<T>(
		this IServiceCollection services)
		where T : class, IResetter
	{
		return services
			.AddSingleton<T>()
			.AddSingleton<IResetter>(x => x.GetRequiredService<T>());
	}

	/// <summary>
	/// Gets the file inside the bot directory.
	/// </summary>
	/// <param name="accessor"></param>
	/// <param name="fileName">The name of the file without the bot directory.</param>
	/// <returns></returns>
	public static FileInfo GetBaseBotDirectoryFile(this IBotDirectoryAccessor accessor, string fileName)
		=> new(Path.Combine(accessor.BaseBotDirectory.FullName, fileName));

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