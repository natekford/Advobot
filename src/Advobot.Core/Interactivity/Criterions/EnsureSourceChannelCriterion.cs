
using Discord;
using Discord.Commands;

namespace Advobot.Interactivity.Criterions
{
	/// <summary>
	/// Determines if a message is from the source channel.
	/// </summary>
	public sealed class EnsureSourceChannelCriterion : ICriterion<IMessage>
	{
		/// <inheritdoc />
		public Task<bool> JudgeAsync(ICommandContext context, IMessage parameter)
			=> Task.FromResult(context.Channel.Id == parameter.Channel.Id);
	}
}