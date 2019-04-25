using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Advobot.Classes.Attributes;
using Advobot.Classes.Attributes.Preconditions.Permissions;
using Advobot.Classes.Modules;
using Advobot.Classes.Settings;
using Advobot.Classes.TypeReaders;
using Advobot.Interfaces;
using AdvorangesUtils;
using Discord;
using Discord.Commands;

namespace Advobot.Commands
{
	public sealed class Quotes : ModuleBase
	{
		[Group(nameof(ModifyQuotes)), ModuleInitialismAlias(typeof(ModifyQuotes))]
		[Summary("Adds the given text to a list that can be called through the `" + nameof(SayQuote) + "` command.")]
		[UserPermissionRequirement(GuildPermission.Administrator)]
		[EnabledByDefault(false)]
		public sealed class ModifyQuotes : AdvobotSettingsModuleBase<IGuildSettings>
		{
			protected override IGuildSettings Settings => Context.GuildSettings;

			[ImplicitCommand, ImplicitAlias]
			public Task Add(string name, [Remainder] string text)
			{
				if (Settings.Quotes.Count >= BotSettings.MaxQuotes)
				{
					return ReplyErrorAsync($"There cannot be more than `{BotSettings.MaxQuotes}` quotes at a time.");
				}
				if (Settings.Quotes.Any(x => x.Name.CaseInsEquals(name)))
				{
					return ReplyErrorAsync($"A quote already has the name `{name}`.");
				}
				if (string.IsNullOrWhiteSpace(text))
				{
					return ReplyErrorAsync("A quote requires text to be added.");
				}

				Settings.Quotes.Add(new Quote(name, text));
				return ReplyTimedAsync($"Successfully added the following quote: `{name}`.");
			}
			[ImplicitCommand, ImplicitAlias]
			public Task Remove(string name)
			{
				if (Settings.Quotes.RemoveAll(x => x.Name.CaseInsEquals(name)) < 1)
				{
					return ReplyErrorAsync($"No quote has the name `{name}`.");
				}
				return ReplyTimedAsync($"Successfully removed the following quote: `{name}`.");
			}
		}

		[Group(nameof(SayQuote)), ModuleInitialismAlias(typeof(SayQuote))]
		[Summary("Shows the content for the given quote. " +
			"If nothing is input, then shows the list of the current quotes.")]
		[EnabledByDefault(false)]
		public sealed class SayQuote : AdvobotModuleBase
		{
			[Command]
			public Task Command()
				=> ReplyIfAny(Context.GuildSettings.Quotes, "quotes", x => x.Name);
			[Command, Priority(1)]
			public Task Command([Remainder] Quote quote)
				=> ReplyAsync(quote.Description);
			[Command(RunMode = RunMode.Async), Priority(0)]
			public async Task Command([Remainder, OverrideTypeReader(typeof(CloseQuoteTypeReader))] IEnumerable<Quote> quote)
			{
				var entry = await NextItemAtIndexAsync(quote.ToArray(), x => x.Name).CAF();
				if (entry != null)
				{
					await ReplyAsync(entry.Description).CAF();
				}
			}
		}
	}
}
