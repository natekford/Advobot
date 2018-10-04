using System;
using System.Threading.Tasks;
using Advobot.Interfaces;
using Advobot.Utilities;
using Discord.Commands;

namespace Advobot.Classes.Attributes.Preconditions
{
	/// <summary>
	/// Checks to make sure the bot is loaded, the guild is loaded, the channel isn't ignored from commands, and the command is enabled for the user.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public sealed class CommandRequirementAttribute : SelfGroupPreconditionAttribute
	{
		/// <inheritdoc />
		public override bool Visible => false;

		/// <inheritdoc />
		public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
		{
			var (Context, Invoker) = context.InternalCastContext();

			if (!(Context.GuildSettings is IGuildSettings settings))
			{
				return Task.FromResult(PreconditionResult.FromError("Unable to get the guild settings."));
			}
			if (!settings.Loaded)
			{
				return Task.FromResult(PreconditionResult.FromError("Wait until the guild is loaded."));
			}
			if (settings.IgnoredCommandChannels.Contains(context.Channel.Id)
				|| !settings.CommandSettings.IsCommandEnabled(context, command))
			{
				return Task.FromResult(PreconditionResult.FromError(default(string)));
			}
			return Task.FromResult(PreconditionResult.FromSuccess());
		}
		/// <summary>
		/// Returns a string describing what this attribute requires.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
			=> "Default command requirements";
	}
}
