using Advobot.Core;
using Advobot.Core.Actions;
using Advobot.UILauncher.Actions;
using Advobot.UILauncher.Classes.Controls;
using Advobot.UILauncher.Enums;
using Advobot.UILauncher.Windows;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Advobot.UILauncher.Classes
{
	internal class ColorSettings
	{
		public static SolidColorBrush LightModeBackground => AdvobotColor.CreateBrush("#FFFFFF");
		public static SolidColorBrush LightModeForeground => AdvobotColor.CreateBrush("#000000");
		public static SolidColorBrush LightModeBorder => AdvobotColor.CreateBrush("#ABADB3");
		public static SolidColorBrush LightModeButtonBackground => AdvobotColor.CreateBrush("#DDDDDD");
		public static SolidColorBrush LightModeButtonForeground => AdvobotColor.CreateBrush("#0E0E0E");
		public static SolidColorBrush LightModeButtonBorder => AdvobotColor.CreateBrush("#707070");
		public static SolidColorBrush LightModeButtonDisabledBackground => AdvobotColor.CreateBrush("#F4F4F4");
		public static SolidColorBrush LightModeButtonDisabledForeground => AdvobotColor.CreateBrush("#888888");
		public static SolidColorBrush LightModeButtonDisabledBorder => AdvobotColor.CreateBrush("#ADB2B5");
		public static SolidColorBrush LightModeButtonMouseOver => AdvobotColor.CreateBrush("#BEE6FD");

		public static SolidColorBrush DarkModeBackground => AdvobotColor.CreateBrush("#1C1C1C");
		public static SolidColorBrush DarkModeForeground => AdvobotColor.CreateBrush("#E1E1E1");
		public static SolidColorBrush DarkModeBorder => AdvobotColor.CreateBrush("#ABADB3");
		public static SolidColorBrush DarkModeButtonBackground => AdvobotColor.CreateBrush("#151515");
		public static SolidColorBrush DarkModeButtonForeground => AdvobotColor.CreateBrush("#E1E1E1");
		public static SolidColorBrush DarkModeButtonBorder => AdvobotColor.CreateBrush("#ABADB3");
		public static SolidColorBrush DarkModeButtonDisabledBackground => AdvobotColor.CreateBrush("#343434");
		public static SolidColorBrush DarkModeButtonDisabledForeground => AdvobotColor.CreateBrush("#A0A0A0");
		public static SolidColorBrush DarkModeButtonDisabledBorder => AdvobotColor.CreateBrush("#ADB2B5");
		public static SolidColorBrush DarkModeButtonMouseOver => AdvobotColor.CreateBrush("#303333");

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
			r[ColorTarget.BaseBackground] = LightModeBackground;
			r[ColorTarget.BaseForeground] = LightModeForeground;
			r[ColorTarget.BaseBorder] = LightModeBorder;
			r[ColorTarget.ButtonBackground] = LightModeButtonBackground;
			r[ColorTarget.ButtonForeground] = LightModeButtonForeground;
			r[ColorTarget.ButtonBorder] = LightModeButtonBorder;
			r[ColorTarget.ButtonDisabledBackground] = LightModeButtonDisabledBackground;
			r[ColorTarget.ButtonDisabledForeground] = LightModeButtonDisabledForeground;
			r[ColorTarget.ButtonDisabledBorder] = LightModeButtonDisabledBorder;
			r[ColorTarget.ButtonMouseOverBackground] = LightModeButtonMouseOver;
		}
		private static void SetDarkModeTheme()
		{
			var r = Application.Current.Resources;
			r[ColorTarget.BaseBackground] = DarkModeBackground;
			r[ColorTarget.BaseForeground] = DarkModeForeground;
			r[ColorTarget.BaseBorder] = DarkModeBorder;
			r[ColorTarget.ButtonBackground] = DarkModeButtonBackground;
			r[ColorTarget.ButtonForeground] = DarkModeButtonForeground;
			r[ColorTarget.ButtonBorder] = DarkModeButtonBorder;
			r[ColorTarget.ButtonDisabledBackground] = DarkModeButtonDisabledBackground;
			r[ColorTarget.ButtonDisabledForeground] = DarkModeButtonDisabledForeground;
			r[ColorTarget.ButtonDisabledBorder] = DarkModeButtonDisabledBorder;
			r[ColorTarget.ButtonMouseOverBackground] = DarkModeButtonMouseOver;
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

		public static void SwitchElementColorOfChildren(DependencyObject parent)
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
					if (button.Foreground == null)
					{
						button.SetResourceReference(Button.ForegroundProperty, ColorTarget.ButtonForeground);
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
					SwitchElementColorOfChildren(child);
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