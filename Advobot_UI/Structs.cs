using System.IO;

namespace Advobot.Graphics
{
	/// <summary>
	/// Holds information about a guild in order to list them in the file menu.
	/// </summary>
	internal struct GuildFileInformation
	{
		public ulong Id { get; }
		public string Name { get; }
		public int MemberCount { get; }

		public GuildFileInformation(ulong id, string name, int memberCount)
		{
			Id = id;
			Name = name;
			MemberCount = memberCount;
		}
	}

	/// <summary>
	/// Used in the file menu to list the information about a file.
	/// </summary>
	internal struct FileInformation
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
