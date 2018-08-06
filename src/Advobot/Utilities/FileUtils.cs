using System.IO;

namespace Advobot.Utilities
{
	/// <summary>
	/// Actions for getting files related to the bot.
	/// </summary>
	public static class FileUtils
	{
		/// <summary>
		/// Makes sure the directory and file exists then writes the text to the file.
		/// </summary>
		/// <param name="file"></param>
		/// <param name="text"></param>
		public static void SafeWriteAllText(FileInfo file, string text)
		{
			//Don't use file.Exists because the property sometimes isn't updated.
			if (!File.Exists(file.FullName))
			{
				Directory.CreateDirectory(Path.GetDirectoryName(file.FullName));
				using (var fs = file.Create())
				{
					fs.Close();
				}
			}
			File.WriteAllText(file.FullName, text);
		}
	}
}
