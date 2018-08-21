using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Newtonsoft.Json;

namespace Advobot.SharedUI
{
	/// <summary>
	/// Holds a collection of colors for usage in UI.
	/// </summary>
	/// <typeparam name="TBrush"></typeparam>
	/// <typeparam name="TBrushFactory"></typeparam>
	public abstract class Theme<TBrush, TBrushFactory> : INotifyCollectionChanged
		where TBrushFactory : BrushFactory<TBrush>, new()
	{
		/// <summary>
		/// This theme's name.
		/// </summary>
		[JsonIgnore]
		public string Name { get; }
		/// <summary>
		/// The factory for creating brushes in this theme.
		/// </summary>
		[JsonIgnore]
		public TBrushFactory BrushFactory { get; } = new TBrushFactory();
		/// <summary>
		/// All the brushes in this theme.
		/// </summary>
		[JsonIgnore]
		public ImmutableDictionary<string, TBrush> Brushes => _Brushes.ToDictionary(x => x.Key, x => BrushFactory.CreateBrush(x.Value)).ToImmutableDictionary();
		[JsonProperty("Brushes")]
		private Dictionary<string, string> _Brushes = new Dictionary<string, string>();

		/// <inheritdoc />
		public event NotifyCollectionChangedEventHandler CollectionChanged;

		/// <summary>
		/// Creates an instance of <see cref="Theme{TBrush, TBrushFactory}"/>.
		/// </summary>
		/// <param name="name"></param>
		public Theme(string name)
		{
			Name = name;
		}

		/// <summary>
		/// Gets or sets the color for the specified color target which can be used when the custom theme is enabled.
		/// </summary>
		/// <param name="target"></param>
		/// <returns></returns>
		public TBrush this[string target]
		{
			get => _Brushes[target] is string val ? BrushFactory.CreateBrush(val) : default;
			set
			{
				_Brushes[target] = value == null ? null : BrushFactory.FormatBrush(value);
				CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, value));
				/*
				if (_Theme == ColorTheme.UserMade)
				{
					UpdateResources(target, value);
				}*/
			}
		}

		private void RegisterBrush(string value, [CallerMemberName] string name = null)
		{

		}
	}
}