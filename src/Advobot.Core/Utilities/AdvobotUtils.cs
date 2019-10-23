using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Resources;
using Advobot.Services;
using Advobot.Services.BotSettings;
using Advobot.Services.GuildSettings;
using Advobot.Settings;
using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Utilities
{
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
		public static IServiceCollection AddDefaultOptionsSetter<T>(this IServiceCollection services)
			where T : class, IDefaultOptionsSetter
		{
			return services
				.AddSingleton<T>()
				.AddSingleton<IDefaultOptionsSetter>(x => x.GetRequiredService<T>());
		}

		/// <summary>
		/// Returns a UTC <see cref="DateTimeOffset"/>.
		/// </summary>
		/// <param name="ticks"></param>
		/// <returns></returns>
		public static DateTimeOffset CreateUtcDTOFromTicks(this long ticks)
			=> DateTime.SpecifyKind(new DateTime(ticks), DateTimeKind.Utc);

		/// <summary>
		/// Gets the file inside the bot directory.
		/// </summary>
		/// <param name="accessor"></param>
		/// <param name="fileName">The name of the file without the bot directory.</param>
		/// <returns></returns>
		public static FileInfo GetBaseBotDirectoryFile(this IBotDirectoryAccessor accessor, string fileName)
			=> new FileInfo(Path.Combine(accessor.BaseBotDirectory.FullName, fileName));

		/// <summary>
		/// Gets the prefix to use for this guild. Prioritizes the guild prefix over the global prefix.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="botSettings"></param>
		/// <returns></returns>
		public static string GetPrefix(this IGuildSettings settings, IBotSettings botSettings)
			=> settings.Prefix ?? botSettings.Prefix ?? throw new InvalidOperationException("Invalid prefix.");

		/// <summary>
		/// Calls <see cref="ResourceManager.GetString(string)"/> and throws an exception if it does not exist.
		/// </summary>
		/// <param name="resources"></param>
		/// <param name="name"></param>
		/// <returns></returns>
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

		/// <summary>
		/// Gets the values of an enum.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static IReadOnlyList<T> GetValues<T>() where T : Enum
		{
			var uncast = Enum.GetValues(typeof(T));
			var cast = new T[uncast.Length];
			for (var i = 0; i < uncast.Length; ++i)
			{
				cast[i] = (T)uncast.GetValue(i);
			}
			return cast;
		}

		/// <summary>
		/// Ensures the extension of the file is '.db' and that the directory exists.
		/// </summary>
		/// <param name="accessor"></param>
		/// <param name="fileNameParts"></param>
		/// <returns></returns>
		public static FileInfo ValidateDbPath(this IBotDirectoryAccessor accessor, params string[] fileNameParts)
		{
			static void ExtensionValidation(ref string fileName)
			{
				const string EXT = ".db";
				if (!Path.HasExtension(fileName))
				{
					fileName += EXT;
				}
				else if (Path.GetExtension(fileName) != EXT)
				{
					fileName = Path.GetFileNameWithoutExtension(fileName) + EXT;
				}
			}
			ExtensionValidation(ref fileNameParts[^1]);

			var relativePath = Path.Combine(fileNameParts);
			var absolutePath = accessor.GetBaseBotDirectoryFile(relativePath).FullName;
			//Make sure the directory the db will be created in exists
			Directory.CreateDirectory(Path.GetDirectoryName(absolutePath));
			return new FileInfo(absolutePath);
		}
	}
}