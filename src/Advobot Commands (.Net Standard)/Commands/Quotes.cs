using Advobot.Actions;
using Advobot.Actions.Formatting;
using Advobot.Classes;
using Advobot.Classes.Attributes;
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
				await MessageActions.SendErrorMessageAsync(Context, new ErrorReason($"You cannot have more than `{Constants.MAX_QUOTES}` quotes at a time."));
				return;
			}
			else if (Context.GuildSettings.Quotes.Any(x => x.Name.CaseInsEquals(name)))
			{
				await MessageActions.SendErrorMessageAsync(Context, new ErrorReason("A quote already has that name."));
				return;
			}
			else if (String.IsNullOrWhiteSpace(text))
			{
				await MessageActions.SendErrorMessageAsync(Context, new ErrorReason("Adding a quote requires text."));
				return;
			}

			Context.GuildSettings.Quotes.Add(new Quote(name, text));
			await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, $"Successfully added the following quote: `{name}`.");
		}
		[Command(nameof(Remove)), ShortAlias(nameof(Remove))]
		public async Task Remove(string name)
		{
			if (!Context.GuildSettings.Quotes.Any())
			{
				await MessageActions.SendErrorMessageAsync(Context, new ErrorReason("There needs to be at least one quote before you can remove any."));
				return;
			}

			var removed = Context.GuildSettings.Quotes.RemoveAll(x => x.Name.CaseInsEquals(name));
			if (removed < 1)
			{
				await MessageActions.SendErrorMessageAsync(Context, new ErrorReason("No quote has that name."));
				return;
			}

			await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, $"Successfully removed the following quote: `{name}`.");
		}
	}

	[Group(nameof(SayQuote)), TopLevelShortAlias(typeof(SayQuote))]
	[Summary("Shows the content for the given quote. If nothing is input, then shows the list of the current quotes.")]
	[DefaultEnabled(false)]
	public sealed class SayQuote : AdvobotModuleBase
	{
		[Command]
		public async Task Command([Optional, Remainder] string name)
		{
			var quotes = Context.GuildSettings.Quotes;
			if (!quotes.Any())
			{
				await MessageActions.SendErrorMessageAsync(Context, new ErrorReason("There are no quotes."));
				return;
			}
			else if (name == null)
			{
				var desc = $"`{String.Join("`, `", quotes.Select(x => x.Name))}`";
				await MessageActions.SendEmbedMessageAsync(Context.Channel, new AdvobotEmbed("Quotes", desc));
				return;
			}

			var quote = quotes.FirstOrDefault(x => x.Name.CaseInsEquals(name));
			if (quote != null)
			{
				await MessageActions.SendMessageAsync(Context.Channel, quote.Description);
				return;
			}

			var closeQuotes = new CloseWords<Quote>(Context.User as IGuildUser, quotes, name);
			if (closeQuotes.List.Any())
			{
				Context.Timers.AddActiveCloseQuote(closeQuotes);

				var msg = "Did you mean any of the following:\n" + closeQuotes.List.FormatNumberedList("{0}", x => x.Word.Name);
				await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, msg, Constants.SECONDS_ACTIVE_CLOSE);
				return;
			}

			await MessageActions.SendErrorMessageAsync(Context, new ErrorReason("Nonexistent quote."));
		}
	}
}
