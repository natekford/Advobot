using Advobot.Core;
using Advobot.Core.Actions;
using Advobot.UILauncher.Actions;
using Advobot.UILauncher.Classes.Controls;
using Advobot.UILauncher.Enums;
using Advobot.UILauncher.Windows;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Advobot.UILauncher.Classes
{
	internal class ColorSettings
	{
		public static ImmutableDictionary<ColorTarget, SolidColorBrush> LightModeProperties { get; private set; } = GetColorProperties("LightMode");
		public static ImmutableDictionary<ColorTarget, SolidColorBrush> DarkModeProperties { get; private set; } = GetColorProperties("DarkMode");

		public static SolidColorBrush LightModeBaseBackground { get; private set; } = AdvobotColor.CreateBrush("#FFFFFF");
		public static SolidColorBrush LightModeBaseForeground => AdvobotColor.CreateBrush("#000000");
		public static SolidColorBrush LightModeBaseBorder => AdvobotColor.CreateBrush("#ABADB3");
		public static SolidColorBrush LightModeButtonBackground => AdvobotColor.CreateBrush("#DDDDDD");
		public static SolidColorBrush LightModeButtonForeground => AdvobotColor.CreateBrush("#0E0E0E");
		public static SolidColorBrush LightModeButtonBorder => AdvobotColor.CreateBrush("#707070");
		public static SolidColorBrush LightModeButtonDisabledBackground => AdvobotColor.CreateBrush("#F4F4F4");
		public static SolidColorBrush LightModeButtonDisabledForeground => AdvobotColor.CreateBrush("#888888");
		public static SolidColorBrush LightModeButtonDisabledBorder => AdvobotColor.CreateBrush("#ADB2B5");
		public static SolidColorBrush LightModeButtonMouseOverBackground => AdvobotColor.CreateBrush("#BEE6FD");
		public static SolidColorBrush LightModeJsonDigits => AdvobotColor.CreateBrush("#8700FF");
		public static SolidColorBrush LightModeJsonValue => AdvobotColor.CreateBrush("#000CFF");
		public static SolidColorBrush LightModeJsonParamName => AdvobotColor.CreateBrush("#057500");

		public static SolidColorBrush DarkModeBaseBackground => AdvobotColor.CreateBrush("#1C1C1C");
		public static SolidColorBrush DarkModeBaseForeground => AdvobotColor.CreateBrush("#E1E1E1");
		public static SolidColorBrush DarkModeBaseBorder => AdvobotColor.CreateBrush("#ABADB3");
		public static SolidColorBrush DarkModeButtonBackground => AdvobotColor.CreateBrush("#151515");
		public static SolidColorBrush DarkModeButtonForeground => AdvobotColor.CreateBrush("#E1E1E1");
		public static SolidColorBrush DarkModeButtonBorder => AdvobotColor.CreateBrush("#ABADB3");
		public static SolidColorBrush DarkModeButtonDisabledBackground => AdvobotColor.CreateBrush("#343434");
		public static SolidColorBrush DarkModeButtonDisabledForeground => AdvobotColor.CreateBrush("#A0A0A0");
		public static SolidColorBrush DarkModeButtonDisabledBorder => AdvobotColor.CreateBrush("#ADB2B5");
		public static SolidColorBrush DarkModeButtonMouseOverBackground => AdvobotColor.CreateBrush("#303333");
		public static SolidColorBrush DarkModeJsonDigits => AdvobotColor.CreateBrush("#8700FF");
		public static SolidColorBrush DarkModeJsonValue => AdvobotColor.CreateBrush("#000CFF");
		public static SolidColorBrush DarkModeJsonParamName => AdvobotColor.CreateBrush("#057500");

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

		private void ActivateTheme()
		{
			switch (Theme)
			{
				case ColorTheme.Classic:
				{
					SetClassicTheme();
					return;
				}
				case ColorTheme.DarkMode:
				{
					SetDarkModeTheme();
					return;
				}
				case ColorTheme.UserMade:
				{
					SetCustomTheme();
					return;
				}
			}
		}
		private static void SetClassicTheme()
		{
			var r = Application.Current.Resources;
			foreach (ColorTarget ct in Enum.GetValues(typeof(ColorTarget)))
			{
				r[ct] = LightModeProperties[ct];
			}
		}
		private static void SetDarkModeTheme()
		{
			var r = Application.Current.Resources;
			foreach (ColorTarget ct in Enum.GetValues(typeof(ColorTarget)))
			{
				r[ct] = DarkModeProperties[ct];
			}
		}
		private void SetCustomTheme()
		{
			var r = Application.Current.Resources;
			foreach (var kvp in ColorTargets)
			{
				r[kvp.Key] = kvp.Value;
			}
		}
		public void SaveSettings()
		{
			//Only needs to save custom colors and the theme
			//Current set colors aren't important
			SavingAndLoadingActions.OverWriteFile(GetActions.GetBaseBotDirectoryFile(Constants.UI_INFO_LOCATION), SavingAndLoadingActions.Serialize(this));
		}

		public static ImmutableDictionary<ColorTarget, SolidColorBrush> GetColorProperties(string prefix)
		{
			return typeof(ColorSettings)
				.GetProperties(BindingFlags.Public | BindingFlags.Static)
				.Where(x => x.PropertyType == typeof(SolidColorBrush) && x.Name.Contains(prefix))
				.ToDictionary(
					x => (ColorTarget)Enum.Parse(typeof(ColorTarget), x.Name.Replace(prefix, "")),
					x => (SolidColorBrush)x.GetValue(null)
				).ToImmutableDictionary();
		}
		public static void SetAllColorBindingsOnChildren(DependencyObject parent)
		{
			if (parent is AdvobotWindow)
			{
				SetClassicTheme();
			}

			foreach (var child in parent.GetChildren())
			{
				if (child is AdvobotButton button)
				{
					if (button.Style == null)
					{
						button.SetResourceReference(Button.StyleProperty, OtherTarget.ButtonStyle);
					}
				}
				else if (child is Control control)
				{
					if (control.Background == null)
					{
						control.SetResourceReference(Control.BackgroundProperty, ColorTarget.BaseBackground);
					}
					if (control.Foreground == null)
					{
						control.SetResourceReference(Control.ForegroundProperty, ColorTarget.BaseForeground);
					}
					if (control.BorderBrush == null)
					{
						control.SetResourceReference(Control.BorderBrushProperty, ColorTarget.BaseBorder);
					}
				}

				if (child.GetChildren().Any())
				{
					SetAllColorBindingsOnChildren(child);
				}
			}
		}
		public static ColorSettings LoadUISettings()
		{
			ColorSettings UISettings = null;
			var fileInfo = GetActions.GetBaseBotDirectoryFile(Constants.UI_INFO_LOCATION);
			if (fileInfo.Exists)
			{
				try
				{
					using (var reader = new StreamReader(fileInfo.FullName))
					{
						UISettings = JsonConvert.DeserializeObject<ColorSettings>(reader.ReadToEnd());
					}
					ConsoleActions.WriteLine("The bot UI information has successfully been loaded.");
				}
				catch (Exception e)
				{
					ConsoleActions.ExceptionToConsole(e);
				}
			}
			else
			{
				ConsoleActions.WriteLine("The bot UI information file could not be found; using default.");
			}
			return UISettings ?? new ColorSettings();
		}
	}
}