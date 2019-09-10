using Advobot.Formatting;
using Advobot.Modules;
using Advobot.Services.GuildSettings.Settings;
using Advobot.Utilities;

namespace Advobot.Settings.Responses
{
	public sealed class BannedPhrases : CommandResponses
	{
		private BannedPhrases()
		{
		}

		public static AdvobotResult ChangePunishment(
			string type,
			BannedPhrase phrase,
			Punishment punishment)
			=> Success(Default.FormatInterpolated($"Successfully changed the punishment of the banned {type.NoFormatting()} {phrase} to {punishment}."));

		public static AdvobotResult Modified(string type, bool added, BannedPhrase phrase)
			=> Success(Default.FormatInterpolated($"Successfully {GetAdded(added)} the banned {type.NoFormatting()} {phrase}."));
	}
}