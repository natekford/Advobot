using System.Collections.Generic;
using System.Threading.Tasks;

using Advobot.Quotes.ReadOnlyModels;

namespace Advobot.Quotes.Database
{
	public interface IQuoteDatabase
	{
		Task<int> AddQuoteAsync(IReadOnlyQuote quote);

		Task<int> DeleteQuoteAsync(IReadOnlyQuote quote);

		Task<IReadOnlyQuote?> GetQuoteAsync(ulong guildId, string name);

		Task<IReadOnlyList<IReadOnlyQuote>> GetQuotesAsync(ulong guildId);
	}
}