using System;
using System.Threading.Tasks;

using Advobot.Services.HelpEntries;

using AdvorangesUtils;

using Discord.Commands;

namespace Advobot.Attributes.Preconditions
{
	/// <summary>
	/// Requires bot owner before this command will execute.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public sealed class RequireBotOwnerAttribute
		: PreconditionAttribute, IPrecondition
	{
		/// <inheritdoc />
		public string Summary
			=> "Invoker is the bot owner";

		/// <inheritdoc />
		public override async Task<PreconditionResult> CheckPermissionsAsync(
			ICommandContext context,
			CommandInfo command,
			IServiceProvider services)
		{
			var application = await context.Client.GetApplicationInfoAsync().CAF();
			if (application.Owner.Id == context.User.Id)
			{
				return PreconditionResult.FromSuccess();
			}
			return PreconditionResult.FromError("You are not the bot owner.");
		}
	}
}