using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

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
			if (!(context is AdvobotSocketCommandContext advobotCommandContext))
			{
				return Task.FromResult(PreconditionResult.FromError((string)null));
			}
			if (!(advobotCommandContext.Guild.CurrentUser is SocketGuildUser bot))
			{
				return Task.FromResult(PreconditionResult.FromError("Unable to get the bot."));
			}
			if (!bot.GuildPermissions.Administrator)
			{
				return Task.FromResult(PreconditionResult.FromError($"This bot will not function without the `{nameof(GuildPermission.Administrator)}` permission."));
			}

			var settings = advobotCommandContext.GuildSettings;
			if (!settings.Loaded)
			{
				return Task.FromResult(PreconditionResult.FromError("Wait until the guild is loaded."));
			}
			if (settings.IgnoredCommandChannels.Contains(context.Channel.Id)
				|| !settings.CommandSettings.IsCommandEnabled(services.GetRequiredService<HelpEntryHolder>(), context, command))
			{
				return Task.FromResult(PreconditionResult.FromError((string)null));
			}

			return Task.FromResult(PreconditionResult.FromSuccess());
		}
	}
}
