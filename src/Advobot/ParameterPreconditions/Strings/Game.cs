namespace Advobot.ParameterPreconditions.Strings;

/// <summary>
/// Validates the game by making sure it is between 0 and 128 characters.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
public sealed class Game : StringLengthParameterPrecondition
{
	/// <inheritdoc />
	public override string StringType => "game";

	/// <summary>
	/// Creates an instance of <see cref="Game"/>.
	/// </summary>
	public Game() : base(0, 128) { }
}