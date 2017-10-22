namespace Advobot.UILauncher.Classes
{
	/// <summary>
	/// Holds information about a guild in order to list them in the file menu.
	/// </summary>
	public struct GuildFileInformation
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
}
