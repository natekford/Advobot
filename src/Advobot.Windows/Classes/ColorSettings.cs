using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using Advobot.Classes;
using Advobot.Classes.Attributes;
using Advobot.Enums;
using Advobot.Interfaces;
using Advobot.Utilities;
using Advobot.Windows.Enums;
using Advobot.Windows.Utilities;
using AdvorangesUtils;
using ICSharpCode.AvalonEdit.Highlighting;
using Newtonsoft.Json;

namespace Advobot.Windows.Classes
{
	/// <summary>
	/// Indicates what colors to use in the UI.
	/// </summary>
	public sealed class ColorSettings : SettingsBase
	{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
		public static ImmutableDictionary<ColorTarget, SolidColorBrush> LightModeProperties { get; } = GetColorProperties("LightMode");
		public static ImmutableDictionary<ColorTarget, SolidColorBrush> DarkModeProperties { get; } = GetColorProperties("DarkMode");

		public static SolidColorBrush LightModeBaseBackground => BrushUtils.CreateBrush("#FFFFFF");
		public static SolidColorBrush LightModeBaseForeground => BrushUtils.CreateBrush("#000000");
		public static SolidColorBrush LightModeBaseBorder => BrushUtils.CreateBrush("#ABADB3");
		public static SolidColorBrush LightModeButtonBackground => BrushUtils.CreateBrush("#DDDDDD");
		public static SolidColorBrush LightModeButtonForeground => BrushUtils.CreateBrush("#0E0E0E");
		public static SolidColorBrush LightModeButtonBorder => BrushUtils.CreateBrush("#707070");
		public static SolidColorBrush LightModeButtonDisabledBackground => BrushUtils.CreateBrush("#F4F4F4");
		public static SolidColorBrush LightModeButtonDisabledForeground => BrushUtils.CreateBrush("#888888");
		public static SolidColorBrush LightModeButtonDisabledBorder => BrushUtils.CreateBrush("#ADB2B5");
		public static SolidColorBrush LightModeButtonMouseOverBackground => BrushUtils.CreateBrush("#BEE6FD");
		public static SolidColorBrush LightModeJsonDigits => BrushUtils.CreateBrush("#8700FF");
		public static SolidColorBrush LightModeJsonValue => BrushUtils.CreateBrush("#000CFF");
		public static SolidColorBrush LightModeJsonParamName => BrushUtils.CreateBrush("#057500");

		public static SolidColorBrush DarkModeBaseBackground => BrushUtils.CreateBrush("#1C1C1C");
		public static SolidColorBrush DarkModeBaseForeground => BrushUtils.CreateBrush("#E1E1E1");
		public static SolidColorBrush DarkModeBaseBorder => BrushUtils.CreateBrush("#ABADB3");
		public static SolidColorBrush DarkModeButtonBackground => BrushUtils.CreateBrush("#151515");
		public static SolidColorBrush DarkModeButtonForeground => BrushUtils.CreateBrush("#E1E1E1");
		public static SolidColorBrush DarkModeButtonBorder => BrushUtils.CreateBrush("#ABADB3");
		public static SolidColorBrush DarkModeButtonDisabledBackground => BrushUtils.CreateBrush("#343434");
		public static SolidColorBrush DarkModeButtonDisabledForeground => BrushUtils.CreateBrush("#A0A0A0");
		public static SolidColorBrush DarkModeButtonDisabledBorder => BrushUtils.CreateBrush("#ADB2B5");
		public static SolidColorBrush DarkModeButtonMouseOverBackground => BrushUtils.CreateBrush("#303333");
		public static SolidColorBrush DarkModeJsonDigits => BrushUtils.CreateBrush("#8700FF");
		public static SolidColorBrush DarkModeJsonValue => BrushUtils.CreateBrush("#0051FF");
		public static SolidColorBrush DarkModeJsonParamName => BrushUtils.CreateBrush("#057500");
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

