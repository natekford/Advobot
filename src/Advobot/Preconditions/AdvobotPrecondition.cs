using Advobot.Services.Help;

using Discord.Commands;

namespace Advobot.Preconditions;

/// <summary>
/// Requires the context to meet a preconidition.
/// </summary>
public abstract class AdvobotPrecondition : PreconditionAttribute, IHelpPrecondition
{
	/// <inheritdoc />
	public string Name => Summary;
	/// <inheritdoc />
	public abstract string Summary { get; }
}