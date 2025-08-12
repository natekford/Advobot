using Advobot.AutoMod.Database.Models;

using static Advobot.Resources.Responses;

namespace Advobot.AutoMod.ParameterPreconditions;

/// <summary>
/// Makes sure the passed in <see cref="string"/> is not already a banned regex.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
public sealed class NotAlreadyBannedRegex
	: NotAlreadyBannedPhraseParameterPrecondition
{
	/// <inheritdoc />
	protected override string BannedPhraseName => VariableRegex;

	/// <inheritdoc />
	protected override bool IsMatch(BannedPhrase phrase, string input)
		=> phrase.IsRegex && phrase.Phrase == input;
}