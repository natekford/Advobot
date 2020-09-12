using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Advobot.Quotes.Database;
using Advobot.Quotes.ReadOnlyModels;

namespace Advobot.Tests.Commands.Quotes
{
	public sealed class FakeQuoteDatabase : IQuoteDatabase
	{
		private readonly Dictionary<(ulong, string), IReadOnlyQuote> _Quotes
			= new Dictionary<(ulong, string), IReadOnlyQuote>();

		public Task<int> AddQuoteAsync(IReadOnlyQuote quote)
		{
			var result = _Quotes.TryAdd(ToKey(quote), quote);
			return Task.FromResult(result ? 1 : 0);
		}

		public Task<int> DeleteQuoteAsync(IReadOnlyQuote quote)
		{
			var result = _Quotes.Remove(ToKey(quote));
			return Task.FromResult(result ? 1 : 0);
		}

		public Task<IReadOnlyQuote?> GetQuoteAsync(ulong guildId, string name)
		{
			var result = _Quotes.TryGetValue((guildId, name), out var val) ? val : null;
			return Task.FromResult(result);
		}

		public Task<IReadOnlyList<IReadOnlyQuote>> GetQuotesAsync(ulong guildId)
		{
			var result = _Quotes.Values.Where(x => x.GuildId == guildId).ToArray();
			return Task.FromResult<IReadOnlyList<IReadOnlyQuote>>(result);
		}

		private (ulong, string) ToKey(IReadOnlyQuote quote)
			=> (quote.GuildId, quote.Name);
	}
}