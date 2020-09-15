using System.Threading.Tasks;

using AdvorangesUtils;

using Discord;
using Discord.WebSocket;

namespace Advobot.Levels.Utilities
{
	public static class LevelUtils
	{
		public static async Task<IUser?> GetUserAsync(this BaseSocketClient client, ulong id)
			=> client.GetUser(id) ?? (IUser)await client.Rest.GetUserAsync(id).CAF();
	}
}