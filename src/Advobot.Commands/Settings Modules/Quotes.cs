using Advobot.Classes;
using Advobot.Classes.Attributes;
using Advobot.Classes.CloseWords;
using Advobot.Classes.Settings;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using Discord.Commands;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Advobot.Commands.Quotes
{
	[Group(nameof(ModifyQuotes)), TopLevelShortAlias(typeof(ModifyQuotes))]
	[Summary("Adds the given text to a list that can be called through the `" + nameof(SayQuote) + "` command.")]
	[PermissionRequirement(null, null)]
	[DefaultEnabled(false)]
	[SaveGuildSettings]
	public sealed class ModifyQuotes : AdvobotModuleBase
	{
		[Command(nameof(Add)), ShortAlias(nameof(Add))]
		public async Task Add(string name, [Remainder] string text)
		{
			if (Context.GuildSettings.Quotes.Count >= Context.BotSettings.MaxQuotes)
			{
				var error = new Error($"There cannot be more than `{Context.BotSettings.MaxQuotes}` quotes at a time.");
				await MessageUtils.SendErrorMessageAsync(Context, error).CAF();
				return;
			}
			if (Context.GuildSettings.Quotes.Any(x => x.Name.CaseInsEquals(name)))
			{
				await MessageUtils.SendErrorMessageAsync(Context, new Error($"A quote already has the name `{name}`.")).CAF();
				return;
			}
			if (String.IsNullOrWhiteSpace(text))
			{
				await MessageUtils.SendErrorMessageAsync(Context, new Error("A quote requires text to be added.")).CAF();
				return;
			}

			Context.GuildSettings.Quotes.Add(new Quote(name, text));
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, $"Successfully added the following quote: `{name}`.").CAF();
		}
		[Command(nameof(Remove)), ShortAlias(nameof(Remove))]
		public async Task Remove(string name)
		{
			var removed = Context.GuildSettings.Quotes.RemoveAll(x => x.Name.CaseInsEquals(name));
			if (removed < 1)
			{
				await MessageUtils.SendErrorMessageAsync(Context, new Error($"No quote has the name `{name}`.")).CAF();
				return;
			}

			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, $"Successfully removed the following quote: `{name}`.").CAF();
		}
	}

	[Group(nameof(SayQuote)), TopLevelShortAlias(typeof(SayQuote))]
	[Summary("Shows the content for the given quote. " +
		"If nothing is input, then shows the list of the current quotes.")]
	[DefaultEnabled(false)]
	public sealed class SayQuote : AdvobotModuleBase
	{
		[Command]
		public async Task Command()
		{
			var quotes = Context.GuildSettings.Quotes;
			if (!quotes.Any())
			{
				await MessageUtils.SendErrorMessageAsync(Context, new Error("There are currently no quotes.")).CAF();
				return;
			}

			var embed = new EmbedWrapper
			{
				Title = "Quotes",
				Description = $"`{String.Join("`, `", quotes.Select(x => x.Name))}`"
			};
			await MessageUtils.SendMessageAsync(Context.Channel, null, embed).CAF();
		}
		[Command]
		public async Task Command([Optional, Remainder] string name)
		{
			var quote = Context.GuildSettings.Quotes.SingleOrDefault(x => x.Name.CaseInsEquals(name));
			if (quote != null)
			{
				await MessageUtils.SendMessageAsync(Context.Channel, quote.Description).CAF();
				return;
			}

			var closeQuotes = new CloseQuotes(default, Context, Context.GuildSettings, name);
			if (closeQuotes.List.Any())
			{
				await closeQuotes.SendBotMessageAsync(Context.Channel).CAF();
				await Context.Timers.AddAsync(closeQuotes).CAF();
				return;
			}

			await MessageUtils.SendErrorMessageAsync(Context, new Error($"No quote has the name `{name}`.")).CAF();
		}
	}
}
