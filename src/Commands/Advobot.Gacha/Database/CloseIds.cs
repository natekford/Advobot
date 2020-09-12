using System.Collections.Generic;

using Advobot.Classes.CloseWords;
using Advobot.Interfaces;

namespace Advobot.Gacha.Database
{
	public sealed class CloseIds : CloseWords<CloseIds.NameAndId>
	{
		public CloseIds() : base(new List<NameAndId>(), x => x.Name)
		{
		}

		internal void Add(long id, string name)
			=> Source.Add(new NameAndId(name, id));

		public sealed class NameAndId : INameable
		{
			public long Id { get; }
			public string Name { get; }

			public NameAndId(string name, long id)
			{
				Name = name;
				Id = id;
			}
		}
	}
}