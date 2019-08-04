using System;
using System.Linq;
using System.Threading.Tasks;
using Advobot.Classes.CloseWords;
using Advobot.Services.GuildSettings;
using Advobot.Services.GuildSettings.Settings;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace Advobot.TypeReaders
{
	/// <summary>
	/// Finds quotes with names similar to the passed in input.
	/// </summary>
	public sealed class CloseQuoteTypeReader : TypeReader
	{
		/// <inheritdoc />
		public override async Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
		{
			var settingsFactory = services.GetRequiredService<IGuildSettingsFactory>();
			var settings = await settingsFactory.GetOrCreateAsync(context.Guild).CAF();
			var matches = new CloseWords<Quote>(settings.Quotes).FindMatches(input).Select(x => x.Value).ToArray();
			return TypeReaderUtils.MultipleValidResults(matches, "quotes", input);
		}
	}
}