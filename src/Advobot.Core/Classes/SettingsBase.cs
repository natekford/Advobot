using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Advobot.Classes.Attributes;
using Advobot.Classes.Formatting;
using Advobot.Interfaces;
using AdvorangesUtils;

namespace Advobot.Classes
{
	/// <summary>
	/// Abstract class for settings.
	/// </summary>
	internal abstract class SettingsBase : ISettingsBase
	{
		private readonly IEnumerable<(SettingAttribute Attribute, PropertyInfo Property)> _Properties;
		private IDictionary<string, PropertyInfo> Settings => _Properties.ToDictionary(
			x => GetLocalizedName(x.Attribute),
			x => x.Property,
			StringComparer.OrdinalIgnoreCase);

		public IReadOnlyCollection<string> SettingNames => Settings.Keys.ToArray();

		/// <inheritdoc />
		public event PropertyChangedEventHandler PropertyChanged;

		public SettingsBase()
		{
			_Properties = GetType().GetProperties()
				.Select(x => (Attribute: x.GetCustomAttribute<SettingAttribute>(), Property: x))
				.Where(x => x.Attribute != null);
		}

		/// <inheritdoc />
		public IDiscordFormattableString Format()
		{
			var settings = Settings.Select(x => (x.Key, FormatValue(x.Value.GetValue(this))));
			var formattable = new DiscordFormattableStringCollection();
#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
			foreach (var (Name, FormattedValue) in settings)
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.
			{
				if (FormattedValue != null)
				{
					formattable.Add($"{Name.AsTitle()}\n{FormattedValue.NoFormatting()}\n\n");
				}
			}
			return formattable;
		}
		/// <inheritdoc />
		public IDiscordFormattableString FormatSetting(string name)
			=> FormatValue(Settings[name].GetValue(this));
		/// <inheritdoc />
		public IDiscordFormattableString FormatValue(object? value)
			=> new DiscordFormattableString($"{value}");
		/// <inheritdoc />
		public abstract void Save();
		/// <summary>
		/// Gets the localized setting name.
		/// </summary>
		/// <param name="attr"></param>
		/// <returns></returns>
		protected abstract string GetLocalizedName(SettingAttribute attr);
		/// <inheritdoc />
		protected void RaisePropertyChanged([CallerMemberName] string caller = "")
			=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(caller));
		/// <summary>
		/// Sets the field and raises property changed.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="field"></param>
		/// <param name="value"></param>
		/// <param name="caller"></param>
		protected void SetValue<T>(ref T field, T value, [CallerMemberName] string caller = "")
		{
			field = value;
			RaisePropertyChanged(caller);
		}
		/// <summary>
		/// Throws an argument exception if the condition is true.
		/// </summary>
		/// <param name="field"></param>
		/// <param name="value"></param>
		/// <param name="condition"></param>
		/// <param name="msg"></param>
		/// <param name="caller"></param>
		protected void ThrowIfElseSet<T>(ref T field, T value, Func<T, bool> condition, string msg, [CallerMemberName] string caller = "")
		{
			if (condition(value))
			{
				throw new ArgumentException(msg, caller);
			}
			field = value;
			RaisePropertyChanged(caller);
		}
	}
}