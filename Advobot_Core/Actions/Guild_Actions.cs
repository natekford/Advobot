using Advobot.Structs;
using Discord;
using System.Linq;
using System.Threading.Tasks;

namespace Advobot
{
	namespace Actions
	{
		public static class GuildActions
		{
			public static async Task<IGuild> GetGuild(IDiscordClient client, ulong id)
			{
				return await client.GetGuildAsync(id);
			}

			public static ulong AddGuildPermissionBit(string permissionName, ulong inputValue)
			{
				var permission = Constants.GUILD_PERMISSIONS.FirstOrDefault(x => x.Name.CaseInsEquals(permissionName));
				if (!permission.Equals(default(BotGuildPermission)))
				{
					inputValue |= permission.Bit;
				}
				return inputValue;
			}
			public static ulong RemoveGuildPermissionBit(string permissionName, ulong inputValue)
			{
				var permission = Constants.GUILD_PERMISSIONS.FirstOrDefault(x => x.Name.CaseInsEquals(permissionName));
				if (!permission.Equals(default(BotGuildPermission)))
				{
					inputValue &= ~permission.Bit;
				}
				return inputValue;
			}
		}
	}
}