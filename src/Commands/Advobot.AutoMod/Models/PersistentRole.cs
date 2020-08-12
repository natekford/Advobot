using Advobot.AutoMod.ReadOnlyModels;

namespace Advobot.AutoMod.Models
{
	public sealed class PersistentRole : IReadOnlyPersistentRole
	{
		public ulong GuildId { get; set; }
		public ulong RoleId { get; set; }
		public ulong UserId { get; set; }
	}
}