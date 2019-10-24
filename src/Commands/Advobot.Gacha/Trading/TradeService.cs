using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;

namespace Advobot.Gacha.Trading
{
	public interface ITradeService
	{
		Task<IReadOnlyList<IReadOnlyList<ITrade>>> GiveAsync(TradeCollection collection);

		Task<IReadOnlyList<IReadOnlyList<ITrade>>> TradeAsync(TradeCollection collection);
	}

	public sealed class TradeService
	{
	}
}