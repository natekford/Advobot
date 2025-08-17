using Discord;
using Discord.Commands;

namespace Advobot.Interactivity.Criterions;

/// <summary>
/// Determines if a message is from the specified user.
/// </summary>
/// <param name="userId"></param>
public sealed class EnsureFromUserCriterion(ulong userId) : ICriterion<IMessage>
{
	private readonly ulong _UserId = userId;

	/// <inheritdoc />
	public Task<bool> JudgeAsync(ICommandContext context, IMessage parameter)
		=> Task.FromResult(_UserId == parameter.Author.Id);
}