using Advobot.Modules;

using YACCS.Preconditions;
using YACCS.Results;

namespace Advobot.ParameterPreconditions;

/// <summary>
/// Requires the parameter to meet a precondition unless it's optional.
/// </summary>
public abstract class AdvobotParameterPrecondition<T>
	: ParameterPrecondition<IGuildContext, T>
{
	/// <summary>
	/// Whether or not a null value is valid.
	/// </summary>
	public bool AllowNull { get; set; }
	/// <inheritdoc />
	public virtual string Name => Summary;
	/// <inheritdoc />
	public abstract string Summary { get; }

	/// <inheritdoc />
	public override string ToString()
		=> Summary;

	/// <inheritdoc />
	protected override ValueTask<IResult> CheckNullAsync(CommandMeta meta, IGuildContext context)
	{
		if (AllowNull)
		{
			return new(Result.EmptySuccess);
		}
		return base.CheckNullAsync(meta, context);
	}
}