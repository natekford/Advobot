using Advobot.Core.Actions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Advobot.UILauncher.Actions;
using Advobot.UILauncher.Enums;
using System.Windows.Controls;
using Advobot.Core;

namespace Advobot.UILauncher.Classes
{
	internal class ColorSettings
	{
		public static SolidColorBrush LightModeBackground => UIModification.MakeSolidColorBrush("#FFFFFF");
		public static SolidColorBrush LightModeForeground => UIModification.MakeSolidColorBrush("#000000");
		public static SolidColorBrush LightModeBorder => UIModification.MakeSolidColorBrush("#ABADB3");
		public static SolidColorBrush LightModeButtonBackground => UIModification.MakeSolidColorBrush("#DDDDDD");
		public static SolidColorBrush LightModeButtonBorder => UIModification.MakeSolidColorBrush("#707070");
		public static SolidColorBrush LightModeButtonDisabledBackground => UIModification.MakeSolidColorBrush("#F4F4F4");
		public static SolidColorBrush LightModeButtonDisabledForeground => UIModification.MakeSolidColorBrush("#888888");
		public static SolidColorBrush LightModeButtonDisabledBorder => UIModification.MakeSolidColorBrush("#ADB2B5");
		public static SolidColorBrush LightModeButtonMouseOver => UIModification.MakeSolidColorBrush("#BEE6FD");
		public static Style LightModeButtonStyle => AdvobotButton.MakeButtonStyle
		(
			LightModeButtonBackground,
			LightModeForeground,
			LightModeButtonBorder,
			LightModeButtonDisabledBackground,
			LightModeButtonDisabledForeground,
			LightModeButtonDisabledBorder,
			LightModeButtonMouseOver
		);

		public static SolidColorBrush DarkModeBackground => UIModification.MakeSolidColorBrush("#1C1C1C");
		public static SolidColorBrush DarkModeForeground => UIModification.MakeSolidColorBrush("#E1E1E1");
		public static SolidColorBrush DarkModeBorder => UIModification.MakeSolidColorBrush("#ABADB3");
		public static SolidColorBrush DarkModeButtonBackground => UIModification.MakeSolidColorBrush("#151515");
		public static SolidColorBrush DarkModeButtonBorder => UIModification.MakeSolidColorBrush("#ABADB3");
		public static SolidColorBrush DarkModeButtonDisabledBackground => UIModification.MakeSolidColorBrush("#343434");
		public static SolidColorBrush DarkModeButtonDisabledForeground => UIModification.MakeSolidColorBrush("#A0A0A0");
		public static SolidColorBrush DarkModeButtonDisabledBorder => UIModification.MakeSolidColorBrush("#ADB2B5");
		public static SolidColorBrush DarkModeButtonMouseOver => UIModification.MakeSolidColorBrush("#303333");
		public static Style DarkModeButtonStyle => AdvobotButton.MakeButtonStyle
		(
			DarkModeButtonBackground,
			DarkModeForeground,
			DarkModeButtonBorder,
			DarkModeButtonDisabledBackground,
			DarkModeButtonDisabledForeground,
			DarkModeButtonDisabledBorder,
			DarkModeButtonMouseOver
		);

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
			var r = Application.Current.MainWindow.Resources;
			r[ColorTarget.BaseBackground] = LightModeBackground;
			r[ColorTarget.BaseForeground] = LightModeForeground;
			r[ColorTarget.BaseBorder] = LightModeBorder;
			r[ColorTarget.ButtonBackground] = LightModeButtonBackground;
			r[ColorTarget.ButtonBorder] = LightModeButtonBorder;
			r[ColorTarget.ButtonDisabledBackground] = LightModeButtonDisabledBackground;
			r[ColorTarget.ButtonDisabledForeground] = LightModeButtonDisabledForeground;
			r[ColorTarget.ButtonDisabledBorder] = LightModeButtonDisabledBorder;
			r[ColorTarget.ButtonMouseOverBackground] = LightModeButtonMouseOver;
			r[OtherTarget.ButtonStyle] = LightModeButtonStyle;
		}
		private static void SetDarkModeTheme()
		{
			var r = Application.Current.MainWindow.Resources;
			r[ColorTarget.BaseBackground] = DarkModeBackground;
			r[ColorTarget.BaseForeground] = DarkModeForeground;
			r[ColorTarget.BaseBorder] = DarkModeBorder;
			r[ColorTarget.ButtonBackground] = DarkModeButtonBackground;
			r[ColorTarget.ButtonBorder] = DarkModeButtonBorder;
			r[ColorTarget.ButtonDisabledBackground] = DarkModeButtonDisabledBackground;
			r[ColorTarget.ButtonDisabledForeground] = DarkModeButtonDisabledForeground;
			r[ColorTarget.ButtonDisabledBorder] = DarkModeButtonDisabledBorder;
			r[ColorTarget.ButtonMouseOverBackground] = DarkModeButtonMouseOver;
			r[OtherTarget.ButtonStyle] = DarkModeButtonStyle;
		}
		private void SetCustomTheme()
		{
			var r = Application.Current.MainWindow.Resources;
			foreach (var kvp in ColorTargets)
			{
				r[kvp.Key] = kvp.Value;
			}
			r[OtherTarget.ButtonStyle] = AdvobotButton.MakeButtonStyle
			(
				ColorTargets[ColorTarget.BaseBackground],
				ColorTargets[ColorTarget.BaseForeground],
				ColorTargets[ColorTarget.BaseBorder],
				ColorTargets[ColorTarget.ButtonDisabledBackground],
				ColorTargets[ColorTarget.ButtonDisabledForeground],
				ColorTargets[ColorTarget.ButtonDisabledBorder],
				ColorTargets[ColorTarget.ButtonMouseOverBackground]
			);
		}
		public void SaveSettings()
		{
			//Only needs to save custom colors and the theme
			//Current set colors aren't important
			SavingAndLoadingActions.OverWriteFile(GetActions.GetBaseBotDirectoryFile(Constants.UI_INFO_LOCATION), SavingAndLoadingActions.Serialize(this));
		}

		public static void SwitchElementColorOfChildren(DependencyObject parent)
		{
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
						button.SetResourceReference(Button.ForegroundProperty, ColorTarget.BaseForeground);
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