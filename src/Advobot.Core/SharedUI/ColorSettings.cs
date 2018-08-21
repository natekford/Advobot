using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Advobot.Classes;
using Advobot.Classes.Attributes;
using Advobot.Enums;
using Advobot.Interfaces;
using Advobot.Utilities;
using AdvorangesUtils;
using Newtonsoft.Json;

namespace Advobot.SharedUI
{
	/// <summary>
	/// Indicates what colors to use in the UI.
	/// </summary>
	public abstract class ColorSettings<TBrush, TBrushFactory> : SettingsBase where TBrushFactory : BrushFactory<TBrush>, new()
	{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
		public static TBrushFactory BrushFactory { get; } = new TBrushFactory();
		public static ImmutableDictionary<string, TBrush> LightModeProperties { get; } = GetColorProperties("LightMode");
		public static ImmutableDictionary<string, TBrush> DarkModeProperties { get; } = GetColorProperties("DarkMode");
		public static ImmutableArray<string> Targets { get; } = GetColorTargets();

		public static TBrush LightModeBaseBackground => BrushFactory.CreateBrush("#FFFFFF");
		public static TBrush LightModeBaseForeground => BrushFactory.CreateBrush("#000000");
		public static TBrush LightModeBaseBorder => BrushFactory.CreateBrush("#ABADB3");
		public static TBrush LightModeButtonBackground => BrushFactory.CreateBrush("#DDDDDD");
		public static TBrush LightModeButtonForeground => BrushFactory.CreateBrush("#0E0E0E");
		public static TBrush LightModeButtonBorder => BrushFactory.CreateBrush("#707070");
		public static TBrush LightModeButtonDisabledBackground => BrushFactory.CreateBrush("#F4F4F4");
		public static TBrush LightModeButtonDisabledForeground => BrushFactory.CreateBrush("#888888");
		public static TBrush LightModeButtonDisabledBorder => BrushFactory.CreateBrush("#ADB2B5");
		public static TBrush LightModeButtonMouseOverBackground => BrushFactory.CreateBrush("#BEE6FD");
		public static TBrush LightModeJsonDigits => BrushFactory.CreateBrush("#8700FF");
		public static TBrush LightModeJsonValue => BrushFactory.CreateBrush("#000CFF");
		public static TBrush LightModeJsonParamName => BrushFactory.CreateBrush("#057500");

		public static TBrush DarkModeBaseBackground => BrushFactory.CreateBrush("#1C1C1C");
		public static TBrush DarkModeBaseForeground => BrushFactory.CreateBrush("#E1E1E1");
		public static TBrush DarkModeBaseBorder => BrushFactory.CreateBrush("#ABADB3");
		public static TBrush DarkModeButtonBackground => BrushFactory.CreateBrush("#151515");
		public static TBrush DarkModeButtonForeground => BrushFactory.CreateBrush("#E1E1E1");
		public static TBrush DarkModeButtonBorder => BrushFactory.CreateBrush("#ABADB3");
		public static TBrush DarkModeButtonDisabledBackground => BrushFactory.CreateBrush("#343434");
		public static TBrush DarkModeButtonDisabledForeground => BrushFactory.CreateBrush("#A0A0A0");
		public static TBrush DarkModeButtonDisabledBorder => BrushFactory.CreateBrush("#ADB2B5");
		public static TBrush DarkModeButtonMouseOverBackground => BrushFactory.CreateBrush("#303333");
		public static TBrush DarkModeJsonDigits => BrushFactory.CreateBrush("#8700FF");
		public static TBrush DarkModeJsonValue => BrushFactory.CreateBrush("#0051FF");
		public static TBrush DarkModeJsonParamName => BrushFactory.CreateBrush("#057500");
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

