using Advobot.Modules;

using YACCS.Commands;
using YACCS.Preconditions;
using YACCS.Results;

namespace Advobot.ParameterPreconditions;

/// <summary>
/// Requires the parameter to meet a precondition unless it's optional.
/// </summary>
public abstract class AdvobotParameterPrecondition<T>
	: ParameterPrecondition<IGuildContext, T>
{
	/// <inheritdoc />
	public virtual string Name => Summary;
	/// <inheritdoc />
	public abstract string Summary { get; }
}