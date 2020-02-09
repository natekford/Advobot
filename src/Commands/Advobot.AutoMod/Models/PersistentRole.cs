using Advobot.AutoMod.ReadOnlyModels;
using Advobot.Databases.Relationships;
using Advobot.Utilities;

namespace Advobot.AutoMod.Models
{
	public sealed class PersistentRole : IReadOnlyPersistentRole
	{
		public string GuildId { get; set; } = null!;
		public string RoleId { get; set; } = null!;
		public string UserId { get; set; } = null!;

		ulong IGuildChild.GuildId => GuildId.ToId();
		ulong IReadOnlyPersistentRole.RoleId => RoleId.ToId();
		ulong IUserChild.UserId => UserId.ToId();
	}
}