		/// <summary>
		/// Colors defined by the user.
		/// </summary>
		[JsonProperty("ColorTargets", Order = 1), Setting(NonCompileTimeDefaultValue.ResetDictionaryValues)]
		protected Dictionary<string, string> UserDefinedColors { get; set; } = new Dictionary<string, string>();
		/// <inheritdoc />
		[JsonProperty("Theme", Order = 2), Setting(ColorTheme.LightMode)]
		public ColorTheme Theme
		{
			get => _Theme;
			set
			{
				_Theme = value;
				switch (Theme)
				{
					case ColorTheme.LightMode:
						foreach (var ct in Targets)
						{
							UpdateResources(ct, LightModeProperties[ct]);
						}
						break;
					case ColorTheme.DarkMode:
						foreach (var ct in Targets)
						{
							UpdateResources(ct, DarkModeProperties[ct]);
						}
						break;
					case ColorTheme.UserMade:
						foreach (var kvp in UserDefinedColors)
						{
							UpdateResources(kvp.Key, kvp.Value is string val ? BrushFactory.CreateBrush(val) : default);
						}
						break;
				}
				AfterThemeUpdated();
				NotifyPropertyChanged();
			}
		}
		[JsonIgnore]
		private ColorTheme _Theme = ColorTheme.LightMode;

		/// <summary>
		/// Creates an instance of <see cref="ColorSettings{TBrush, TBrushFactory}"/> and sets the default theme and colors to light.
		/// </summary>
		public ColorSettings()
		{
			Theme = ColorTheme.LightMode;
			foreach (var target in Targets)
			{
				if (!UserDefinedColors.TryGetValue(target, out var val))
				{
					UserDefinedColors.Add(target, BrushFactory.FormatBrush(LightModeProperties[target]));
				}
			}
		}

		/// <summary>
		/// Gets or sets the color for the specified color target which can be used when the custom theme is enabled.
		/// </summary>
		/// <param name="target"></param>
		/// <returns></returns>
		public TBrush this[string target]
		{
			get => UserDefinedColors[target] is string val ? BrushFactory.CreateBrush(val) : default;
			set
			{
				UserDefinedColors[target] = value == null ? null : BrushFactory.FormatBrush(value);
				if (_Theme == ColorTheme.UserMade)
				{
					UpdateResources(target, value);
				}
			}
		}

		/// <summary>
		/// Updates a resource dictionary with the specified value.
		/// </summary>
		/// <param name="target"></param>
		/// <param name="value"></param>
		protected abstract void UpdateResources(string target, TBrush value);
		/// <summary>
		/// Does an action after the theme has been updated.
		/// </summary>
		protected virtual void AfterThemeUpdated() { }
		/// <inheritdoc />
		public override FileInfo GetFile(IBotDirectoryAccessor accessor)
		{
			return StaticGetPath(accessor);
		}
		/// <summary>
		/// Loads the UI settings from file.
		/// </summary>
		/// <param name="accessor"></param>
		/// <returns></returns>
		public static T Load<T>(IBotDirectoryAccessor accessor) where T : ColorSettings<TBrush, TBrushFactory>, new()
		{
			return IOUtils.DeserializeFromFile<T>(StaticGetPath(accessor)) ?? new T();
		}
		private static FileInfo StaticGetPath(IBotDirectoryAccessor accessor)
		{
			return accessor.GetBaseBotDirectoryFile("UISettings.json");
		}
		private static ImmutableDictionary<string, TBrush> GetColorProperties(string prefix)
		{
			return typeof(ColorSettings<TBrush, TBrushFactory>).GetProperties(BindingFlags.Public | BindingFlags.Static)
				.Where(x => x.PropertyType == typeof(TBrush) && x.Name.Contains(prefix))
				.ToDictionary(x => x.Name.Replace(prefix, ""), x => (TBrush)x.GetValue(null)).ToImmutableDictionary();
		}
		private static ImmutableArray<string> GetColorTargets()
		{
			var light = LightModeProperties.Keys.ToList();
			var dark = DarkModeProperties.Keys.ToList();
			var keys = light.Concat(dark).Distinct().ToImmutableArray();
			if (keys.Length != (light.Count + dark.Count) / 2)
			{
				throw new InvalidOperationException("Light and dark are not balanced.");
			}
			return keys;
		}
	}
}