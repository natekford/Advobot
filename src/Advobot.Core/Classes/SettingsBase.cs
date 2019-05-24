using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Advobot.Classes.Formatting;
using Advobot.Interfaces;
using AdvorangesUtils;
using Newtonsoft.Json;

namespace Advobot.Classes
{
	/// <summary>
	/// Abstract class for settings.
	/// </summary>
	internal abstract class SettingsBase : ISettingsBase
	{
		private readonly Dictionary<string, PropertyInfo> _Settings;

		public IReadOnlyCollection<string> SettingNames => _Settings.Keys.ToArray();

		/// <inheritdoc />
		public event PropertyChangedEventHandler PropertyChanged;

		public SettingsBase()
		{
#warning make setting attribute?
			_Settings = GetType().GetProperties()
				.Where(x => x.GetCustomAttribute<JsonPropertyAttribute>() != null)
				.ToDictionary(x => x.Name, x => x, StringComparer.OrdinalIgnoreCase);
		}

		/// <inheritdoc />
		public IDiscordFormattableString Format()
		{
			var settings = _Settings.Select(x => (x.Key, FormatValue(x.Value.GetValue(this))));
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
			=> FormatValue(_Settings[name].GetValue(this));
		/// <inheritdoc />
		public IDiscordFormattableString FormatValue(object? value)
			=> new DiscordFormattableString($"{value}");
		/// <inheritdoc />
		public void Save(IBotDirectoryAccessor accessor)
			=> IOUtils.SafeWriteAllText(GetFile(accessor), IOUtils.Serialize(this));
		/// <inheritdoc />
		public abstract FileInfo GetFile(IBotDirectoryAccessor accessor);
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