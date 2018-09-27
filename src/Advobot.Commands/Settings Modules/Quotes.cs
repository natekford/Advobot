using System.Linq;
using System.Threading.Tasks;
using Advobot.Classes;
using Advobot.Classes.Attributes;
using Advobot.Classes.CloseWords;
using Advobot.Classes.Settings;
using Advobot.Interfaces;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Commands.Quotes
{
	[Category(typeof(ModifyQuotes)), Group(nameof(ModifyQuotes)), TopLevelShortAlias(typeof(ModifyQuotes))]
	[Summary("Adds the given text to a list that can be called through the `" + nameof(SayQuote) + "` command.")]
	[PermissionRequirement(null, null)]
	[DefaultEnabled(false)]
	[SaveGuildSettings]
	public sealed class ModifyQuotes : AdvobotModuleBase
	{
		[Command(nameof(Add)), ShortAlias(nameof(Add))]
		public async Task Add(string name, [Remainder] string text)
		{
			if (Context.GuildSettings.Quotes.Count >= BotSettings.MaxQuotes)
			{
				await ReplyErrorAsync(new Error($"There cannot be more than `{BotSettings.MaxQuotes}` quotes at a time.")).CAF();
				return;
			}
			if (Context.GuildSettings.Quotes.Any(x => x.Name.CaseInsEquals(name)))
			{
				await ReplyErrorAsync(new Error($"A quote already has the name `{name}`.")).CAF();
				return;
			}
			if (string.IsNullOrWhiteSpace(text))
			{
				await ReplyErrorAsync(new Error("A quote requires text to be added.")).CAF();
				return;
			}

			Context.GuildSettings.Quotes.Add(new Quote(name, text));
			await ReplyTimedAsync($"Successfully added the following quote: `{name}`.").CAF();
		}
		[Command(nameof(Remove)), ShortAlias(nameof(Remove))]
		public async Task Remove(string name)
		{
			var removed = Context.GuildSettings.Quotes.RemoveAll(x => x.Name.CaseInsEquals(name));
			if (removed < 1)
			{
				await ReplyErrorAsync(new Error($"No quote has the name `{name}`.")).CAF();
				return;
			}

			await ReplyTimedAsync($"Successfully removed the following quote: `{name}`.").CAF();
		}
	}

	[Category(typeof(SayQuote)), Group(nameof(SayQuote)), TopLevelShortAlias(typeof(SayQuote))]
	[Summary("Shows the content for the given quote. " +
		"If nothing is input, then shows the list of the current quotes.")]
	[DefaultEnabled(false)]
	public sealed class SayQuote : AdvobotModuleBase
	{
		[Command]
		public async Task Command()
			=> await ReplyIfAny(Context.GuildSettings.Quotes, "quotes", x => x.Name).CAF();
		[Command, Priority(1)]
		public async Task Command([Remainder] Quote quote)
			=> await ReplyAsync(quote.Description).CAF();
		[Command, Priority(0)]
		public async Task Command([Remainder] string quote)
		{
			var matches = new CloseQuotes(Context.GuildSettings.Quotes, quote).Matches;
			await ReplyIfAny(matches, $"No quote has the name `{quote}`.", async x =>
			{
				var message = await ReplyAsync($"Did you mean any of the following:\n{x.FormatNumberedList(cw => cw.Name)}").CAF();
				await Timers.AddAsync(new RemovableCloseWords("Quotes", x, Context, new[] { Context.Message, message })).CAF();
				return message;
			}).CAF();
		}
	}
}
