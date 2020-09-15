using System.Threading.Tasks;

using Discord.Commands;

namespace Advobot.Services
{
	/// <summary>
	/// Checks whether a command can be invoked.
	/// </summary>
	public interface ICommandValidator
	{
		/// <summary>
		/// Checks whether <paramref name="command"/> can be invoked in <paramref name="context"/>.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="command"></param>
		/// <returns></returns>
		Task<PreconditionResult> CanInvokeAsync(
			ICommandContext context,
			CommandInfo command);
	}
}