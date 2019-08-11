using System;
using System.Collections.Generic;
using Advobot.Services.GuildSettings;
using Advobot.Services.GuildSettings.Settings;

namespace Advobot.Attributes.ParameterPreconditions.BannedPhrases
{
	/// <summary>
	/// Makes sure the passed in <see cref="string"/> is not already a banned regex.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public sealed class NotAlreadyBannedRegexAttribute
		: BannedPhraseParameterPreconditionAttribute
	{
		/// <inheritdoc />
		protected override string BannedPhraseName => "regex";

		/// <inheritdoc />
		protected override IEnumerable<BannedPhrase> GetPhrases(IGuildSettings settings)
			=> settings.BannedPhraseRegex;
	}
}
