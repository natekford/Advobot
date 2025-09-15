using Advobot.Modules;

using YACCS.Commands.Models;
using YACCS.Results;

namespace Advobot.Services;

/// <summary>
/// Checks whether a command can be invoked.
/// </summary>
public interface ICommandValidator
{
	/// <summary>
	/// Checks whether <paramref name="command"/> can be invoked in <paramref name="context"/>.
	/// </summary>
	/// <param name="command"></param>
	/// <param name="context"></param>
	/// <returns></returns>
	Task<IResult> CanInvokeAsync(
		IImmutableCommand command,
		IGuildContext context);
}