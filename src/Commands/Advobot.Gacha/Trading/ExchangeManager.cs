using System.Collections.Concurrent;
using System.Collections.ObjectModel;

using Advobot.Gacha.Database;
using Advobot.Gacha.Models;
using Advobot.Gacha.Utilities;

using AdvorangesUtils;

using Discord;

namespace Advobot.Gacha.Trading
{
	public sealed class ExchangeManager
	{
		private readonly IGachaDatabase _Db;
		private readonly ConcurrentDictionary<(ulong, ulong), Exchange> _Dictionary = new();

		public ExchangeManager(IGachaDatabase db)
		{
			_Db = db;
		}

		public bool AddExchange(ExchangeMethod method, User receiver, IEnumerable<Character> characters)
		{
			var exchange = new Exchange(method);
			exchange.AddRange(characters.Select(x => new Trade(receiver, x)));

			return method == ExchangeMethod.Gift || _Dictionary.ContainsKey((receiver.GuildId, receiver.UserId));
		}

		public void Cancel(IGuildUser user)
			=> _Dictionary.TryRemove(user.ToKey(), out _);

		public async Task<bool> FinalizeAsync(IGuildUser user)
		{
			//No valid trades for the user means they removed them and we should cancel
			if (!_Dictionary.TryRemove(user.ToKey(), out var exchange))
			{
				return false;
			}
			//If it's a gift we can skip checking the other person's offer
			else if (exchange.Method == ExchangeMethod.Gift)
			{
				await _Db.TradeAsync(exchange).CAF();
				return true;
			}

			//No valid trades for the other user means we should cancel as well
			var otherUserKey = (user.GuildId, exchange[0].ReceiverId);
			if (!_Dictionary.TryRemove(otherUserKey, out var exchange2))
			{
				return false;
			}

			await _Db.TradeAsync(exchange.Concat(exchange2)).CAF();
			return true;
		}

		private sealed class Exchange : Collection<Trade>
		{
			public ExchangeMethod Method { get; }

			public Exchange(ExchangeMethod method)
			{
				Method = method;
			}

			protected override void InsertItem(int index, Trade item)
			{
				if (Count != 0 && item.GuildId != Items[0].GuildId)
				{
					throw new InvalidOperationException("Trades can only be done across a guild.");
				}
				base.InsertItem(index, item);
			}
		}
	}
}