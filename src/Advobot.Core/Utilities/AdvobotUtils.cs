using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Resources;

using Advobot.Services;
using Advobot.Settings;

using Discord;

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
		public static IServiceCollection AddDefaultOptionsSetter<T>(
			this IServiceCollection services)
			where T : class, IResetter
		{
			return services
				.AddSingleton<T>()
				.AddSingleton<IResetter>(x => x.GetRequiredService<T>());
		}

		/// <summary>
		/// Counts how many times something has occurred within a given timeframe.
		/// Also modifies the queue by removing instances which are too old to matter (locks the source when doing so).
		/// Returns the listlength if seconds is less than 0 or the listlength is less than 2.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="time"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentException">When <paramref name="source"/> is not in order.</exception>
		public static int CountItemsInTimeFrame(this IEnumerable<ulong> source, TimeSpan? time)
		{
			ulong[] copy;
			lock (source)
			{
				copy = source.ToArray();
			}

			//No timeFrame given means that it's a timed prevention that doesn't check against time
			if (time == null || copy.Length < 2)
			{
				return copy.Length;
			}

			//If there is a timeFrame then that means to gather the highest amount of messages that are in the time frame
			var maxCount = 0;
			for (var i = 0; i < copy.Length; ++i)
			{
				//If the queue is out of order that kinda ruins the method
				if (i > 0 && copy[i - 1] > copy[i])
				{
					throw new ArgumentException("The queue must be in order from oldest to newest.", nameof(source));
				}

				var currentIterCount = 1;
				var iTime = SnowflakeUtils.FromSnowflake(copy[i]).UtcDateTime;
				for (var j = i + 1; j < copy.Length; ++j)
				{
					var jTime = SnowflakeUtils.FromSnowflake(copy[j]).UtcDateTime;
					if ((jTime - iTime) < time)
					{
						++currentIterCount;
						continue;
					}
					//Optimization by checking if the time difference between two numbers is too high to bother starting at j - 1
					var jMinOneTime = SnowflakeUtils.FromSnowflake(copy[j - 1]).UtcDateTime;
					if ((jTime - jMinOneTime) > time)
					{
						i = j + 1;
					}
					break;
				}
				maxCount = Math.Max(maxCount, currentIterCount);
			}

			return maxCount;
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
			=> new(Path.Combine(accessor.BaseBotDirectory.FullName, fileName));

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