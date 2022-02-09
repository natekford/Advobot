using Advobot.AutoMod.Models;

using static Advobot.Resources.Responses;

namespace Advobot.AutoMod.ParameterPreconditions;

/// <summary>
/// Makes sure the passed in <see cref="string"/> is not already a banned name.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
public sealed class NotAlreadyBannedName
	: NotAlreadyBannedPhraseParameterPrecondition
{
	/// <inheritdoc />
	protected override string BannedPhraseName => VariableName;

	/// <inheritdoc />
	protected override bool IsMatch(BannedPhrase phrase, string input)
		=> phrase.IsName && phrase.Phrase == input;
}