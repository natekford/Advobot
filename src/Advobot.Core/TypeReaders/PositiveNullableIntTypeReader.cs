
using Advobot.Utilities;

using Discord.Commands;

namespace Advobot.TypeReaders
{
	/// <summary>
	/// Typereader for values which can be null or positive.
	/// </summary>
	public sealed class PositiveNullableIntTypeReader : TypeReader
	{
		/// <inheritdoc />
		public override Task<TypeReaderResult> ReadAsync(
			ICommandContext context,
			string input,
			IServiceProvider services)
		{
			if (input == null)
			{
				return TypeReaderResult.FromSuccess(null).AsTask();
			}
			else if (!int.TryParse(input, out var value))
			{
				return TypeReaderUtils.ParseFailedResult<int?>().AsTask();
			}
			else if (value < 1)
			{
				return TypeReaderResult.FromError(CommandError.UnmetPrecondition, "Value must be positive.").AsTask();
			}
			else
			{
				return TypeReaderResult.FromSuccess(value).AsTask();
			}
		}
	}
}