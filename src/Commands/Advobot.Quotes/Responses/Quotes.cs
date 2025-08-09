using Advobot.Embeds;
using Advobot.Modules;
using Advobot.Quotes.Models;
using Advobot.Utilities;

using static Advobot.Resources.Responses;

namespace Advobot.Quotes.Responses;

public sealed class Quotes : AdvobotResult
{
	private Quotes() : base(null, "")
	{
	}

	public static AdvobotResult AddedQuote(Quote quote)
	{
		return Success(QuotesAddedQuote.Format(
			quote.Name.WithBlock()
		));
	}

	public static AdvobotResult Quote(Quote quote)
	{
		if (quote.Description != null)
		{
			return Success(quote.Description);
		}
		return Failure(QuotesRemovedQuote.Format(
			quote.Name.WithBlock()
		));
	}

	public static AdvobotResult RemovedQuote(Quote quote)
	{
		return Success(QuotesRemovedQuote.Format(
			quote.Name.WithBlock()
		));
	}

	public static AdvobotResult ShowQuotes(IEnumerable<Quote> quotes)
	{
		return Success(new EmbedWrapper
		{
			Title = VariableQuotes,
			Description = quotes.Select(x => x.Name).Join().WithBigBlock().Value,
		});
	}
}