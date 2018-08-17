using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Advobot.Classes.Attributes;
using Advobot.Enums;
using Advobot.SharedUI;
using AdvorangesUtils;
using ICSharpCode.AvalonEdit.Highlighting;
using Newtonsoft.Json;

namespace Advobot.NetFrameworkUI.Classes
{
	/// <summary>
	/// Color settings for Advobot's .Net Framework UI.
	/// </summary>
	public sealed class NetFrameworkColorSettings : ColorSettings<SolidColorBrush, NetFrameworkBrushFactory>
	{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
		public static string BaseBackground => nameof(BaseBackground);
		public static string BaseForeground => nameof(BaseForeground);
		public static string BaseBorder => nameof(BaseBorder);
		public static string ButtonBackground => nameof(ButtonBackground);
		public static string ButtonForeground => nameof(ButtonForeground);
		public static string ButtonBorder => nameof(ButtonBorder);
		public static string ButtonDisabledBackground => nameof(ButtonDisabledBackground);
		public static string ButtonDisabledForeground => nameof(ButtonDisabledForeground);
		public static string ButtonDisabledBorder => nameof(ButtonDisabledBorder);
		public static string ButtonMouseOverBackground => nameof(ButtonMouseOverBackground);
		public static string JsonDigits => nameof(JsonDigits);
		public static string JsonValue => nameof(JsonValue);
		public static string JsonParamName => nameof(JsonParamName);
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

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
							Application.Current.Resources[ct] = LightModeProperties[ct];
						}
						break;
					case ColorTheme.DarkMode:
						foreach (var ct in Targets)
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

		/// <inheritdoc />
		public override SolidColorBrush this[string target]
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

		private void SetSyntaxHighlightingColors(params string[] names)
		{
			foreach (var name in names)
			{
				var highlighting = HighlightingManager.Instance.GetDefinition(name) ?? throw new ArgumentException("not a valid highlighting.", name);

				foreach (var namedColor in highlighting.NamedHighlightingColors)
				{
					//E.G.: Highlighting name is json, color name is Param, searches for jsonParam
					var colorName = highlighting.Name + namedColor.Name;
					if (!(Targets.SingleOrDefault(x => x.CaseInsEquals(colorName)) is string str))
					{
						continue;
					}

					//Get the set color, if one doesn't exist, use the default light mode
					var color = ((SolidColorBrush)Application.Current.Resources[str])?.Color ?? LightModeProperties[str].Color;
					namedColor.Foreground = new SimpleHighlightingBrush(color);
				}
			}
		}
	}
}