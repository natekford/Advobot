using Advobot.AutoMod.ReadOnlyModels;

namespace Advobot.AutoMod.Models
{
	public sealed class SelfRole : IReadOnlySelfRole
	{
		public int GroupId { get; set; }
		public ulong GuildId { get; set; }
		public ulong RoleId { get; set; }

		public SelfRole()
		{
		}

		public SelfRole(IReadOnlySelfRole other)
		{
			GuildId = other.GuildId;
			RoleId = other.RoleId;
			GroupId = other.GroupId;
		}
	}
}