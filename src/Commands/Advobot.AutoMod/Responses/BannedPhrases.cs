using Advobot.AutoMod.Database.Models;
using Advobot.Modules;
using Advobot.Punishments;
using Advobot.Utilities;

using static Advobot.Resources.Responses;
using static Advobot.Utilities.FormattingUtils;

namespace Advobot.AutoMod.Responses;

public sealed class BannedPhrases : AdvobotResult
{
	public static AdvobotResult Added(Phrase phraseType, string phrase)
		=> Modified(phraseType, true, phrase);

	public static AdvobotResult Display(IEnumerable<BannedPhrase> phrases)
	{
		var joined = phrases.Select(x => x.Phrase).Join();
		if (string.IsNullOrWhiteSpace(joined))
		{
			return Success(VariableNone);
		}
		return Success(joined.WithBigBlock().Current);
	}

	public static AdvobotResult PunishmentChanged(
		Phrase phraseType,
		string phrase,
		PunishmentType punishment)
	{
		return Success(BannedPhraseChangedPunishment.Format(
			GetPhraseType(phraseType).WithNoMarkdown(),
			phrase.WithBlock(),
			punishment.ToString().WithBlock()
		));
	}

	public static AdvobotResult Removed(Phrase phraseType, string phrase)
		=> Modified(phraseType, false, phrase);

	private static string GetPhraseType(Phrase phrase)
	{
		return phrase switch
		{
			Phrase.Name => VariableName,
			Phrase.Regex => VariableRegex,
			Phrase.String => VariableString,
			_ => throw new ArgumentOutOfRangeException(nameof(phrase)),
		};
	}

	private static AdvobotResult Modified(Phrase phraseType, bool added, string phrase)
	{
		var format = added ? BannedPhraseAdded : BannedPhraseRemoved;
		return Success(format.Format(
			GetPhraseType(phraseType).WithNoMarkdown(),
			phrase.WithBlock()
		));
	}
}

public enum Phrase
{
	Name,
	Regex,
	String,
}