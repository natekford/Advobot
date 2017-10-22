using Advobot.UILauncher.Enums;
using System.IO;

namespace Advobot.UILauncher.Classes
{
	/// <summary>
	/// Used in the file menu to list the information about a file.
	/// </summary>
	public struct FileInformation
	{
		public FileType FileType { get; }
		public FileInfo FileInfo { get; }

		public FileInformation(FileType fileType, FileInfo fileInfo)
		{
			FileType = fileType;
			FileInfo = fileInfo;
		}
	}
}
