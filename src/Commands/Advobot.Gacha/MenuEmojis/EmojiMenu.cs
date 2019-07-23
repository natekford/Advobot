using Discord;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Advobot.Gacha.MenuEmojis
{
	public sealed class EmojiMenu : ICollection<Emoji>
	{
		public const int EMOJI_MENU_LIMIT = 20;

		/// <inheritdoc />
		public int Count => _Emojis.Count;
		/// <inheritdoc />
		public bool IsReadOnly => ((ICollection<Emoji>)_Emojis).IsReadOnly;
		/// <summary>
		/// Returns the emojis in this menu.
		/// </summary>
		public Emoji[] Values => _Emojis.ToArray();

		private readonly List<Emoji> _Emojis = new List<Emoji>();

		/// <summary>
		/// Attempts to find the first menu emoji that has the same name as <paramref name="emote"/>.
		/// </summary>
		/// <param name="emote"></param>
		/// <param name="menuItem"></param>
		/// <returns></returns>
		public bool TryGet(IEmote emote, out Emoji? menuItem)
		{
			foreach (var emoji in _Emojis)
			{
				if (emote.Name == emoji.Name)
				{
					menuItem = emoji;
					return true;
				}
			}
			menuItem = null;
			return false;
		}
		/// <inheritdoc />
		public void Add(Emoji item)
		{
			if (_Emojis.Count == EMOJI_MENU_LIMIT)
			{
				throw new InvalidOperationException($"Cannot have more than {EMOJI_MENU_LIMIT} emojis in one menu.");
			}
			_Emojis.Add(item);
		}
		/// <inheritdoc />
		public void Clear()
			=> _Emojis.Clear();
		/// <inheritdoc />
		public bool Contains(Emoji item)
			=> _Emojis.Contains(item);
		/// <inheritdoc />
		public void CopyTo(Emoji[] array, int arrayIndex)
			=> _Emojis.CopyTo(array, arrayIndex);
		/// <inheritdoc />
		public IEnumerator<Emoji> GetEnumerator()
			=> ((ICollection<Emoji>)_Emojis).GetEnumerator();
		/// <inheritdoc />
		public bool Remove(Emoji item)
			=> _Emojis.Remove(item);

		//IEnumerator
		IEnumerator IEnumerable.GetEnumerator() => ((ICollection<Emoji>)_Emojis).GetEnumerator();
	}
}
