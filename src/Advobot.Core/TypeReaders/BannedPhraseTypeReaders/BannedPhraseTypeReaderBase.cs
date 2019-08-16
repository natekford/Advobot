using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Advobot.Services.GuildSettings;
using Advobot.Services.GuildSettings.Settings;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace Advobot.TypeReaders.BannedPhraseTypeReaders
{
	/// <summary>
	/// A type reader for banned phrases.
	/// </summary>
	public abstract class BannedPhraseTypeReaderBase : TypeReader
	{
		/// <summary>
		/// Gets the name of the banned phrase type.
		/// </summary>
		protected abstract string BannedPhraseName { get; }

		/// <inheritdoc />
		public override async Task<TypeReaderResult> ReadAsync(
			ICommandContext context,
			string input,
			IServiceProvider services)
		{
			var settingsFactory = services.GetRequiredService<IGuildSettingsFactory>();
			var settings = await settingsFactory.GetOrCreateAsync(context.Guild).CAF();
			var matches = GetBannedPhrases(settings).Where(x => x.Phrase.CaseInsEquals(input)).ToArray();
			return this.SingleValidResult(matches, $"banned {BannedPhraseName}", input);
		}
		/// <summary>
		/// Gets banned phrases from <paramref name="settings"/>.
		/// </summary>
		/// <param name="settings"></param>
		/// <returns></returns>
		protected abstract IEnumerable<BannedPhrase> GetBannedPhrases(IGuildSettings settings);
	}
}
