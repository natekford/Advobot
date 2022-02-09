using Advobot.AutoMod.Models;

using static Advobot.Resources.Responses;

namespace Advobot.AutoMod.TypeReaders;

/// <summary>
/// A type reader for banned strings.
/// </summary>
public sealed class BannedStringTypeReader : BannedPhraseTypeReaderBase
{
	/// <inheritdoc />
	protected override string BannedPhraseName => VariableString;

	/// <inheritdoc />
	protected override bool IsValid(BannedPhrase phrase, string input)
		=> !phrase.IsName && !phrase.IsRegex && phrase.Phrase == input;
}