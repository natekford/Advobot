using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Advobot.Interfaces;
using AdvorangesUtils;

namespace Advobot.Services.HelpEntries
{
	/// <summary>
	/// Creates a help entry for every command and then allows those to be accessed.
	/// </summary>
	internal sealed class HelpEntryService : IHelpEntryService
	{
		/// <inheritdoc />
		public int Count => _Source.Count;
		/// <inheritdoc />
		public bool IsReadOnly => false;
		/// <inheritdoc />
		public IEnumerable<string> Keys => _NameMap.Keys;
		/// <inheritdoc />
		public IEnumerable<IHelpEntry> Values => _Source.Values;

		/// <inheritdoc />
		public IHelpEntry this[string key] => _Source[_NameMap[key]];

		private readonly Dictionary<string, Guid> _NameMap = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
		private readonly Dictionary<Guid, IHelpEntry> _Source = new Dictionary<Guid, IHelpEntry>();

		/// <inheritdoc />
		public void Add(IHelpEntry item)
		{
			var guid = Guid.NewGuid();
			foreach (var alias in item.Aliases)
			{
				_NameMap.Add(alias, guid);
			}
			_Source.Add(guid, item);
		}
		/// <inheritdoc />
		public bool Remove(IHelpEntry helpEntry)
		{
			if (!_NameMap.TryGetValue(helpEntry.Name, out var guid))
			{
				return false;
			}

			foreach (var kvp in _NameMap.Where(x => x.Value == guid).ToArray())
			{
				_NameMap.Remove(kvp.Key);
			}
			return _Source.Remove(guid);
		}
		/// <inheritdoc />
		public void Clear()
		{
			_NameMap.Clear();
			_Source.Clear();
		}
		/// <inheritdoc />
		public bool Contains(IHelpEntry item)
			=> _Source.Values.Contains(item);
		/// <inheritdoc />
		public void CopyTo(IHelpEntry[] array, int arrayIndex)
			=> _Source.Values.CopyTo(array, arrayIndex);
		/// <inheritdoc />
		public IEnumerator<IHelpEntry> GetEnumerator()
			=> _Source.Values.GetEnumerator();
		/// <inheritdoc />
		public bool ContainsKey(string key)
			=> _NameMap.ContainsKey(key);
		/// <inheritdoc />
		public bool TryGetValue(string key, out IHelpEntry value)
		{
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
			value = default;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
			return _NameMap.TryGetValue(key, out var guid) && _Source.TryGetValue(guid, out value);
		}
		/// <inheritdoc />
		public IReadOnlyCollection<string> GetCategories()
			=> _Source.Values.Select(x => x.Category).Distinct().ToArray();
		/// <inheritdoc />
		public IReadOnlyCollection<IHelpEntry> GetHelpEntries(string? category = null)
		{
			return category == null
				? _Source.Values.ToArray()
				: _Source.Values.Where(x => x.Category.CaseInsEquals(category)).ToArray();
		}
		/// <inheritdoc />
		public IReadOnlyCollection<IHelpEntry> GetUnsetCommands(IEnumerable<string> setCommands)
			=> _Source.Values.Where(x => !setCommands.CaseInsContains(x.Name)).ToArray();
		/// <inheritdoc />
		IEnumerator IEnumerable.GetEnumerator()
			=> GetEnumerator();
		/// <inheritdoc />
		IEnumerator<KeyValuePair<string, IHelpEntry>> IEnumerable<KeyValuePair<string, IHelpEntry>>.GetEnumerator()
		{
			foreach (var kvp in _NameMap)
			{
				yield return new KeyValuePair<string, IHelpEntry>(kvp.Key, _Source[kvp.Value]);
			}
		}
	}
}