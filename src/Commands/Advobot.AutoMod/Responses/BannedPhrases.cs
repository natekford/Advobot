using System.Collections.Generic;

using Advobot.AutoMod.ReadOnlyModels;
using Advobot.Modules;
using Advobot.Punishments;
using Advobot.Utilities;

using AdvorangesUtils;

using static Advobot.Resources.Responses;
using static Advobot.Utilities.FormattingUtils;

namespace Advobot.AutoMod.Responses
{
	public sealed class BannedPhrases : AdvobotResult
	{
		private BannedPhrases() : base(null, "")
		{
		}

		public static AdvobotResult Added(string type, string phrase)
			=> Modified(type, true, phrase);

		public static AdvobotResult Display(IEnumerable<IReadOnlyBannedPhrase> phrases)
		{
			var joined = phrases.Join(x => x.Phrase);
			if (string.IsNullOrWhiteSpace(joined))
			{
				return Success(VariableNone);
			}
			return Success(joined.WithBigBlock().Value);
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
}