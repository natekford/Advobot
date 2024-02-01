using Advobot.Quotes.Database;
using Advobot.Quotes.Models;

namespace Advobot.Tests.Commands.Quotes;

public sealed class FakeQuoteDatabase : IQuoteDatabase
{
	private readonly Dictionary<(ulong, string), Quote> _Quotes = [];

	public Task<int> AddQuoteAsync(Quote quote)
	{
		var result = _Quotes.TryAdd(ToKey(quote), quote);
		return Task.FromResult(result ? 1 : 0);
	}

	public Task<int> DeleteQuoteAsync(Quote quote)
	{
		var result = _Quotes.Remove(ToKey(quote));
		return Task.FromResult(result ? 1 : 0);
	}

	public Task<Quote?> GetQuoteAsync(ulong guildId, string name)
	{
		var result = _Quotes.TryGetValue((guildId, name), out var val) ? val : null;
		return Task.FromResult(result);
	}

	public Task<IReadOnlyList<Quote>> GetQuotesAsync(ulong guildId)
	{
		var result = _Quotes.Values.Where(x => x.GuildId == guildId).ToArray();
		return Task.FromResult<IReadOnlyList<Quote>>(result);
	}

	private (ulong, string) ToKey(Quote quote)
		=> (quote.GuildId, quote.Name);
}