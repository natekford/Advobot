using System;
using System.Threading.Tasks;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord.Commands;

namespace Advobot.Attributes.Preconditions
{
	/// <summary>
	/// Requires bot owner before this command will execute.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public sealed class RequireBotOwnerAttribute : PreconditionAttribute
	{
		/// <inheritdoc />
		public override async Task<PreconditionResult> CheckPermissionsAsync(
			ICommandContext context,
			CommandInfo command,
			IServiceProvider services)
		{
			if (await context.Client.GetOwnerIdAsync().CAF() == context.User.Id)
			{
				return PreconditionUtils.FromSuccess();
			}
			return PreconditionUtils.FromError("You are not the bot owner.");
		}
		/// <inheritdoc />
		public override string ToString()
			=> "Bot owner";
	}
}