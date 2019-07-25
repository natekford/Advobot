using Advobot.Gacha.ReadOnlyModels;

namespace Advobot.Gacha.Models
{
	public class User : IReadOnlyUser
	{
		public string GuildId { get; set; }
		public string UserId { get; set; }
	}
}
