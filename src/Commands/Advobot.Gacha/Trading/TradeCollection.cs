using System;
using System.Collections;
using System.Collections.Generic;

namespace Advobot.Gacha.Trading
{
	public sealed class TradeCollection : ICollection<ITrade>
	{
		public int Count => _Gives.Count;
		public bool IsReadOnly => ((ICollection<ITrade>)_Gives).IsReadOnly;

		private readonly List<ITrade> _Gives = new List<ITrade>();
		private readonly string _GuildId;

		public TradeCollection(ulong guildId)
		{
			_GuildId = guildId.ToString();
		}

		public void Add(ITrade item)
		{
			//All trades are done on a guild basis, not global basis
			if (item.GuildId != _GuildId)
			{
				throw new ArgumentException($"{nameof(item)} must belong to guild {_GuildId}.");
			}
			_Gives.Add(item);
		}
		public void Clear()
			=> _Gives.Clear();
		public bool Contains(ITrade item)
			=> _Gives.Contains(item);
		public void CopyTo(ITrade[] array, int arrayIndex)
			=> _Gives.CopyTo(array, arrayIndex);
		public IEnumerator<ITrade> GetEnumerator()
			=> ((ICollection<ITrade>)_Gives).GetEnumerator();
		public bool Remove(ITrade item)
			=> _Gives.Remove(item);

		IEnumerator IEnumerable.GetEnumerator()
			=> ((ICollection<ITrade>)_Gives).GetEnumerator();
	}
}
