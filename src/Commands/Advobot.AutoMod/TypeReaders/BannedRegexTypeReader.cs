using Advobot.AutoMod.Models;

using static Advobot.Resources.Responses;

namespace Advobot.AutoMod.TypeReaders;

/// <summary>
/// A type reader for banned regex.
/// </summary>
public sealed class BannedRegexTypeReader : BannedPhraseTypeReaderBase
{
	/// <inheritdoc />
	protected override string BannedPhraseName => VariableRegex;

	/// <inheritdoc />
	protected override bool IsValid(BannedPhrase phrase, string input)
		=> !phrase.IsName && phrase.IsRegex && phrase.Phrase == input;
}