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
		[Usage(BOT_PREFIX +"fullmute [@User]")]
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

		[Command("removemessages")]
		[Alias("rm")]
		[Usage(BOT_PREFIX + "removemessages <@User> <#Channel> [Number of Messages]")]
		[Summary("Removes the selected number of messages from either the user, the channel, both, or, if neither is input, the channel the command is said on." +
			"People without administrator can only delete up to 100 messages.")]
		[PermissionRequirements(1U<<(int)GuildPermission.ManageMessages, 1U << (int)GuildPermission.Administrator)]
		public async Task RemoveMessages([Remainder] String input)
		{
			String[] values = input.Split(' ');
			if ((values.Length < 1) || (values.Length > 3))
			{
				await makeAndDeleteSecondaryMessage(Context.Channel, Context.Message, ERROR(ARGUMENTS_ERROR), WAIT_TIME);
				return;
			}

			int argIndex = 0;
			int argCount = values.Length;

			//Testing if starts with user mention
			IGuildUser inputUser = null;
			if (argIndex < argCount && values[argIndex].StartsWith("<@"))
			{
				inputUser = await getUser(Context.Guild, values[argIndex]);
				if (null == inputUser)
				{
					await makeAndDeleteSecondaryMessage(Context.Channel, Context.Message, ERROR(USER_ERROR), WAIT_TIME);
					return;
				}
				++argIndex;
			}

			//Testing if starts with channel mention
			IMessageChannel inputChannel = Context.Channel;
			if (argIndex < argCount && values[argIndex].StartsWith("<#"))
			{
				inputChannel = await getChannelID(Context.Guild, values[argIndex]);
				if (null == inputChannel)
				{
					await makeAndDeleteSecondaryMessage(Context.Channel, Context.Message, ERROR(CHANNEL_ERROR), WAIT_TIME);
					return;
				}
				++argIndex;
			}

			//Checking for valid request count
			int requestCount = (argIndex == argCount - 1) ? getInteger(values[argIndex]) : -1;
			if (requestCount < 1)
			{
				await makeAndDeleteSecondaryMessage(Context.Channel, Context.Message, ERROR("Incorrect input for number of messages to be removed."), WAIT_TIME);
				return;
			}

			//See if the user is trying to delete more than 100 messages at a time
			if (requestCount > 100 && !(await Context.Guild.GetUserAsync(Context.User.Id)).GuildPermissions.Administrator)
			{
				await makeAndDeleteSecondaryMessage(Context.Channel, Context.Message, ERROR("You need administrator to remove more than 100 messages at a time."), WAIT_TIME);
				return;
			}

			//Removing the command message itself
			if (Context.Channel != inputChannel)
			{
				await removeMessages(Context.Channel, 0);
			}
			else if ((Context.User == inputUser) && (Context.Channel == inputChannel))
			{
				++requestCount;
			}

			await removeMessages(inputChannel, requestCount, inputUser);
		}
	}
}
