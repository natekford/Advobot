using System.IO;
using Advobot.Interfaces;

namespace Advobot.Utilities
{
	/// <summary>
	/// Random utilities.
	/// </summary>
	public static class Utils
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
			=> obj.SaveSettings(obj);
	}
}