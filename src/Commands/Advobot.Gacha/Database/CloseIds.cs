using Advobot.Classes.CloseWords;
using Advobot.Interfaces;

namespace Advobot.Gacha.Database;

public sealed class CloseIds : CloseWords<CloseIds.NameAndId>
{
	private readonly List<NameAndId> _MutableSource;

	public CloseIds() : this([])
	{
	}

	private CloseIds(List<NameAndId> source)
		: base(source, x => x.Name)
	{
		_MutableSource = source;
	}

	internal void Add(long id, string name)
		=> _MutableSource.Add(new NameAndId(name, id));

	public readonly struct NameAndId(string name, long id) : INameable
	{
		public long Id { get; } = id;
		public string Name { get; } = name;
	}
}