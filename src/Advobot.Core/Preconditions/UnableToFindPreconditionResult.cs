
using Discord.Commands;

namespace Advobot.Preconditions
{
	/// <summary>
	/// Result indicating an object of a specified type was not found.
	/// </summary>
	public class UnableToFindPreconditionResult : PreconditionResult
	{
		/// <summary>
		/// The type of object not found.
		/// </summary>
		public Type Type { get; }

		/// <summary>
		/// Creates an instance of <see cref="UnableToFindPreconditionResult"/>.
		/// </summary>
		/// <param name="type"></param>
		public UnableToFindPreconditionResult(Type type)
			: base(CommandError.ObjectNotFound, $"Unable to find a matching `{type.Name}`.")
		{
			Type = type;
		}
	}
}