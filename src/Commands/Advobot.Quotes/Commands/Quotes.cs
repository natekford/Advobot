using System.Collections.Generic;
using System.Threading.Tasks;

using Advobot.Attributes;
using Advobot.Attributes.Preconditions.Permissions;
using Advobot.Modules;
using Advobot.Quotes.Database;
using Advobot.Quotes.Localization;
using Advobot.Quotes.Models;
using Advobot.Quotes.ParameterPreconditions;
using Advobot.Quotes.ReadOnlyModels;
using Advobot.Quotes.Resources;

using AdvorangesUtils;

using Discord.Commands;

using static Advobot.Quotes.Responses.Quotes;

namespace Advobot.Quotes.Commands
{
	[Category(nameof(Quotes))]
	public sealed class Quotes : ModuleBase
	{
		[LocalizedGroup(nameof(Groups.ModifyQuotes))]
		[LocalizedAlias(nameof(Aliases.ModifyQuotes))]
		[LocalizedSummary(nameof(Summaries.ModifyQuotes))]
		[Meta("6a6c952a-ea22-4478-9433-99304ae440b7")]
		[RequireGenericGuildPermissions]
		public sealed class ModifyQuotes : QuoteModuleBase
		{
			[LocalizedCommand(nameof(Groups.Add))]
			[LocalizedAlias(nameof(Aliases.Add))]
			public async Task<RuntimeResult> Add(
				[QuoteName]
				string name,
				[Remainder]
				string text)
			{
				var quote = new Quote
				{
					Name = name,
					Description = text,
					GuildId = Context.Guild.Id,
				};
				await Db.AddQuoteAsync(quote).CAF();
				return AddedQuote(quote);
			}

			[LocalizedCommand(nameof(Groups.Remove))]
			[LocalizedAlias(nameof(Aliases.Remove))]
			public async Task<RuntimeResult> Remove([Remainder] IReadOnlyQuote quote)
			{
				await Db.DeleteQuoteAsync(quote).CAF();
				return RemovedQuote(quote);
			}
		}

		[LocalizedGroup(nameof(Groups.SayQuote))]
		[LocalizedAlias(nameof(Aliases.SayQuote))]
		[LocalizedSummary(nameof(Summaries.SayQuote))]
		[Meta("70dd6bb8-789c-4d72-931d-c72cb58041f2")]
		public sealed class SayQuote : QuoteModuleBase
		{
			[Command]
			public async Task<RuntimeResult> Command()
			{
				var quotes = await Db.GetQuotesAsync(Context.Guild.Id).CAF();
				return ShowQuotes(quotes);
			}

			[Command, Priority(1)]
			public Task<RuntimeResult> Command([Remainder] IReadOnlyQuote quote)
				=> Quote(quote);

			[Command(RunMode = RunMode.Async), Priority(0)]
			[Hidden]
			public async Task<RuntimeResult> Command(
				[Remainder]
				IReadOnlyList<IReadOnlyQuote> quote)
			{
				var entry = await NextItemAtIndexAsync(quote, x => x.Name).CAF();
				if (entry.HasValue)
				{
					return Quote(entry.Value);
				}
				return AdvobotResult.IgnoreFailure;
			}
		}
	}
}