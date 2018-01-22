using Advobot.Core.Utilities;
using Advobot.Core.Interfaces;
using Discord;
using Discord.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Advobot.Core.Classes.Attributes
{
	/// <summary>
	/// Checks to make sure the bot has admin, the bot is loaded, the guild is loaded, the channel isn't ignored from commands, and the command is enabled for the user.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public class CommandRequirementAttribute : PreconditionAttribute
	{
		public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
		{
			if (context is IAdvobotCommandContext advobotCommandContext)
			{
				var user = context.User as IGuildUser;

				if (!advobotCommandContext.Guild.GetBot().GuildPermissions.Administrator)
				{
					return Task.FromResult(PreconditionResult.FromError($"This bot will not function without the `{nameof(GuildPermission.Administrator)}` permission."));
				}
				else if (!advobotCommandContext.GuildSettings.Loaded)
				{
					return Task.FromResult(PreconditionResult.FromError("Wait until the guild is loaded."));
				}
				else if (advobotCommandContext.GuildSettings.IgnoredCommandChannels.Contains(context.Channel.Id)
					|| !advobotCommandContext.GuildSettings.CommandSettings.IsCommandEnabled(context, command))
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
	}
}
