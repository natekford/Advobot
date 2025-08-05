using Discord.Commands;

namespace Advobot.Preconditions.Results;

/// <summary>
/// Result indicating an object of a specified type was not found.
/// </summary>
/// <remarks>
/// Creates an instance of <see cref="UnableToFind"/>.
/// </remarks>
/// <param name="type"></param>
public class UnableToFind(Type type) : PreconditionResult(CommandError.ObjectNotFound, $"Unable to find a matching `{type.Name}`.")
{
	/// <summary>
	/// The type of object not found.
	/// </summary>
	public Type Type { get; } = type;
}