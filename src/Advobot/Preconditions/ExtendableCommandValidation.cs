using Advobot.Modules;
using Advobot.Services;

using Microsoft.Extensions.DependencyInjection;

using YACCS.Commands.Models;
using YACCS.Results;

namespace Advobot.Preconditions;

/// <summary>
/// Checks to make sure the bot is loaded, the guild is loaded, the channel isn't ignored from commands, and the command is enabled for the user.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class ExtendableCommandValidation : AdvobotPrecondition
{
	/// <inheritdoc />
	public override string Summary => "Command is turned on";

	/// <inheritdoc />
	public override async ValueTask<IResult> CheckAsync(
		IImmutableCommand command,
		IGuildContext context)
	{
		foreach (var checker in context.Services.GetServices<ICommandValidator>())
		{
			var result = await checker.CanInvokeAsync(command, context).ConfigureAwait(false);
			if (!result.IsSuccess)
			{
				return result;
			}
		}
		return CachedResults.Success;
	}
}