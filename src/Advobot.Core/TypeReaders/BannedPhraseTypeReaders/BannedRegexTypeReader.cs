using System.Collections.Generic;
using Advobot.Services.GuildSettings;
using Advobot.Services.GuildSettings.Settings;

namespace Advobot.TypeReaders.BannedPhraseTypeReaders
{
	/// <summary>
	/// A type reader for banned regex.
	/// </summary>
	public sealed class BannedRegexTypeReader : BannedPhraseTypeReaderBase
	{
		/// <inheritdoc />
		protected override string BannedPhraseName => "regex";

		/// <inheritdoc />
		protected override IEnumerable<BannedPhrase> GetBannedPhrases(IGuildSettings settings)
			=> settings.BannedPhraseRegex;
	}
}