		[JsonProperty("ColorTargets"), Setting(NonCompileTimeDefaultValue.ResetDictionaryValues)]
		private Dictionary<ColorTarget, SolidColorBrush> _ColorTargets { get; set; } = new Dictionary<ColorTarget, SolidColorBrush>();
		/// <summary>
		/// The current theme to use.
		/// </summary>
		/// <remarks>Has to be under <see cref="_ColorTargets"/> to set property on startup if the theme is <see cref="ColorTheme.UserMade"/>.</remarks>
		[JsonProperty("Theme"), Setting(ColorTheme.Classic)]
		public ColorTheme Theme
		{
			get => _Theme;
			set
			{
				_Theme = value;
				switch (Theme)
				{
					case ColorTheme.Classic:
						foreach (ColorTarget ct in Enum.GetValues(typeof(ColorTarget)))
						{
							Application.Current.Resources[ct] = LightModeProperties[ct];
						}
						break;
					case ColorTheme.DarkMode:
						foreach (ColorTarget ct in Enum.GetValues(typeof(ColorTarget)))
						{
							Application.Current.Resources[ct] = DarkModeProperties[ct];
						}
						break;
					case ColorTheme.UserMade:
						foreach (var kvp in _ColorTargets)
						{
							Application.Current.Resources[kvp.Key] = kvp.Value;
						}
						break;
				}
				SetSyntaxHighlightingColors("Json");
				NotifyPropertyChanged();
			}
		}
		[JsonIgnore]
		private ColorTheme _Theme = ColorTheme.Classic;

		/// <summary>
		/// Gets or sets the color for the specified color target which can be used when the custom theme is enabled.
		/// </summary>
		/// <param name="target"></param>
		/// <returns></returns>
		public SolidColorBrush this[ColorTarget target]
		{
			get => _ColorTargets[target];
			set
			{
				_ColorTargets[target] = value;
				if (_Theme == ColorTheme.UserMade)
				{
					Application.Current.Resources[target] = value;
				}
			}
		}

		/// <summary>
		/// Creates an instance of <see cref="ColorSettings"/>.
		/// </summary>
		public ColorSettings()
		{
			foreach (ColorTarget target in Enum.GetValues(typeof(ColorTarget)))
			{
				_ColorTargets.Add(target, null);
			}
		}

		/// <summary>
		/// Attempts to get the specified color target's value. Returns false if unable to.
		/// </summary>
		/// <param name="target"></param>
		/// <param name="brush"></param>
		/// <returns></returns>
		public bool TryGetValue(ColorTarget target, out SolidColorBrush brush)
		{
			return _ColorTargets.TryGetValue(target, out brush);
		}
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
		public static ColorSettings Load(IBotDirectoryAccessor accessor)
		{
			return IOUtils.DeserializeFromFile<ColorSettings>(StaticGetPath(accessor)) ?? new ColorSettings();
		}
		private void SetSyntaxHighlightingColors(params string[] names)
		{
			foreach (var name in names)
			{
				var highlighting = HighlightingManager.Instance.GetDefinition(name) ?? throw new ArgumentException("not a valid highlighting.", name);

				foreach (var namedColor in highlighting.NamedHighlightingColors)
				{
					//E.G.: Highlighting name is json, color name is Param, searches for jsonParam
					var colorName = highlighting.Name + namedColor.Name;
					if (!Enum.TryParse(colorName, true, out ColorTarget target))
					{
						continue;
					}

					//Get the set color, if one doesn't exist, use the default light mode
					var color = ((SolidColorBrush)Application.Current.Resources[target])?.Color ?? LightModeProperties[target].Color;
					namedColor.Foreground = new SimpleHighlightingBrush(color);
				}
			}
		}
		private static FileInfo StaticGetPath(IBotDirectoryAccessor accessor)
		{
			return accessor.GetBaseBotDirectoryFile("UISettings.json");
		}
		private static ImmutableDictionary<ColorTarget, SolidColorBrush> GetColorProperties(string prefix)
		{
			return typeof(ColorSettings).GetProperties(BindingFlags.Public | BindingFlags.Static)
				.Where(x => x.PropertyType == typeof(SolidColorBrush) && x.Name.Contains(prefix))
				.ToDictionary(
					x => (ColorTarget)Enum.Parse(typeof(ColorTarget), x.Name.Replace(prefix, "")),
					x => (SolidColorBrush)x.GetValue(null)
				).ToImmutableDictionary();
		}
	}
}