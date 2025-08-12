using Advobot.AutoMod.Database.Models;

using static Advobot.Resources.Responses;

namespace Advobot.AutoMod.ParameterPreconditions;

/// <summary>
/// Makes sure the passed in <see cref="string"/> is not already a banned string.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
public sealed class NotAlreadyBannedString
	: NotAlreadyBannedPhraseParameterPrecondition
{
	/// <inheritdoc />
	protected override string BannedPhraseName => VariableString;

	/// <inheritdoc />
	protected override bool IsMatch(BannedPhrase phrase, string input)
		=> !phrase.IsRegex && phrase.Phrase == input;
}