using System;
using System.Threading.Tasks;
using Advobot.Utilities;
using Discord.Commands;

namespace Advobot.Attributes.Preconditions
{
	/// <summary>
	/// Will return success if the bot is the owner of the guild in the context.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public class RequireBotIsOwnerAttribute : PreconditionAttribute
	{
		/// <inheritdoc />
		public override Task<PreconditionResult> CheckPermissionsAsync(
			ICommandContext context,
			CommandInfo command,
			IServiceProvider services)
		{
			if (context.Client.CurrentUser.Id == context.Guild.OwnerId)
			{
				return PreconditionUtils.FromSuccessAsync();
			}
			return PreconditionUtils.FromErrorAsync("The bot is not the owner of the guild.");
		}
		/// <inheritdoc />
		public override string ToString()
			=> "Bot is guild owner";
	}
}