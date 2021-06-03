using System.Collections.Generic;
using System.Threading.Tasks;

using Advobot.Quotes.Models;

namespace Advobot.Quotes.Database
{
	public interface IQuoteDatabase
	{
		Task<int> AddQuoteAsync(Quote quote);

		Task<int> DeleteQuoteAsync(Quote quote);

		Task<Quote?> GetQuoteAsync(ulong guildId, string name);

		Task<IReadOnlyList<Quote>> GetQuotesAsync(ulong guildId);
	}
}