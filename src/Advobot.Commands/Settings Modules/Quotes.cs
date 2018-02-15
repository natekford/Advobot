using Advobot.Core.Classes;
using Advobot.Core.Classes.Attributes;
using Advobot.Core.Classes.CloseWords;
using Advobot.Core.Classes.Settings;
using Advobot.Core.Utilities;
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
	public sealed class ModifyQuotes : GuildSettingsSavingModuleBase
	{
		[Command(nameof(Add)), ShortAlias(nameof(Add))]
		public async Task Add(string name, [Remainder] string text)
		{
			if (Context.GuildSettings.Quotes.Count >= Context.BotSettings.MaxQuotes)
			{
				var error = new Error($"You cannot have more than `{Context.BotSettings.MaxQuotes}` quotes at a time.");
				await MessageUtils.SendErrorMessageAsync(Context, error).CAF();
				return;
			}

			if (Context.GuildSettings.Quotes.Any(x => x.Name.CaseInsEquals(name)))
			{
				await MessageUtils.SendErrorMessageAsync(Context, new Error("A quote already has that name.")).CAF();
				return;
			}

			if (String.IsNullOrWhiteSpace(text))
			{
				await MessageUtils.SendErrorMessageAsync(Context, new Error("Adding a quote requires text.")).CAF();
				return;
			}

			Context.GuildSettings.Quotes.Add(new Quote(name, text));
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, $"Successfully added the following quote: `{name}`.").CAF();
		}
		[Command(nameof(Remove)), ShortAlias(nameof(Remove))]
		public async Task Remove(string name)
		{
			if (!Context.GuildSettings.Quotes.Any())
			{
				var error = new Error("There needs to be at least one quote before you can remove any.");
				await MessageUtils.SendErrorMessageAsync(Context, error).CAF();
				return;
			}

			var removed = Context.GuildSettings.Quotes.RemoveAll(x => x.Name.CaseInsEquals(name));
			if (removed < 1)
			{
				await MessageUtils.SendErrorMessageAsync(Context, new Error("No quote has that name.")).CAF();
				return;
			}

			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, $"Successfully removed the following quote: `{name}`.").CAF();
		}
	}

	[Group(nameof(SayQuote)), TopLevelShortAlias(typeof(SayQuote))]
	[Summary("Shows the content for the given quote. " +
		"If nothing is input, then shows the list of the current quotes.")]
	[DefaultEnabled(false)]
	public sealed class SayQuote : NonSavingModuleBase
	{
		[Command]
		public async Task Command([Optional, Remainder] string name)
		{
			var quotes = Context.GuildSettings.Quotes;
			if (!quotes.Any())
			{
				await MessageUtils.SendErrorMessageAsync(Context, new Error("There are no quotes.")).CAF();
				return;
			}

			if (name == null)
			{
				var embed = new EmbedWrapper
				{
					Title = "Quotes",
					Description = $"`{String.Join("`, `", quotes.Select(x => x.Name))}`"
				};
				await MessageUtils.SendEmbedMessageAsync(Context.Channel, embed).CAF();
				return;
			}

			var quote = quotes.FirstOrDefault(x => x.Name.CaseInsEquals(name));
			if (quote != null)
			{
				await MessageUtils.SendMessageAsync(Context.Channel, quote.Description).CAF();
				return;
			}

			var closeQuotes = new CloseQuotes(default, Context, Context.GuildSettings, name);
			if (closeQuotes.List.Any())
			{
				var text = $"Did you mean any of the following:\n{closeQuotes.List.FormatNumberedList(x => x.Name)}";
				var msg = await MessageUtils.SendMessageAsync(Context.Channel, text).CAF();
				await Context.Timers.AddAsync(closeQuotes).CAF();
				return;
			}

			await MessageUtils.SendErrorMessageAsync(Context, new Error("Nonexistent quote.")).CAF();
		}
	}
}
