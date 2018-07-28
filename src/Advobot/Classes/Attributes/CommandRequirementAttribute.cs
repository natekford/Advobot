using System;
using System.Threading.Tasks;
using Advobot.Interfaces;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Classes.Attributes
{
	/// <summary>
	/// Checks to make sure the bot has admin, the bot is loaded, the guild is loaded, the channel isn't ignored from commands, and the command is enabled for the user.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class CommandRequirementAttribute : PreconditionAttribute
	{
		/// <summary>
		/// Makes sure all the required checks are passed. Otherwise returns an error string.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="command"></param>
		/// <param name="services"></param>
		/// <returns></returns>
		public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
		{
			if (!(context is AdvobotSocketCommandContext aContext))
			{
				throw new ArgumentException("Invalid context provided.");
			}
			if (!(aContext.Guild.CurrentUser is SocketGuildUser bot))
			{
				return Task.FromResult(PreconditionResult.FromError("Unable to get the bot."));
			}
			if (!(aContext.GuildSettings is IGuildSettings settings))
			{
				return Task.FromResult(PreconditionResult.FromError("Unable to get the guild settings."));
			}
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
