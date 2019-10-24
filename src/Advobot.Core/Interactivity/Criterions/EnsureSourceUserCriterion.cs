using System.Threading.Tasks;

using Discord;
using Discord.Commands;

namespace Advobot.Interactivity.Criterions
{
	/// <summary>
	/// Determines if a message is from the source user.
	/// </summary>
	public sealed class EnsureSourceUserCriterion : ICriterion<IMessage>
	{
		/// <inheritdoc />
		public Task<bool> JudgeAsync(ICommandContext context, IMessage parameter)
			=> Task.FromResult(context.User.Id == parameter.Author.Id);
	}
}