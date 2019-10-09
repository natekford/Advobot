using System;
using System.Threading.Tasks;

using Advobot.Services.BotSettings;
using Advobot.Services.HelpEntries;
using Advobot.Utilities;

using Discord.Commands;

using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Attributes.Preconditions
{
	/// <summary>
	/// Checks to make sure the user is allowed to dm the bot owner.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public sealed class RequireAllowedToDmBotOwnerAttribute
		: PreconditionAttribute, IPrecondition
	{
		/// <inheritdoc />
		public string Summary
			=> "Not blocked by the bot owner";

		/// <inheritdoc />
		public override Task<PreconditionResult> CheckPermissionsAsync(
			ICommandContext context,
			CommandInfo command,
			IServiceProvider services)
		{
			var botSettings = services.GetRequiredService<IBotSettings>();
			if (!botSettings.UsersUnableToDmOwner.Contains(context.User.Id))
			{
				return PreconditionResult.FromSuccess().AsTask();
			}
			return PreconditionResult.FromError("You are unable to dm the bot owner.").AsTask();
		}
	}
}