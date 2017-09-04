using Advobot.Enums;
using System.IO;

namespace Advobot.Graphics
{
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
