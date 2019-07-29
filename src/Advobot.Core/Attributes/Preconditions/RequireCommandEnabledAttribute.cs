using Advobot.Modules;

using Discord.Commands;

using System;
using System.Threading.Tasks;

namespace Advobot.Attributes.Preconditions
{
	/// <summary>
	/// Checks to make sure the bot is loaded, the guild is loaded, the channel isn't ignored from commands, and the command is enabled for the user.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public sealed class RequireCommandEnabledAttribute : AdvobotPreconditionAttribute
	{
		/// <inheritdoc />
		public override bool Visible => false;

		/// <inheritdoc />
		public override Task<PreconditionResult> CheckPermissionsAsync(AdvobotCommandContext context, CommandInfo command, IServiceProvider services)
		{
			return context.GuildSettings.CommandSettings.IsCommandEnabled(context.User, context.Channel, command)
				? Task.FromResult(PreconditionResult.FromSuccess())
				: Task.FromResult(PreconditionResult.FromError("This command is disabled on the guild."));
		}
		/// <summary>
		/// Returns a string describing what this attribute requires.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
			=> "Default command requirements";
	}
}
