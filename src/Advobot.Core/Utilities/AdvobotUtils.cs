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