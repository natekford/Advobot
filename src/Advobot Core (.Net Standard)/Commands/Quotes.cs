using Advobot.Actions;
using Advobot.Classes.Attributes;
using Advobot.Classes;
using Advobot.Enums;
using Advobot.Actions.Formatting;
using Discord.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Advobot.Commands.Quotes
{
	[Group(nameof(ModifyQuotes)), Alias("mrem")]
	[Usage("[Add|Remove] [\"Name\"] <Text>")]
	[Summary("Adds the given text to a list that can be called through the `" + nameof(SayQuote) + "` command.")]
	[PermissionRequirement(null, null)]
	[DefaultEnabled(false)]
	public sealed class ModifyQuotes : SavingModuleBase
	{
		[Command(nameof(ActionType.Add)), Alias("a")]
		public async Task CommandAdd(string name, [Remainder] string text)
		{
			if (Context.GuildSettings.Quotes.Count >= Constants.MAX_QUOTES)
			{
				await MessageActions.SendErrorMessage(Context, new ErrorReason($"You cannot have more than `{Constants.MAX_QUOTES}` quotes at a time."));
				return;
			}
			else if (Context.GuildSettings.Quotes.Any(x => x.Name.CaseInsEquals(name)))
			{
				await MessageActions.SendErrorMessage(Context, new ErrorReason("A quote already has that name."));
				return;
			}
			else if (String.IsNullOrWhiteSpace(text))
			{
				await MessageActions.SendErrorMessage(Context, new ErrorReason("Adding a quote requires text."));
				return;
			}

			Context.GuildSettings.Quotes.Add(new Quote(name, text));
			await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully added the following quote: `{name}`.");
		}
		[Command(nameof(ActionType.Remove)), Alias("r")]
		public async Task CommandRemove(string name)
		{
			if (!Context.GuildSettings.Quotes.Any())
			{
				await MessageActions.SendErrorMessage(Context, new ErrorReason("There needs to be at least one quote before you can remove any."));
				return;
			}

			var removed = Context.GuildSettings.Quotes.RemoveAll(x => x.Name.CaseInsEquals(name));
			if (removed < 1)
			{
				await MessageActions.SendErrorMessage(Context, new ErrorReason("No quote has that name."));
				return;
			}

			await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully removed the following quote: `{name}`.");
		}
	}

	[Group(nameof(SayQuote)), Alias("sq")]
	[Usage("<Name>")]
	[Summary("Shows the content for the given quote. If nothing is input, then shows the list of the current quotes.")]
	[DefaultEnabled(false)]
	public sealed class SayQuote : AdvobotModuleBase
	{
		[Command]
		public async Task Command()
		{
			var quotes = Context.GuildSettings.Quotes;
			if (!quotes.Any())
			{
				await MessageActions.SendErrorMessage(Context, new ErrorReason("There are no quotes."));
				return;
			}

			var desc = $"`{String.Join("`, `", quotes.Select(x => x.Name))}`";
			await MessageActions.SendEmbedMessage(Context.Channel, EmbedActions.MakeNewEmbed("Quotes", desc));
		}
		[Command]
		public async Task Command([Remainder] string name)
		{
			var quotes = Context.GuildSettings.Quotes;
			if (!quotes.Any())
			{
				await MessageActions.SendErrorMessage(Context, new ErrorReason("There are no quotes."));
				return;
			}

			var quote = quotes.FirstOrDefault(x => x.Name.CaseInsEquals(name));
			if (quote != null)
			{
				await MessageActions.SendMessage(Context.Channel, quote.Description);
				return;
			}

			var closeQuotes = new CloseWords<Quote>(Context.User, quotes, name);
			if (closeQuotes.List.Any())
			{
				Context.Timers.AddActiveCloseQuote(closeQuotes);

				var msg = "Did you mean any of the following:\n" + closeQuotes.List.FormatNumberedList("{0}", x => x.Word.Name);
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, msg, Constants.SECONDS_ACTIVE_CLOSE);
				return;
			}

			await MessageActions.SendErrorMessage(Context, new ErrorReason("Nonexistent quote."));
		}
	}
}
