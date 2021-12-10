using Advobot.Attributes;
using Advobot.Utilities;

using Discord.Commands;

using ImageMagick;

namespace Advobot.TypeReaders;

/// <summary>
/// Attempts to parse a percentage from a string.
/// </summary>
[TypeReaderTargetType(typeof(Percentage))]
public sealed class PercentageTypeReader : TypeReader
{
	/// <summary>
	/// Creates a percentage from a string.
	/// </summary>
	/// <param name="context"></param>
	/// <param name="input"></param>
	/// <param name="services"></param>
	/// <returns></returns>
	public override Task<TypeReaderResult> ReadAsync(
		ICommandContext context,
		string input,
		IServiceProvider services)
	{
		if (double.TryParse(input, out var value))
		{
			return TypeReaderResult.FromSuccess(new Percentage(value)).AsTask();
		}
		return TypeReaderUtils.ParseFailedResult<Percentage>().AsTask();
	}
}