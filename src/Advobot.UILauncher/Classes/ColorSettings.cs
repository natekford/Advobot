using Advobot.Core;
using Advobot.Core.Utilities;
using Advobot.UILauncher.Enums;
using Advobot.UILauncher.Utilities;
using ICSharpCode.AvalonEdit.Highlighting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Media;

namespace Advobot.UILauncher.Classes
{
	internal class ColorSettings
	{
		public static ImmutableDictionary<ColorTarget, SolidColorBrush> LightModeProperties { get; private set; } = GetColorProperties("LightMode");
		public static ImmutableDictionary<ColorTarget, SolidColorBrush> DarkModeProperties { get; private set; } = GetColorProperties("DarkMode");

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

		[JsonIgnore]
		private ColorTheme _Theme = ColorTheme.Classic;
		[JsonProperty("Theme")]
		public ColorTheme Theme
		{
			get => _Theme;
			set
			{
				_Theme = value;
				ActivateTheme();
			}
		}
		[JsonProperty("ColorTargets")]
		private Dictionary<ColorTarget, SolidColorBrush> ColorTargets = new Dictionary<ColorTarget, SolidColorBrush>();

		public ColorSettings()
		{
			foreach (var target in Enum.GetValues(typeof(ColorTarget)).Cast<ColorTarget>())
			{
				ColorTargets.Add(target, null);
			}
		}

		public SolidColorBrush this[ColorTarget target]
		{
			get => ColorTargets[target];
			set => ColorTargets[target] = value;
		}
		public bool TryGetValue(ColorTarget target, out SolidColorBrush brush)
		{
			return ColorTargets.TryGetValue(target, out brush);
		}
		public void SetSyntaxHighlightingColors(params string[] names)
		{
			foreach (var name in names)
			{
				var highlighting = HighlightingManager.Instance.GetDefinition(name)
					?? throw new ArgumentException("not a valid highlighting.", name);

				foreach (var namedColor in highlighting.NamedHighlightingColors)
				{
					//E.G.: Highlighting name is JSON, color name is Param, searches for JSONParam
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
		/// <summary>
		/// Saves custom colors and the current theme.
		/// </summary>
		public void SaveSettings()
		{
			IOUtils.OverWriteFile(IOUtils.GetBaseBotDirectoryFile(Constants.UI_INFO_LOC), IOUtils.Serialize(this));
		}

		public static ColorSettings LoadUISettings()
		{
			var fileInfo = IOUtils.GetBaseBotDirectoryFile(Constants.UI_INFO_LOC);
			return IOUtils.DeserializeFromFile<ColorSettings>(fileInfo, typeof(ColorSettings), true);
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
		private void ActivateTheme()
		{
			var r = Application.Current.Resources;
			switch (Theme)
			{
				case ColorTheme.Classic:
				{
					foreach (ColorTarget ct in Enum.GetValues(typeof(ColorTarget)))
					{
						r[ct] = LightModeProperties[ct];
					}
					break;
				}
				case ColorTheme.DarkMode:
				{
					foreach (ColorTarget ct in Enum.GetValues(typeof(ColorTarget)))
					{
						r[ct] = DarkModeProperties[ct];
					}
					break;
				}
				case ColorTheme.UserMade:
				{
					foreach (var kvp in ColorTargets)
					{
						r[kvp.Key] = kvp.Value;
					}
					break;
				}
			}
			SetSyntaxHighlightingColors("JSON");
		}
	}
}