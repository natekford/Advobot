using Advobot.Modules;

using YACCS.Preconditions;

namespace Advobot.Preconditions;

/// <summary>
/// Requires the context to meet a preconidition.
/// </summary>
public abstract class AdvobotPrecondition : Precondition<IGuildContext>
{
	/// <inheritdoc />
	public string Name => Summary;
	/// <inheritdoc />
	public abstract string Summary { get; }

	/// <inheritdoc />
	public override string ToString()
		=> Summary;
}