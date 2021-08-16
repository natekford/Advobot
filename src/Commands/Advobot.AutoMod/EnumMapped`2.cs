using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Advobot.AutoMod
{
	public sealed class EnumMapped<TEnum, TValue>
		: IReadOnlyDictionary<TEnum, TValue>
		where TEnum : Enum
		where TValue : new()
	{
		private static readonly TEnum[] _Values
			= Enum.GetValues(typeof(TEnum)).Cast<TEnum>().ToArray();

		private readonly Dictionary<TEnum, TValue> _Dict = new();

		public int Count => _Dict.Count;
		public IEnumerable<TEnum> Keys
			=> ((IReadOnlyDictionary<TEnum, TValue>)_Dict).Keys;
		public IEnumerable<TValue> Values
			=> ((IReadOnlyDictionary<TEnum, TValue>)_Dict).Values;

		public TValue this[TEnum key]
		{
			get => _Dict[key];
			set => _Dict[key] = value;
		}

		public EnumMapped()
		{
			foreach (var value in _Values)
			{
				_Dict[value] = new();
			}
		}

		public bool ContainsKey(TEnum key) => true;

		public IEnumerator<KeyValuePair<TEnum, TValue>> GetEnumerator()
			=> ((IReadOnlyDictionary<TEnum, TValue>)_Dict).GetEnumerator();

		public void Reset(TEnum key)
			=> _Dict[key] = new();

		public void ResetAll()
		{
			foreach (var value in _Values)
			{
				_Dict[value] = new();
			}
		}

		public bool TryGetValue(TEnum key, [NotNullWhen(true)] out TValue value)
			=> _Dict.TryGetValue(key, out value!);

		public void Update(TEnum key, Func<TValue, TValue> updater)
			=> _Dict[key] = updater(_Dict[key]);

		IEnumerator IEnumerable.GetEnumerator()
			=> ((IReadOnlyDictionary<TEnum, TValue>)_Dict).GetEnumerator();
	}
}