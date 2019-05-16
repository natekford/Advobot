using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Advobot.Enums;
using Advobot.Interfaces;

namespace Advobot.Utilities
{
	/// <summary>
	/// Random utilities.
	/// </summary>
	public static class AdvobotUtils
	{
		/// <summary>
		/// Gets the file inside the bot directory.
		/// </summary>
		/// <param name="accessor"></param>
		/// <param name="fileName"></param>
		/// <returns></returns>
		public static FileInfo GetBaseBotDirectoryFile(this IBotDirectoryAccessor accessor, string fileName)
			=> new FileInfo(Path.Combine(accessor.BaseBotDirectory.FullName, fileName));
		/// <summary>
		/// Gets the path of the object which implements both <see cref="IBotDirectoryAccessor"/> and <see cref="ISettingsBase"/>.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="obj"></param>
		/// <returns></returns>
		public static FileInfo GetFile<T>(this T obj) where T : IBotDirectoryAccessor, ISettingsBase
			=> obj.GetFile(obj);
		/// <summary>
		/// Saves the settings of the object which implements both <see cref="IBotDirectoryAccessor"/> and <see cref="ISettingsBase"/>.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="obj"></param>
		public static void SaveSettings<T>(this T obj) where T : IBotDirectoryAccessor, ISettingsBase
			=> obj.Save(obj);
		/// <summary>
		/// Checks whether to use the bot prefix, or the guild settings prefix.
		/// </summary>
		/// <param name="b"></param>
		/// <param name="g"></param>
		/// <returns></returns>
		public static string GetPrefix(this IBotSettings b, IGuildSettings g)
			=> g == null || string.IsNullOrWhiteSpace(g.Prefix) ? b.Prefix : g.Prefix;
		/// <summary>
		/// Joins the strings together after selecting them.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="source"></param>
		/// <param name="seperator"></param>
		/// <param name="selector"></param>
		/// <returns></returns>
		public static string Join<T>(this IEnumerable<T> source, string seperator, Func<T, string> selector)
			=> string.Join(seperator, source.Select(selector));
		/// <summary>
		/// Joins the strings together with the seperator.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="seperator"></param>
		/// <returns></returns>
		public static string Join(this IEnumerable<string> source, string seperator)
			=> string.Join(seperator, source);
		/// <summary>
		/// Attempts to get the first matching value. Will return default if no matches are found.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="source"></param>
		/// <param name="predicate"></param>
		/// <param name="found"></param>
		/// <returns></returns>
		public static bool TryGetFirst<T>(this IEnumerable<T> source, Func<T, bool> predicate, out T found)
		{
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference or unconstrained type parameter.
			found = default;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference or unconstrained type parameter.
			foreach (var item in source)
			{
				if (predicate(item))
				{
					found = item;
					return true;
				}
			}
			return false;
		}
		/// <summary>
		/// Attempts to get a single matching value. Will throw if more than one match is found. Will return default if no matches are found.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="source"></param>
		/// <param name="predicate"></param>
		/// <param name="found"></param>
		/// <returns></returns>
		public static bool TryGetSingle<T>(this IEnumerable<T> source, Func<T, bool> predicate, out T found)
		{
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference or unconstrained type parameter.
			found = default;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference or unconstrained type parameter.
			var matched = false;
			foreach (var item in source)
			{
				if (predicate(item))
				{
					if (matched)
					{
						throw new InvalidOperationException("More than one match found.");
					}
					found = item;
					matched = true;
				}
			}
			return matched;
		}
		/// <summary>
		/// Returns objects where the function does not return null and is either equal to, less than, or greater than a specified number.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="objects"></param>
		/// <param name="method"></param>
		/// <param name="number"></param>
		/// <param name="f"></param>
		/// <returns></returns>
		public static IEnumerable<T> GetFromCount<T>(this IEnumerable<T> objects, CountTarget method, int? number, Func<T, int?> f) => method switch
		{
			CountTarget.Equal => objects.Where(x => f(x) == number),
			CountTarget.Below => objects.Where(x => f(x) < number),
			CountTarget.Above => objects.Where(x => f(x) > number),
			_ => throw new ArgumentException(nameof(method)),
		};
	}
}