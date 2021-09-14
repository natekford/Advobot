
using Advobot.Services;
using Advobot.Services.HelpEntries;

using AdvorangesUtils;

using Discord.Commands;

using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Attributes.Preconditions
{
	/// <summary>
	/// Checks to make sure the bot is loaded, the guild is loaded, the channel isn't ignored from commands, and the command is enabled for the user.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public sealed class ExtendableCommandValidationAttribute
		: PreconditionAttribute, IPrecondition
	{
		/// <inheritdoc />
		public string Summary
			=> "Command is turned on";

		/// <inheritdoc />
		public override async Task<PreconditionResult> CheckPermissionsAsync(
			ICommandContext context,
			CommandInfo command,
			IServiceProvider services)
		{
			foreach (var checker in services.GetServices<ICommandValidator>())
			{
				var result = await checker.CanInvokeAsync(context, command).CAF();
				if (!result.IsSuccess)
				{
					return result;
				}
			}
			return PreconditionResult.FromSuccess();
		}
	}
}