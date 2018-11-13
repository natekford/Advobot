using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Advobot.Classes;
using Advobot.Interfaces;
using Advobot.Utilities;
using AdvorangesSettingParser.Implementation.Instance;
using AdvorangesUtils;
using Newtonsoft.Json;

namespace Advobot.NetCoreUI.Classes.AbstractUI.Colors
{
	/// <summary>
	/// Indicates what colors to use in the UI.
	/// </summary>
	public abstract class ColorSettings<TBrush, TBrushFactory> : SettingsBase, IColorSettings<TBrush>
		where TBrushFactory : BrushFactory<TBrush>, new()
	{
		/// <summary>
		/// A light color UI theme.
		/// </summary>
		public static ITheme<TBrush> LightMode { get; } = new Theme<TBrush, TBrushFactory>
		{
			{ ColorTargets.BaseBackground,            "#FFFFFF" },
			{ ColorTargets.BaseForeground,            "#000000" },
			{ ColorTargets.BaseBorder,                "#ABADB3" },
			{ ColorTargets.ButtonBackground,          "#DDDDDD" },
			{ ColorTargets.ButtonForeground,          "#0E0E0E" },
			{ ColorTargets.ButtonBorder,              "#707070" },
			{ ColorTargets.ButtonDisabledBackground,  "#F4F4F4" },
			{ ColorTargets.ButtonDisabledForeground,  "#888888" },
			{ ColorTargets.ButtonDisabledBorder,      "#ADB2B5" },
			{ ColorTargets.ButtonMouseOverBackground, "#BEE6FD" },
			{ ColorTargets.JsonDigits,                "#8700FF" },
			{ ColorTargets.JsonValue,                 "#000CFF" },
			{ ColorTargets.JsonParamName,             "#057500" },
		};
		/// <summary>
		/// A dark color UI theme.
		/// </summary>
		public static ITheme<TBrush> DarkMode { get; } = new Theme<TBrush, TBrushFactory>
		{
			{ ColorTargets.BaseBackground,            "#1C1C1C" },
			{ ColorTargets.BaseForeground,            "#E1E1E1" },
			{ ColorTargets.BaseBorder,                "#ABADB3" },
			{ ColorTargets.ButtonBackground,          "#151515" },
			{ ColorTargets.ButtonForeground,          "#E1E1E1" },
			{ ColorTargets.ButtonBorder,              "#ABADB3" },
			{ ColorTargets.ButtonDisabledBackground,  "#343434" },
			{ ColorTargets.ButtonDisabledForeground,  "#A0A0A0" },
			{ ColorTargets.ButtonDisabledBorder,      "#ADB2B5" },
			{ ColorTargets.ButtonMouseOverBackground, "#303333" },
			{ ColorTargets.JsonDigits,                "#8700FF" },
			{ ColorTargets.JsonValue,                 "#0051FF" },
			{ ColorTargets.JsonParamName,             "#057500" },
		};
		/// <summary>
		/// Static instance of the brush factory.
		/// </summary>
		public static BrushFactory<TBrush> Factory { get; } = new TBrushFactory();

		/// <inheritdoc />
		[JsonProperty("ColorTargets", Order = 1)]
		public ITheme<TBrush> UserDefinedColors { get; private set; } = new Theme<TBrush, TBrushFactory>();
		/// <inheritdoc />
		[JsonProperty("Theme", Order = 2)]
		public ColorTheme Theme
		{
			get => _Theme;
			set
			{
				_Theme = value;
				switch (value)
				{
					case ColorTheme.LightMode:
						UpdateResources(LightMode);
						break;
					case ColorTheme.DarkMode:
						UpdateResources(DarkMode);
						break;
					case ColorTheme.UserMade:
						UpdateResources(UserDefinedColors);
						break;
				}
				AfterThemeUpdated();
				RaisePropertyChanged();
			}
		}
		[JsonIgnore]
		private ColorTheme _Theme = ColorTheme.LightMode;

		static ColorSettings()
		{
			LightMode.Freeze();
			DarkMode.Freeze();
		}

		/// <summary>
		/// Creates an instance of <see cref="ColorSettings{TBrush, TBrushFactory}"/> and sets the default theme and colors to light.
		/// </summary>
		public ColorSettings()
		{
			SettingParser.Add(new Setting<ColorTheme>(() => Theme)
			{
				ResetValueFactory = x => ColorTheme.LightMode,
			});
			SettingParser.Add(new CollectionSetting<KeyValuePair<string, TBrush>>(() => UserDefinedColors, parser: TryParseBrush)
			{
				ResetValueFactory = x =>
				{
					x.Clear();
					foreach (var key in LightMode.Keys)
					{
						if (!x.Any(kvp => kvp.Key == key))
						{
							x.Add(new KeyValuePair<string, TBrush>(key, LightMode[key]));
						}
					}
					return x;
				},
			});

			Theme = ColorTheme.LightMode;
			foreach (var key in LightMode.Keys)
			{
				if (!UserDefinedColors.TryGetValue(key, out var val))
				{
					UserDefinedColors.Add(key, LightMode[key]);
				}
			}
			UserDefinedColors.PropertyChanged += OnPropertyChanged;
		}

		private bool TryParseBrush(string s, out KeyValuePair<string, TBrush> kvp)
		{
			var parts = s.Split(new[] { ' ' }, 2);
			if (parts.Length != 2 || !Factory.TryCreateBrush(parts[1], out var brush))
			{
				kvp = default;
				return false;
			}

			kvp = new KeyValuePair<string, TBrush>(parts[0], brush);
			return true;
		}
		private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (Theme == ColorTheme.UserMade)
			{
				UpdateResource(e.PropertyName, ((ITheme<TBrush>)sender)[e.PropertyName]);
			}
		}
		private void UpdateResources(ITheme<TBrush> theme)
		{
			foreach (var key in theme.Keys)
			{
				UpdateResource(key, theme[key]);
			}
		}
		/// <summary>
		/// Updates a resource dictionary with the specified value.
		/// </summary>
		/// <param name="target"></param>
		/// <param name="value"></param>
		protected abstract void UpdateResource(string target, TBrush value);
		/// <summary>
		/// Does an action after the theme has been updated.
		/// </summary>
		protected virtual void AfterThemeUpdated() { }
		/// <inheritdoc />
		public override FileInfo GetFile(IBotDirectoryAccessor accessor)
			=> StaticGetPath(accessor);
		/// <summary>
		/// Loads the UI settings from file.
		/// </summary>
		/// <param name="accessor"></param>
		/// <returns></returns>
		public static T Load<T>(IBotDirectoryAccessor accessor) where T : ColorSettings<TBrush, TBrushFactory>, new()
			=> IOUtils.DeserializeFromFile<T>(StaticGetPath(accessor)) ?? new T();
		private static FileInfo StaticGetPath(IBotDirectoryAccessor accessor)
			=> accessor.GetBaseBotDirectoryFile("UISettings.json");
	}
}