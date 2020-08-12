using Advobot.SQLite.Relationships;

namespace Advobot.Levels.ReadOnlyModels
{
	public interface IReadOnlyUser : IChannelChild, IUserChild
	{
		int Experience { get; }
		int MessageCount { get; }

		IReadOnlyUser AddXp(int xp);

		IReadOnlyUser RemoveXp(int xp);
	}
}