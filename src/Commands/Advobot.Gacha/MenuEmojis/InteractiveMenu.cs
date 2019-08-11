using System.Collections;
using System.Collections.Generic;

namespace Advobot.Gacha.MenuEmojis
{
	public sealed class InteractiveMenu : ICollection<IMenuAction>
	{
		/// <inheritdoc />
		public int Count => _Actions.Count;
		/// <inheritdoc />
		public bool IsReadOnly => ((ICollection<IMenuAction>)_Actions).IsReadOnly;
		/// <summary>
		/// Returns the emojis in this menu.
		/// </summary>
		public IMenuAction[] Values => _Actions.ToArray();

		private readonly List<IMenuAction> _Actions = new List<IMenuAction>();

		/// <summary>
		/// Attempts to find the first menu emoji that has the same name as <paramref name="name"/>.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="menuItem"></param>
		/// <returns></returns>
		public bool TryGet(string name, out IMenuAction? menuItem)
		{
			foreach (var e in _Actions)
			{
				if (name == e.Name)
				{
					menuItem = e;
					return true;
				}
			}
			menuItem = null;
			return false;
		}
		/// <inheritdoc />
		public void Add(IMenuAction item)
			=> _Actions.Add(item);
		/// <inheritdoc />
		public void Clear()
			=> _Actions.Clear();
		/// <inheritdoc />
		public bool Contains(IMenuAction item)
			=> _Actions.Contains(item);
		/// <inheritdoc />
		public void CopyTo(IMenuAction[] array, int arrayIndex)
			=> _Actions.CopyTo(array, arrayIndex);
		/// <inheritdoc />
		public IEnumerator<IMenuAction> GetEnumerator()
			=> ((ICollection<IMenuAction>)_Actions).GetEnumerator();
		/// <inheritdoc />
		public bool Remove(IMenuAction item)
			=> _Actions.Remove(item);

		//IEnumerator
		IEnumerator IEnumerable.GetEnumerator() => ((ICollection<IMenuAction>)_Actions).GetEnumerator();
	}
}
