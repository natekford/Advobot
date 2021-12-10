using Advobot.Attributes;
using Advobot.Utilities;

using Discord.Commands;

using System.Collections.Immutable;

namespace Advobot.TypeReaders;

/// <summary>
/// Attempts to parse bools and also other positive/negative words.
/// </summary>
[TypeReaderTargetType(typeof(bool))]
public sealed class AdditionalBoolTypeReader : TypeReader
{
	/// <summary>
	/// Values that will set the stored bool to false.
	/// </summary>
	public static readonly ImmutableHashSet<string> FalseVals = new[]
	{
			"false",
			"no",
			"remove",
			"disable",
			"unset",
			"negative"
		}.ToImmutableHashSet(StringComparer.OrdinalIgnoreCase);

	/// <summary>
	/// Values that will set the stored bool to true.
	/// </summary>
	public static readonly ImmutableHashSet<string> TrueVals = new[]
	{
			"true",
			"yes",
			"add",
			"enable",
			"set",
			"positive"
		}.ToImmutableHashSet(StringComparer.OrdinalIgnoreCase);

	/// <summary>
	/// Converts a string into a true bool if it has a match in <see cref="TrueVals"/>,
	/// false bool if it has a match in <see cref="FalseVals"/>,
	/// or returns an error.
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
		if (TrueVals.Contains(input))
		{
			return TypeReaderResult.FromSuccess(true).AsTask();
		}
		else if (FalseVals.Contains(input))
		{
			return TypeReaderResult.FromSuccess(false).AsTask();
		}
		return TypeReaderUtils.ParseFailedResult<bool>().AsTask();
	}
}