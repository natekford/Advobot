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

namespace Advobot
{
	public class Moderation_Commands : ModuleBase
	{
		[Command("fullmute")]
		[Alias("fm")]
		[Summary("Removes the user's ability to speak or type via the 'Muted' role.")]
		[RequireUserPermission(ChannelPermission.ManageMessages)]
		[MyPrecondition((1U<<(int)GuildPermission.ManageRoles) | (1U<<(int)GuildPermission.ManageMessages), 1U<<(int)GuildPermission.Administrator)]
		public async Task FullMute([Remainder] String input)
		{
			//Check if role already exists, if not, create it
			IRole muteRole = await Actions.createRoleIfNotFound(Context.Guild, "Muted");
			if (muteRole == null)
				return;

			//Determine if the bot can use the mute role
			int position = Actions.getPosition(Context.Guild, await Context.Guild.GetUserAsync(Context.Client.CurrentUser.Id));
			if (position < muteRole.Position)
			{
				//makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR("Mute role has a higher position than the bot can access."), WAIT_TIME);
				return;
			}

			//Test if valid user mention
			IGuildUser user = await Actions.getUser(Context.Guild, input);
			if (user == null)
			{
				//makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR(USER_ERROR), WAIT_TIME);
				return;
			}

			//Give the targetted user the role
			await Actions.giveRole(user, muteRole);
			//makeAndDeleteSecondaryMessage(e.Channel, e.Message, String.Format("Successfully muted {0}.", user.Mention), WAIT_TIME);
		}

		[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
		public class MyPreconditionAttribute : PreconditionAttribute
		{
			public MyPreconditionAttribute(uint needed, uint optional)
			{
				mNeeded = needed;
				mOptional = optional;
			}

			public override async Task<PreconditionResult> CheckPermissions(CommandContext context, CommandInfo command, IDependencyMap map)
			{
				IGuildUser user = await context.Guild.GetUserAsync(context.User.Id);
				GuildPermissions perms = user.GuildPermissions;
				PreconditionResult result;
				if ((perms.RawValue & mNeeded) == mNeeded)
					result = PreconditionResult.FromSuccess();
				else if ((perms.RawValue & mOptional) != 0)
					result = PreconditionResult.FromSuccess();
				else
					result = PreconditionResult.FromError("Fuck you!");
				return result;
			}

			private uint mNeeded;
			private uint mOptional;
		}
	}
}
