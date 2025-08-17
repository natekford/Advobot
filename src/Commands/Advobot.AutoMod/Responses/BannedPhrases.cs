using Advobot.AutoMod.Database.Models;
using Advobot.Modules;
using Advobot.Punishments;
using Advobot.Utilities;

using static Advobot.Resources.Responses;
using static Advobot.Utilities.FormattingUtils;

namespace Advobot.AutoMod.Responses;

public sealed class BannedPhrases : AdvobotResult
{
	public static AdvobotResult Added(string type, string phrase)
		=> Modified(type, true, phrase);

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
		string type,
		string phrase,
		PunishmentType punishment)
	{
		return Success(BannedPhraseChangedPunishment.Format(
			type.WithNoMarkdown(),
			phrase.WithBlock(),
			punishment.ToString().WithBlock()
		));
	}

	public static AdvobotResult Removed(string type, string phrase)
		=> Modified(type, false, phrase);

	private static AdvobotResult Modified(string type, bool added, string phrase)
	{
		var format = added ? BannedPhraseAdded : BannedPhraseRemoved;
		return Success(format.Format(
			type.WithNoMarkdown(),
			phrase.WithBlock()
		));
	}
}