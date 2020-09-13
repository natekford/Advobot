using System;
using System.Threading.Tasks;

using Advobot.Services.HelpEntries;
using Advobot.Utilities;

using Discord.Commands;

namespace Advobot.Attributes.Preconditions
{
	/// <summary>
	/// Requires guild owner before this command will execute.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public sealed class RequireGuildOwnerAttribute
		: PreconditionAttribute, IPrecondition
	{
		/// <inheritdoc />
		public string Summary
			=> "Invoker is the guild owner";

		/// <inheritdoc />
		public override Task<PreconditionResult> CheckPermissionsAsync(
			ICommandContext context,
			CommandInfo command,
			IServiceProvider services)
		{
			if (context.Guild.OwnerId == context.User.Id)
			{
				return this.FromSuccess().AsTask();
			}
			return PreconditionResult.FromError("You are not the guild owner.").AsTask();
		}
	}
}