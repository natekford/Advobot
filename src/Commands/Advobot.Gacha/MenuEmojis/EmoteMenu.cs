using Discord;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Advobot.Gacha.MenuEmojis
{
	public sealed class EmojiMenu : ICollection<IMenuEmote>
	{
		public const int EMOJI_MENU_LIMIT = 20;

		/// <inheritdoc />
		public int Count => _Emotes.Count;
		/// <inheritdoc />
		public bool IsReadOnly => ((ICollection<IMenuEmote>)_Emotes).IsReadOnly;
		/// <summary>
		/// Returns the emojis in this menu.
		/// </summary>
		public IMenuEmote[] Values => _Emotes.ToArray();

		private readonly List<IMenuEmote> _Emotes = new List<IMenuEmote>();

		/// <summary>
		/// Attempts to find the first menu emoji that has the same name as <paramref name="emote"/>.
		/// </summary>
		/// <param name="emote"></param>
		/// <param name="menuItem"></param>
		/// <returns></returns>
		public bool TryGet(IEmote emote, out IMenuEmote? menuItem)
		{
			foreach (var e in _Emotes)
			{
				if (emote.Name == e.Name)
				{
					menuItem = e;
					return true;
				}
			}
			menuItem = null;
			return false;
		}
		/// <inheritdoc />
		public void Add(IMenuEmote item)
		{
			if (_Emotes.Count == EMOJI_MENU_LIMIT)
			{
				throw new InvalidOperationException($"Cannot have more than {EMOJI_MENU_LIMIT} emojis in one menu.");
			}
			_Emotes.Add(item);
		}
		/// <inheritdoc />
		public void Clear()
			=> _Emotes.Clear();
		/// <inheritdoc />
		public bool Contains(IMenuEmote item)
			=> _Emotes.Contains(item);
		/// <inheritdoc />
		public void CopyTo(IMenuEmote[] array, int arrayIndex)
			=> _Emotes.CopyTo(array, arrayIndex);
		/// <inheritdoc />
		public IEnumerator<IMenuEmote> GetEnumerator()
			=> ((ICollection<IMenuEmote>)_Emotes).GetEnumerator();
		/// <inheritdoc />
		public bool Remove(IMenuEmote item)
			=> _Emotes.Remove(item);

		//IEnumerator
		IEnumerator IEnumerable.GetEnumerator() => ((ICollection<IMenuEmote>)_Emotes).GetEnumerator();
	}
}
