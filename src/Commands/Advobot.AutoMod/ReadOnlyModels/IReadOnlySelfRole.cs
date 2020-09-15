using Advobot.SQLite.Relationships;

namespace Advobot.AutoMod.ReadOnlyModels
{
	public interface IReadOnlySelfRole : IGuildChild
	{
		int GroupId { get; }
		ulong RoleId { get; }
	}
}