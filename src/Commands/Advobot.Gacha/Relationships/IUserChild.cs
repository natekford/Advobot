using Advobot.Gacha.ReadOnlyModels;

namespace Advobot.Gacha.Relationships
{
	public interface IUserChild
	{
		ulong GuildId { get; }
		ulong UserId { get; }

		IReadOnlyUser User { get; }
	}
}
