using Advobot.Gacha.ReadOnlyModels;

namespace Advobot.Gacha.Relationships
{
	public interface IUserChild
	{
		string GuildId { get; }
		string UserId { get; }

		IReadOnlyUser User { get; }
	}
}
