using Advobot.Classes;
using Advobot.Classes.Results;
using Advobot.Classes.Settings;
using Advobot.Utilities;
using System.Collections.Generic;
using System.Linq;

namespace Advobot.Commands.Responses
{
	public sealed class Quotes : CommandResponses
	{
		private Quotes() { }

		public static AdvobotResult ModifiedQuote(Quote quote, bool added)
			=> Success(Default.FormatInterpolated($"Successfully {GetAdded(added)} the quote {quote.Name}."));
		public static AdvobotResult ShowQuotes(IEnumerable<Quote> quotes)
		{
			return Success(new EmbedWrapper
			{
				Title = "Quotes",
				Description = Default.FormatInterpolated($"{quotes.Select(x => x.Name)}"),
			});
		}
		public static AdvobotResult Quote(Quote quote)
			=> Success(quote.Description);
	}
}
