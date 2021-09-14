
using Discord.Commands;

namespace Advobot.Interactivity.Criterions
{
	/// <summary>
	/// Determines if a message is valid.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface ICriterion<in T>
	{
		/// <summary>
		/// Determines if the message matches this criterion.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="parameter"></param>
		/// <returns></returns>
		Task<bool> JudgeAsync(ICommandContext context, T parameter);
	}
}