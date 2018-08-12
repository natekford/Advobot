using System;
using System.Threading.Tasks;
using Advobot.Interfaces;
using Discord.Commands;

namespace Advobot.Classes.Attributes
{
	/// <summary>
	/// Checks to make sure the bot has admin, the bot is loaded, the guild is loaded, the channel isn't ignored from commands, and the command is enabled for the user.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	public sealed class CommandRequirementAttribute : PreconditionAttribute
	{
		//TODO: rework this?
		/// <summary>
		/// Makes sure all the required checks are passed. Otherwise returns an error string.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="command"></param>
		/// <param name="services"></param>
		/// <returns></returns>
		public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
		{
			if (!(context is AdvobotCommandContext aContext))
			{
				throw new ArgumentException("Invalid context provided.");
			}
			if (!(aContext.GuildSettings is IGuildSettings s))
			{
				return Task.FromResult(PreconditionResult.FromError("Unable to get the guild settings."));
			}
			if (!s.Loaded)
			{
				return Task.FromResult(PreconditionResult.FromError("Wait until the guild is loaded."));
			}
			if (s.IgnoredCommandChannels.Contains(context.Channel.Id) || !s.CommandSettings.IsCommandEnabled(context, command))
			{
				return Task.FromResult(PreconditionResult.FromError(default(string)));
			}
			return Task.FromResult(PreconditionResult.FromSuccess());
		}
	}
}
