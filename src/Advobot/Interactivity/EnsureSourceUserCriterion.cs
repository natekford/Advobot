using Advobot.Modules;

using Discord;

using YACCS.Interactivity;
using YACCS.Results;

namespace Advobot.Interactivity;

/// <summary>
/// Determines if a message is from the source user.
/// </summary>
public sealed class EnsureSourceUserCriterion : ICriterion<IGuildContext, IMessage>
{
	/// <inheritdoc />
	public ValueTask<IResult> JudgeAsync(IGuildContext context, IMessage input)
		=> new(context.User.Id == input.Author.Id ? Result.EmptySuccess : Result.EmptyFailure);
}