using System;
using System.Collections.Generic;
using System.Linq;
using Advobot.Classes.Attributes;
using Advobot.Enums;
using Advobot.SharedUI;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Media;
using Newtonsoft.Json;
using AdvorangesUtils;
using Avalonia.Styling;
using System.Collections.Immutable;

namespace Advobot.NetCoreUI.Classes
{
	/// <summary>
	/// Color settings for Advobot's .Net Core UI.
	/// </summary>
	public sealed class NetCoreColorSettings : ColorSettings<SolidColorBrush, NetCoreBrushFactory>
	{
		public static string BaseBackground => nameof(BaseBackground);
		public static string BaseForeground => nameof(BaseForeground);
		public static string BaseBorder => nameof(BaseBorder);
		public static string ButtonBackground => nameof(ButtonBackground);
		public static string ButtonForeground => nameof(ButtonForeground);
		public static string ButtonBorder => nameof(ButtonBorder);
		//public static string ButtonDisabledBackground => nameof(ButtonDisabledBackground);
		//public static string ButtonDisabledForeground => nameof(ButtonDisabledForeground);
		//public static string ButtonDisabledBorder => nameof(ButtonDisabledBorder);
		public static string ButtonMouseOverBackground => nameof(ButtonMouseOverBackground);
		//public static string JsonDigits => nameof(JsonDigits);
		//public static string JsonValue => nameof(JsonValue);
		//public static string JsonParamName => nameof(JsonParamName);

		[JsonProperty("ColorTargets"), Setting(NonCompileTimeDefaultValue.ResetDictionaryValues)]
		private Dictionary<string, SolidColorBrush> _ColorTargets { get; set; } = new Dictionary<string, SolidColorBrush>();
		/// <inheritdoc />
		[JsonProperty("Theme"), Setting(ColorTheme.Classic)]
		public override ColorTheme Theme
		{
			get => _Theme;
			set
			{
				_Theme = value;
				switch (Theme)
				{
					case ColorTheme.Classic:
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
						foreach (var kvp in _ColorTargets)
						{
							UpdateResources(kvp.Key, kvp.Value);
						}
						break;
				}
				//TODO: put this into .Net Core UI eventually?
				//SetSyntaxHighlightingColors("Json");
				NotifyPropertyChanged();
			}
		}
		[JsonIgnore]
		private ColorTheme _Theme = ColorTheme.Classic;
		[JsonIgnore]
		private IResourceDictionary _Resources;
		[JsonIgnore]
		private readonly ImmutableDictionary<string, string[]> _ColorNameMappings = new Dictionary<string, string[]>
		{
			{ BaseBackground, new[] { "ThemeBackgroundBrush" } },
			//{ BaseForeground, "ThemeAccentBrush" },
			{ BaseBorder, new[] { "ThemeBorderMidBrush", "ThemeBorderDarkBrush" } },
			{ ButtonBackground, new[] { "ThemeControlMidBrush" } },
			{ ButtonForeground, new[] { "ThemeForegroundBrush" } },
			{ ButtonBorder, new[] { "ThemeBorderLightBrush" } },
			{ ButtonMouseOverBackground, new[] { "ThemeBorderMidBrush" } },
		}.ToImmutableDictionary();

		/// <summary>
		/// Creates an instance of <see cref="NetCoreColorSettings"/> and sets the default theme and colors to classic.
		/// </summary>
		public NetCoreColorSettings()
		{
			Theme = ColorTheme.Classic;
		}

		/// <inheritdoc />
		public override SolidColorBrush this[string target]
		{
			get => _ColorTargets[target];
			set
			{
				_ColorTargets[target] = value;
				if (_Theme == ColorTheme.UserMade)
				{
					UpdateResources(target, value);
				}
			}
		}
		private void UpdateResources(string target, SolidColorBrush value)
		{
			if (_Resources == null)
			{
				var styles = Application.Current.Styles.OfType<StyleInclude>();
				var colors = styles.Single(x => x.Source.ToString().CaseInsContains("BaseLight.xaml"));
				_Resources = ((Style)colors.Loaded).Resources;
			}

			//If this is remapped to take advantage of how it's easy to recolor already defined background, etc then do that
			if (_ColorNameMappings.TryGetValue(target, out var names))
			{
				foreach (var name in names)
				{
					_Resources[name] = value;
				}
			}
			//Otherwise, if it's like the Json stuff, set it globally.
			else
			{
				Application.Current.Resources[target] = value;
			}
		}
	}
}