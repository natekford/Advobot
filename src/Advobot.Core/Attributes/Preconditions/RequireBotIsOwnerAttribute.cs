using System;
using System.Threading.Tasks;
using Advobot.Modules;
using Discord.Commands;

namespace Advobot.Attributes.Preconditions
{
	/// <summary>
	/// Will return success if the bot is the owner of the guild in the context.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public class RequireBotIsOwnerAttribute : AdvobotPreconditionAttribute
	{
		/// <inheritdoc />
		public override bool Visible => true;

		/// <inheritdoc />
		public override Task<PreconditionResult> CheckPermissionsAsync(AdvobotCommandContext context, CommandInfo command, IServiceProvider services)
		{
			return context.Client.CurrentUser.Id == context.Guild.OwnerId
				? Task.FromResult(PreconditionResult.FromSuccess())
				: Task.FromResult(PreconditionResult.FromError("The bot is not the owner of the guild."));
		}
		/// <summary>
		/// Returns a string describing what this attribute requires.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
			=> "Bot is guild owner";
	}
}