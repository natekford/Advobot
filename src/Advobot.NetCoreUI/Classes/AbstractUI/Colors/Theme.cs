using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;

namespace Advobot.NetCoreUI.Classes.AbstractUI.Colors
{
	/// <summary>
	/// Holds a collection of colors for usage in UI.
	/// </summary>
	/// <typeparam name="TBrush"></typeparam>
	/// <typeparam name="TBrushFactory"></typeparam>
	public class Theme<TBrush, TBrushFactory> : ITheme<TBrush>
		where TBrushFactory : BrushFactory<TBrush>, new()
	{
		[JsonProperty("Brushes")]
		private Dictionary<string, string> _Brushes { get; } = new Dictionary<string, string>();
		[JsonIgnore]
		private Dictionary<string, TBrush> _RuntimeBrushes { get; } = new Dictionary<string, TBrush>();
		[JsonIgnore]
		private TBrushFactory _BrushFactory { get; } = new TBrushFactory();
		[JsonIgnore]
		private bool _Frozen;

		/// <inheritdoc />
		[JsonIgnore]
		public ICollection<string> Keys => _RuntimeBrushes.Keys;
		/// <inheritdoc />
		[JsonIgnore]
		public ICollection<TBrush> Values => _RuntimeBrushes.Values;
		/// <inheritdoc />
		[JsonIgnore]
		public int Count => _RuntimeBrushes.Count;
		/// <inheritdoc />
		[JsonIgnore]
		public bool IsReadOnly => _Frozen;

		/// <inheritdoc />
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Gets or sets the color for the specified color target which can be used when the custom theme is enabled.
		/// </summary>
		/// <param name="target"></param>
		/// <returns></returns>
		public TBrush this[string target]
		{
			get => _RuntimeBrushes.TryGetValue(target, out var brush) ? brush : _BrushFactory.Default;
			set => SetBrush(target, value);
		}

		private void SetBrush(string target, TBrush value)
		{
			if (_Frozen)
			{
				throw new InvalidOperationException("Cannot modify or set a brush after the theme is frozen.");
			}
			if (!_Brushes.ContainsKey(target))
			{
				_Brushes.Add(target, _BrushFactory.FormatBrush(_BrushFactory.Default));
				_RuntimeBrushes.Add(target, _BrushFactory.Default);
			}
			_Brushes[target] = _BrushFactory.FormatBrush(value);
			_RuntimeBrushes[target] = value;
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(target));
		}
		/// <summary>
		/// Adds the brush to the theme.
		/// </summary>
		/// <param name="target"></param>
		/// <param name="value"></param>
		public void Add(string target, string value)
			=> SetBrush(target, _BrushFactory.CreateBrush(value));
		/// <summary>
		/// Adds the brush to the theme.
		/// </summary>
		/// <param name="target"></param>
		/// <param name="value"></param>
		public void Add(string target, TBrush value)
			=> SetBrush(target, value);
		/// <summary>
		/// Prevents the theme from being modified.
		/// </summary>
		public void Freeze()
			=> _Frozen = true;
		/// <inheritdoc />
		public IEnumerator<TBrush> GetEnumerator()
			=> _RuntimeBrushes.Values.GetEnumerator();
		/// <inheritdoc />
		IEnumerator IEnumerable.GetEnumerator()
			=> _RuntimeBrushes.Values.GetEnumerator();
		/// <inheritdoc />
		public bool ContainsKey(string key)
			=> _RuntimeBrushes.ContainsKey(key);
		/// <inheritdoc />
		public bool Remove(string key)
			=> _RuntimeBrushes.Remove(key);
		/// <inheritdoc />
		public bool TryGetValue(string key, out TBrush value)
			=> _RuntimeBrushes.TryGetValue(key, out value);
		/// <inheritdoc />
		public void Add(KeyValuePair<string, TBrush> item)
			=> ((IDictionary<string, TBrush>)_RuntimeBrushes).Add(item);
		/// <inheritdoc />
		public void Clear()
			=> _RuntimeBrushes.Clear();
		/// <inheritdoc />
		public bool Contains(KeyValuePair<string, TBrush> item)
			=> ((IDictionary<string, TBrush>)_RuntimeBrushes).Contains(item);
		/// <inheritdoc />
		public void CopyTo(KeyValuePair<string, TBrush>[] array, int arrayIndex)
			=> ((IDictionary<string, TBrush>)_RuntimeBrushes).CopyTo(array, arrayIndex);
		/// <inheritdoc />
		public bool Remove(KeyValuePair<string, TBrush> item)
			=> ((IDictionary<string, TBrush>)_RuntimeBrushes).Remove(item);
		/// <inheritdoc />
		IEnumerator<KeyValuePair<string, TBrush>> IEnumerable<KeyValuePair<string, TBrush>>.GetEnumerator()
			=> ((IDictionary<string, TBrush>)_RuntimeBrushes).GetEnumerator();
	}
}