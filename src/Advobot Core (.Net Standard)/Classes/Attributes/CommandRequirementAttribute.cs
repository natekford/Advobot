using Advobot.Actions;
using Advobot.Interfaces;
using Discord;
using Discord.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Advobot.Classes.Attributes
{
	/// <summary>
	/// Checks to make sure the bot has admin, the bot is loaded, the guild is loaded, the channel isn't ignored from commands, and the command is enabled for the user.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public class CommandRequirementAttribute : PreconditionAttribute
	{
		public override Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IServiceProvider services)
		{
			if (context is AdvobotCommandContext myContext)
			{
				var user = context.User as IGuildUser;

				if (!UserActions.GetBot(myContext.Guild).GuildPermissions.Administrator)
				{
					return Task.FromResult(PreconditionResult.FromError($"This bot will not function without the `{nameof(GuildPermission.Administrator)}` permission."));
				}
				else if (!myContext.GuildSettings.Loaded)
				{
					return Task.FromResult(PreconditionResult.FromError("Wait until the guild is loaded."));
				}
				else if (myContext.GuildSettings.IgnoredCommandChannels.Contains(context.Channel.Id) || !CheckIfCommandIsEnabled(myContext, command, user))
				{
					return Task.FromResult(PreconditionResult.FromError(Constants.IGNORE_ERROR));
				}
				else
				{
					return Task.FromResult(PreconditionResult.FromSuccess());
				}
			}
			return Task.FromResult(PreconditionResult.FromError(Constants.IGNORE_ERROR));
		}

		private bool CheckIfCommandIsEnabled(IAdvobotCommandContext context, CommandInfo command, IGuildUser user)
		{
			//Use the first alias since that's what group gets set as (could use any alias since GetCommand works for aliases too)
			//Doing a split since subcommands (in this bot's case) are simply easy to use options on a single command
			var cmd = context.GuildSettings.GetCommand(command.Aliases[0].Split(' ')[0]);
			if (!cmd.Value)
			{
				return false;
			}

			//If any of user/role/channel are set that means they are ignored (unignored things will not be set)

			var userOverrides = context.GuildSettings.CommandsDisabledOnUser;
			var userOverride = userOverrides.FirstOrDefault(x => x.Id == context.User.Id && cmd.Name.CaseInsEquals(x.Name));
			if (userOverride != null)
			{
				return false;
			}

			var roleOverrides = context.GuildSettings.CommandsDisabledOnRole;
			var roleOverride = roleOverrides.Where(x => user.RoleIds.Contains(x.Id) && cmd.Name.CaseInsEquals(x.Name)).OrderBy(x => context.Guild.GetRole(x.Id).Position).LastOrDefault();
			if (roleOverride != null)
			{
				return false;
			}

			var channelOverrides = context.GuildSettings.CommandsDisabledOnChannel;
			var channelOverride = channelOverrides.FirstOrDefault(x => x.Id == context.Channel.Id && cmd.Name.CaseInsEquals(x.Name));
			if (channelOverride != null)
			{
				return false;
			}

			return true;
		}
	}
}
