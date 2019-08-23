using System;
using System.Linq;
using System.Threading.Tasks;
using Advobot.Attributes;
using Advobot.Services.GuildSettings;
using Advobot.Services.GuildSettings.Settings;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace Advobot.TypeReaders
{
	/// <summary>
	/// Attempts to find a quote with the supplied name.
	/// </summary>
	[TypeReaderTargetType(typeof(Quote))]
	public sealed class QuoteTypeReader : TypeReader
	{
		/// <summary>
		/// Attempts to find a quote with the supplied input as a name.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="input"></param>
		/// <param name="services"></param>
		/// <returns></returns>
		public override async Task<TypeReaderResult> ReadAsync(
			ICommandContext context,
			string input,
			IServiceProvider services)
		{
			var settingsFactory = services.GetRequiredService<IGuildSettingsFactory>();
			var settings = await settingsFactory.GetOrCreateAsync(context.Guild).CAF();
			var matches = settings.Quotes.Where(x => x.Name.CaseInsEquals(input)).ToArray();
			return TypeReaderUtils.SingleValidResult(matches, "quotes", input);
		}
	}
}