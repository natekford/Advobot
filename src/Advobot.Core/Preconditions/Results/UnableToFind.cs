using Discord.Commands;

namespace Advobot.Preconditions.Results;

/// <summary>
/// Result indicating an object of a specified type was not found.
/// </summary>
public class UnableToFind : PreconditionResult
{
	/// <summary>
	/// The type of object not found.
	/// </summary>
	public Type Type { get; }

	/// <summary>
	/// Creates an instance of <see cref="UnableToFind"/>.
	/// </summary>
	/// <param name="type"></param>
	public UnableToFind(Type type)
		: base(CommandError.ObjectNotFound, $"Unable to find a matching `{type.Name}`.")
	{
		Type = type;
	}
}