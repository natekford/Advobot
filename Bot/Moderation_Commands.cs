using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using Discord;
using Discord.Commands;
using Discord.Modules;
using Discord.WebSocket;
using System.Net;
using System.Runtime.InteropServices;
using System.Diagnostics;
using static Advobot.Constants;
using static Advobot.Actions;

namespace Advobot
{
	public class Moderation_Commands : ModuleBase
	{
		[Command("fullmute")]
		[Alias("fm")]
		[Summary("Removes the user's ability to speak or type via the 'Muted' role.")]
		[PermissionRequirements((1U<<(int)GuildPermission.ManageRoles) | (1U<<(int)GuildPermission.ManageMessages), 1U<<(int)GuildPermission.Administrator)]
		public async Task FullMute([Remainder] String input)
		{
			//Check if role already exists, if not, create it
			IRole muteRole = await createRoleIfNotFound(Context.Guild, MUTE_ROLE_NAME);
			if (muteRole == null)
				return;

			//See if both the bot and the user can edit/use this role
			if (await getRoleEditAbility(Context.Guild, Context.Channel, Context.Message,
				await Context.Guild.GetUserAsync(Context.User.Id), await Context.Guild.GetUserAsync(Context.Client.CurrentUser.Id), MUTE_ROLE_NAME) == null)
			{
				return;
			}

			//Test if valid user mention
			IGuildUser user = await getUser(Context.Guild, input);
			if (user == null)
			{
				await makeAndDeleteSecondaryMessage(Context.Channel, Context.Message, ERROR(USER_ERROR), WAIT_TIME);
				return;
			}

			//Give the targetted user the role
			await giveRole(user, muteRole);
			await makeAndDeleteSecondaryMessage(Context.Channel, Context.Message, String.Format("Successfully muted {0}.", user.Mention), WAIT_TIME);
		}
	}
}
