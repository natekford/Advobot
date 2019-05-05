using Advobot.Classes.Modules;
using Advobot.Interfaces;

using Discord.Commands;

using Microsoft.Extensions.DependencyInjection;

using System;
using System.Threading.Tasks;

namespace Advobot.Classes.Attributes.Preconditions
{
	/// <summary>
	/// Checks to make sure the user is allowed to dm the bot owner.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public sealed class RequireAllowedToDmBotOwnerAttribute : AdvobotPreconditionAttribute
	{
		/// <inheritdoc />
		public override bool Visible => true;

		/// <inheritdoc />
		public override Task<PreconditionResult> CheckPermissionsAsync(AdvobotCommandContext context, CommandInfo command, IServiceProvider services)
		{
			return services.GetRequiredService<IBotSettings>().UsersUnableToDmOwner.Contains(context.User.Id)
				? Task.FromResult(PreconditionResult.FromError("You are unable to dm the bot owner."))
				: Task.FromResult(PreconditionResult.FromSuccess());
		}
		/// <summary>
		/// Returns a string describing what this attribute requires.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
			=> "Allowed to dm the bot owner";
	}
}
