using Advobot.Databases.Relationships;
using Advobot.Gacha.ReadOnlyModels;
using Advobot.Utilities;

using Discord;

namespace Advobot.Gacha.Models
{
	public class User : IReadOnlyUser
	{
		public string GuildId { get; set; }
		public string UserId { get; set; }

		ulong IGuildChild.GuildId => GuildId.ToId();
		ulong IUserChild.UserId => UserId.ToId();

		public User()
		{
			GuildId = "";
			UserId = "";
		}

		public User(IGuildUser user)
		{
			GuildId = user.GuildId.ToString();
			UserId = user.Id.ToString();
		}
	}
}