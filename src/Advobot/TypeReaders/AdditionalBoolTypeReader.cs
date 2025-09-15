using System.Collections.Immutable;

using YACCS.TypeReaders;

namespace Advobot.TypeReaders;

/// <summary>
/// Attempts to parse bools and also other positive/negative words.
/// </summary>
[TypeReaderTargetTypes(typeof(bool), OverrideExistingTypeReaders = true)]
public sealed class AdditionalBoolTypeReader() : TryParseTypeReader<bool>(TryParse)
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

	private static bool TryParse(string s, out bool result)
	{
		if (TrueVals.Contains(s))
		{
			result = true;
			return true;
		}
		else if (FalseVals.Contains(s))
		{
			result = false;
			return true;
		}
		else
		{
			result = false;
			return false;
		}
	}
}