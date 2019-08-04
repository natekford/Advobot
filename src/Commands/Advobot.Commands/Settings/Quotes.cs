using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Advobot.Attributes;
using Advobot.Attributes.ParameterPreconditions.Strings;
using Advobot.Attributes.Preconditions.Permissions;
using Advobot.Attributes.Preconditions.QuantityLimitations;
using Advobot.Commands.Localization;
using Advobot.Commands.Resources;
using Advobot.Modules;
using Advobot.Services.GuildSettings;
using Advobot.Services.GuildSettings.Settings;
using Advobot.TypeReaders;
using AdvorangesUtils;
using Discord;
using Discord.Commands;

namespace Advobot.Commands.Settings
{
	public sealed class Quotes : ModuleBase
	{
		[Group(nameof(ModifyQuotes)), ModuleInitialismAlias(typeof(ModifyQuotes))]
		[LocalizedSummary(nameof(Summaries.ModifyQuotes))]
		[GuildPermissionRequirement(GuildPermission.Administrator)]
		[EnabledByDefault(false)]
		public sealed class ModifyQuotes : SettingsModule<IGuildSettings>
		{
			protected override IGuildSettings Settings => Context.Settings;

			[QuoteLimit(QuantityLimitAction.Add)]
			[ImplicitCommand, ImplicitAlias]
			public Task<RuntimeResult> Add([QuoteName] string name, [Remainder] string text)
			{
				var quote = new Quote(name, text);
				Settings.Quotes.Add(quote);
				return Responses.Quotes.ModifiedQuote(quote, true);
			}
			[QuoteLimit(QuantityLimitAction.Remove)]
			[ImplicitCommand, ImplicitAlias]
			public Task<RuntimeResult> Remove([Remainder] Quote quote)
			{
				Settings.Quotes.Remove(quote);
				return Responses.Quotes.ModifiedQuote(quote, false);
			}
		}

		[Group(nameof(SayQuote)), ModuleInitialismAlias(typeof(SayQuote))]
		[LocalizedSummary(nameof(Summaries.SayQuote))]
		[EnabledByDefault(false)]
		public sealed class SayQuote : AdvobotModuleBase
		{
			[Command]
			public Task<RuntimeResult> Command()
				=> Responses.Quotes.ShowQuotes(Context.Settings.Quotes);
			[Command, Priority(1)]
			public Task<RuntimeResult> Command([Remainder] Quote quote)
				=> Responses.Quotes.Quote(quote);
			[Command(RunMode = RunMode.Async), Priority(0)]
			public async Task<RuntimeResult> Command([Remainder, OverrideTypeReader(typeof(CloseQuoteTypeReader))] IEnumerable<Quote> quote)
			{
				var entry = await NextItemAtIndexAsync(quote.ToArray(), x => x.Name).CAF();
				if (entry != null)
				{
					return Responses.Quotes.Quote(entry);
				}
				return AdvobotResult.IgnoreFailure;
			}
		}
	}
}
