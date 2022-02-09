using Advobot.AutoMod.Models;

using static Advobot.Resources.Responses;

namespace Advobot.AutoMod.TypeReaders;

/// <summary>
/// A type reader for banned names.
/// </summary>
public sealed class BannedNameTypeReader : BannedPhraseTypeReaderBase
{
	/// <inheritdoc />
	protected override string BannedPhraseName => VariableName;

	/// <inheritdoc />
	protected override bool IsValid(BannedPhrase phrase, string input)
		=> phrase.IsName && phrase.Phrase == input;
}