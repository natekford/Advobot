using Advobot.SQLite.Relationships;

namespace Advobot.AutoMod.ReadOnlyModels
{
	public interface IReadOnlyPersistentRole : IGuildChild, IUserChild
	{
		ulong RoleId { get; }
	}
}