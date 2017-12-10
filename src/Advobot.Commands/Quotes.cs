using Advobot.Core;
using Advobot.Core.Actions;
using Advobot.Core.Actions.Formatting;
using Advobot.Core.Classes;
using Advobot.Core.Classes.Attributes;
using Advobot.Core.Classes.CloseWords;
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
	public sealed class ModifyQuotes : SavingModuleBase
	{
		[Command(nameof(Add)), ShortAlias(nameof(Add))]
		public async Task Add(string name, [Remainder] string text)
		{
			if (Context.GuildSettings.Quotes.Count >= Constants.MAX_QUOTES)
			{
				var error = new ErrorReason($"You cannot have more than `{Constants.MAX_QUOTES}` quotes at a time.");
				await MessageActions.SendErrorMessageAsync(Context, error).CAF();
				return;
			}
			else if (Context.GuildSettings.Quotes.Any(x => x.Name.CaseInsEquals(name)))
			{
				await MessageActions.SendErrorMessageAsync(Context, new ErrorReason("A quote already has that name.")).CAF();
				return;
			}
			else if (String.IsNullOrWhiteSpace(text))
			{
				await MessageActions.SendErrorMessageAsync(Context, new ErrorReason("Adding a quote requires text.")).CAF();
				return;
			}

			Context.GuildSettings.Quotes.Add(new Quote(name, text));
			await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, $"Successfully added the following quote: `{name}`.").CAF();
		}
		[Command(nameof(Remove)), ShortAlias(nameof(Remove))]
		public async Task Remove(string name)
		{
			if (!Context.GuildSettings.Quotes.Any())
			{
				var error = new ErrorReason("There needs to be at least one quote before you can remove any.");
				await MessageActions.SendErrorMessageAsync(Context, error).CAF();
				return;
			}

			var removed = Context.GuildSettings.Quotes.RemoveAll(x => x.Name.CaseInsEquals(name));
			if (removed < 1)
			{
				await MessageActions.SendErrorMessageAsync(Context, new ErrorReason("No quote has that name.")).CAF();
				return;
			}

			await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, $"Successfully removed the following quote: `{name}`.").CAF();
		}
	}

	[Group(nameof(SayQuote)), TopLevelShortAlias(typeof(SayQuote))]
	[Summary("Shows the content for the given quote. " +
		"If nothing is input, then shows the list of the current quotes.")]
	[DefaultEnabled(false)]
	public sealed class SayQuote : AdvobotModuleBase
	{
		[Command]
		public async Task Command([Optional, Remainder] string name)
		{
			var quotes = Context.GuildSettings.Quotes;
			if (!quotes.Any())
			{
				await MessageActions.SendErrorMessageAsync(Context, new ErrorReason("There are no quotes.")).CAF();
				return;
			}
			else if (name == null)
			{
				var desc = $"`{String.Join("`, `", quotes.Select(x => x.Name))}`";
				await MessageActions.SendEmbedMessageAsync(Context.Channel, new EmbedWrapper("Quotes", desc)).CAF();
				return;
			}

			var quote = quotes.FirstOrDefault(x => x.Name.CaseInsEquals(name));
			if (quote != null)
			{
				await MessageActions.SendMessageAsync(Context.Channel, quote.Description).CAF();
				return;
			}

			var closeQuotes = new CloseQuotes(Context.GuildSettings, name);
			if (closeQuotes.List.Any())
			{
				var text = $"Did you mean any of the following:\n{closeQuotes.List.FormatNumberedList("{0}", x => x.Word.Name)}";
				var msg = await MessageActions.SendMessageAsync(Context.Channel, text).CAF();
				await Context.Timers.AddActiveCloseQuote(Context.User as IGuildUser, msg, closeQuotes).CAF();
				return;
			}

			await MessageActions.SendErrorMessageAsync(Context, new ErrorReason("Nonexistent quote.")).CAF();
		}
	}
}
