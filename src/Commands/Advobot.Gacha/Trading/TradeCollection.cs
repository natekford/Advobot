using System;
using System.Collections;
using System.Collections.Generic;

using Discord;

namespace Advobot.Gacha.Trading
{
	public sealed class TradeCollection : IReadOnlyList<ITrade>, IList<ITrade>
	{
		private readonly List<ITrade> _Gives = new List<ITrade>();
		private readonly ulong _GuildId;

		public int Count => _Gives.Count;

		public bool IsReadOnly => ((IList<ITrade>)_Gives).IsReadOnly;

		public ITrade this[int index]
		{
			get => _Gives[index];
			set => _Gives[index] = value;
		}

		public TradeCollection(IGuild guild) : this(guild.Id)
		{
		}

		public TradeCollection(ulong guildId)
		{
			_GuildId = guildId;
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
			=> ((IList<ITrade>)_Gives).GetEnumerator();

		public int IndexOf(ITrade item)
			=> _Gives.IndexOf(item);

		public void Insert(int index, ITrade item)
			=> _Gives.Insert(index, item);

		public bool Remove(ITrade item)
							=> _Gives.Remove(item);

		public void RemoveAt(int index)
			=> _Gives.RemoveAt(index);

		IEnumerator IEnumerable.GetEnumerator()
			=> ((IList<ITrade>)_Gives).GetEnumerator();
	}
}