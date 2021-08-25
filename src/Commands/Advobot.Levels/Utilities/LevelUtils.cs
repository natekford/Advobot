
using Advobot.Levels.Models;

using AdvorangesUtils;

using Discord;
using Discord.WebSocket;

namespace Advobot.Levels.Utilities
{
	public static class LevelUtils
	{
		public static User AddXp(this User user, int xp)
		{
			return user with
			{
				Experience = user.Experience + xp,
				MessageCount = user.MessageCount + 1,
			};
		}

		public static async Task<IUser?> GetUserAsync(this BaseSocketClient client, ulong id)
			=> client.GetUser(id) ?? (IUser)await client.Rest.GetUserAsync(id).CAF();

		public static User RemoveXp(this User user, int xp)
		{
			return user with
			{
				Experience = Math.Max(0, user.Experience - xp),
				MessageCount = Math.Max(0, user.MessageCount - 1),
			};
		}
	}
}