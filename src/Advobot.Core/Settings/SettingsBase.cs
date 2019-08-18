using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using Advobot.Formatting;
using Advobot.Localization;
using Advobot.Settings.GenerateResetValues;

namespace Advobot.Settings
{
	/// <summary>
	/// Abstract class for settings.
	/// </summary>
	internal abstract class SettingsBase : ISettingsBase
	{
		private readonly IReadOnlyList<Setting> _Settings;
		private readonly Localized<IReadOnlyDictionary<string, Setting>> _Localized;

		/// <inheritdoc />
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Creates an instance of <see cref="SettingsBase"/>.
		/// </summary>
		public SettingsBase()
		{
			IEnumerable<Setting> GetSettings()
			{
				foreach (var property in GetType().GetProperties())
				{
					var attr = property.GetCustomAttribute<SettingAttribute>();
					if (attr != null)
					{
						yield return new Setting(attr, property);
					}
				}
			}

			_Settings = GetSettings().ToArray();
			_Localized = new Localized<IReadOnlyDictionary<string, Setting>>(x =>
			{
				var dict = new Dictionary<string, Setting>();
				foreach (var setting in _Settings)
				{
					var name = GetLocalizedName(setting.SettingAttribute);
					dict.Add(name, setting);

					if (name.Contains(" "))
					{
						dict.Add(name.Replace(" ", ""), setting);
					}
				}
				return dict.ToImmutableDictionary(StringComparer.OrdinalIgnoreCase);
			});
		}

		/// <inheritdoc />
		public IReadOnlyCollection<string> GetSettingNames()
			=> _Localized.Get().Keys.ToArray();
		/// <inheritdoc />
		public void ResetSetting(string name)
			=> _Localized.Get()[name].Reset(this);
		/// <inheritdoc />
		public IDiscordFormattableString Format()
		{
			var formattable = new DiscordFormattableStringCollection();
			foreach (var kvp in _Localized.Get())
			{
				var name = kvp.Key;
				var formatted = FormatValue(kvp.Value.GetCurrentValue(this));
				if (formatted != null)
				{
					formattable.Add($"{name.AsTitle()}\n{formatted.NoFormatting()}\n\n");
				}
			}
			return formattable;
		}
		/// <inheritdoc />
		public IDiscordFormattableString FormatSetting(string name)
			=> FormatValue(_Localized.Get()[name].GetCurrentValue(this));
		/// <inheritdoc />
		public IDiscordFormattableString FormatValue(object? value)
			=> new DiscordFormattableString(value);
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
			try
			{
				if (condition(value))
				{
					throw new ArgumentException(msg, caller);
				}
			}
			catch (Exception e)
			{
				throw new ArgumentException(msg, caller, e);
			}
			field = value;
			RaisePropertyChanged(caller);
		}

		private sealed class Setting
		{
			public bool CanReset => SettingAttribute.ResetValueClass != null || SettingAttribute.DefaultValue != null;

			public SettingAttribute SettingAttribute { get; }

			private readonly Func<SettingsBase, object?> _Getter;
			private readonly Action<SettingsBase, object?> _Setter;
			private readonly bool _IsValueType;

			private IGenerateResetValue? _GenerateResetValue;
			private Type? _GenerateResetValueType;

			public Setting(SettingAttribute settingAttribute, PropertyInfo property)
			{
				SettingAttribute = settingAttribute;

				_Getter = BuildUntypedGetter(property);
				_Setter = BuildUntypedSetter(property);
				_IsValueType = property.PropertyType.IsValueType;
			}

			public object? GetCurrentValue(SettingsBase parent)
				=> _Getter(parent);
			public void Reset(SettingsBase parent)
			{
				if (SettingAttribute.DefaultValue != null)
				{
					if (!AreEqual(GetCurrentValue(parent), SettingAttribute.DefaultValue))
					{
						_Setter(parent, SettingAttribute.DefaultValue);
					}
					return;
				}

				if (SettingAttribute.ResetValueClass == null)
				{
					throw new InvalidOperationException($"No {nameof(SettingAttribute.ResetValueClass)} is defined.");
				}

				if (_GenerateResetValueType != SettingAttribute.ResetValueClass)
				{
					_GenerateResetValueType = SettingAttribute.ResetValueClass;
					_GenerateResetValue = (IGenerateResetValue)Activator.CreateInstance(_GenerateResetValueType);
				}

				var currentValue = GetCurrentValue(parent);
				var resetValue = _GenerateResetValue?.GenerateResetValue(currentValue);
				if (!AreEqual(currentValue, resetValue))
				{
					_Setter(parent, resetValue);
				}
			}
			private Func<SettingsBase, object?> BuildUntypedGetter(PropertyInfo propertyInfo)
			{
				var settings = Expression.Parameter(typeof(SettingsBase), "t");
				var castSettings = Expression.Convert(settings, propertyInfo.DeclaringType);

				var getter = Expression.Call(castSettings, propertyInfo.GetGetMethod());
				var convertedValue = Expression.Convert(getter, typeof(object));
				var lambda = Expression.Lambda<Func<SettingsBase, object?>>(convertedValue, settings);
				return lambda.Compile();
			}
			private Action<SettingsBase, object?> BuildUntypedSetter(PropertyInfo propertyInfo)
			{
				var settings = Expression.Parameter(typeof(SettingsBase), "t");
				var castSettings = Expression.Convert(settings, propertyInfo.DeclaringType);

				var value = Expression.Parameter(typeof(object), "p");
				var convertedValue = Expression.Convert(value, propertyInfo.PropertyType);
				var setter = Expression.Call(castSettings, propertyInfo.GetSetMethod(), convertedValue);
				var lambda = Expression.Lambda<Action<SettingsBase, object?>>(setter, settings, value);
				return lambda.Compile();
			}
			private bool AreEqual(object? a, object? b)
			{
				//This is needed because everything will be boxed as object
				//Meaning [object]false != [object]false unless we use .Equals
				if (a is null)
				{
					return b is null;
				}
				else if (b is null)
				{
					return false;
				}
				return _IsValueType ? a.Equals(b) : ReferenceEquals(a, b);
			}
		}
	}
}