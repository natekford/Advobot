using Advobot.Actions;
using Advobot.TypeReaders;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Advobot.NonSavedClasses;
using Advobot.Enums;
using Advobot.Attributes;
using System.Reflection;
using Advobot.Interfaces;
using Advobot.SavedClasses;
using Advobot.Structs;

namespace Advobot
{
	namespace Quotes
	{
		[Group(nameof(ModifyQuotes)), Alias("mrem")]
		[Usage("[Add|Remove] [\"Name\"] <\"Text\">")]
		[Summary("Adds the given text to a list that can be called through the `" + nameof(SayQuote) + "` command.")]
		[PermissionRequirement(null, null)]
		[DefaultEnabled(false)]
		public sealed class ModifyQuotes : MySavingModuleBase
		{
			[Command(nameof(ActionType.Add)), Alias("a")]
			public async Task CommandAdd()
			{

			}
			[Command(nameof(ActionType.Remove)), Alias("r")]
			public async Task CommandRemove()
			{

			}
		}

		[Group(nameof(SayQuote)), Alias("sq")]
		[Usage("<Name>")]
		[Summary("Shows the content for the given quote. If nothing is input, then shows the list of the current quotes.")]
		[DefaultEnabled(false)]
		public sealed class SayQuote : MyModuleBase
		{
			[Command]
			public async Task Command()
			{
				var quotes = Context.GuildSettings.Quotes;
				if (!quotes.Any())
				{
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, FormattingActions.ERROR("There are no quotes."));
					return;
				}

				var desc = String.Format("`{0}`", String.Join("`, `", quotes.Select(x => x.Name)));
				await MessageActions.SendEmbedMessage(Context.Channel, EmbedActions.MakeNewEmbed("Quotes", desc));
			}
			[Command]
			public async Task Command([Remainder] string name)
			{
				var quotes = Context.GuildSettings.Quotes;
				if (!quotes.Any())
				{
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, FormattingActions.ERROR("There are no quotes."));
					return;
				}

				var quote = quotes.FirstOrDefault(x => x.Name.CaseInsEquals(name));
				if (quote != null)
				{
					await MessageActions.SendChannelMessage(Context, quote.Text);
					return;
				}

				var closeQuotes = CloseWordActions.GetObjectsWithSimilarNames(quotes, name);
				if (closeQuotes.Any())
				{
					Context.Timers.GetOutActiveCloseQuote(Context.User.Id);
					Context.Timers.AddActiveCloseQuotes(new ActiveCloseWord<Quote>(Context.User.Id, closeQuotes));

					var msg = "Did you mean any of the following:\n" + closeQuotes.FormatNumberedList("{0}", x => x.Word.Name);
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, msg, Constants.SECONDS_ACTIVE_CLOSE);
					return;
				}

				await MessageActions.MakeAndDeleteSecondaryMessage(Context, FormattingActions.ERROR("Nonexistent quote."));
			}
		}
	}
}
