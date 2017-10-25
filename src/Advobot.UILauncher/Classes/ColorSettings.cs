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
		[JsonIgnore]
		private static Brush _LightModeBackground = UIModification.MakeBrush("#FFFFFF");
		[JsonIgnore]
		private static Brush _LightModeForeground = UIModification.MakeBrush("#000000");
		[JsonIgnore]
		private static Brush _LightModeBorder = UIModification.MakeBrush("#ABADB3");
		[JsonIgnore]
		private static Brush _LightModeButtonBackground = UIModification.MakeBrush("#DDDDDD");
		[JsonIgnore]
		private static Brush _LightModeButtonBorder = UIModification.MakeBrush("#707070");
		[JsonIgnore]
		private static Brush _LightModeButtonDisabledBackground = UIModification.MakeBrush("#F4F4F4");
		[JsonIgnore]
		private static Brush _LightModeButtonDisabledForeground = UIModification.MakeBrush("#888888");
		[JsonIgnore]
		private static Brush _LightModeButtonDisabledBorder = UIModification.MakeBrush("#ADB2B5");
		[JsonIgnore]
		private static Brush _LightModeButtonMouseOver = UIModification.MakeBrush("#BEE6FD");
		[JsonIgnore]
		private static Style _LightModeButtonStyle = AdvobotButton.MakeButtonStyle
		(
			_LightModeButtonBackground,
			_LightModeForeground,
			_LightModeButtonBorder,
			_LightModeButtonDisabledBackground,
			_LightModeButtonDisabledForeground,
			_LightModeButtonDisabledBorder,
			_LightModeButtonMouseOver
		);

		[JsonIgnore]
		private static Brush _DarkModeBackground = UIModification.MakeBrush("#1C1C1C");
		[JsonIgnore]
		private static Brush _DarkModeForeground = UIModification.MakeBrush("#E1E1E1");
		[JsonIgnore]
		private static Brush _DarkModeBorder = UIModification.MakeBrush("#ABADB3");
		[JsonIgnore]
		private static Brush _DarkModeButtonBackground = UIModification.MakeBrush("#151515");
		[JsonIgnore]
		private static Brush _DarkModeButtonBorder = UIModification.MakeBrush("#ABADB3");
		[JsonIgnore]
		private static Brush _DarkModeButtonDisabledBackground = UIModification.MakeBrush("#343434");
		[JsonIgnore]
		private static Brush _DarkModeButtonDisabledForeground = UIModification.MakeBrush("#A0A0A0");
		[JsonIgnore]
		private static Brush _DarkModeButtonDisabledBorder = UIModification.MakeBrush("#ADB2B5");
		[JsonIgnore]
		private static Brush _DarkModeButtonMouseOver = UIModification.MakeBrush("#303333");
		[JsonIgnore]
		private static Style _DarkModeButtonStyle = AdvobotButton.MakeButtonStyle
		(
			_DarkModeButtonBackground,
			_DarkModeForeground,
			_DarkModeButtonBorder,
			_DarkModeButtonDisabledBackground,
			_DarkModeButtonDisabledForeground,
			_DarkModeButtonDisabledBorder,
			_DarkModeButtonMouseOver
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
		private Dictionary<ColorTarget, Brush> ColorTargets = new Dictionary<ColorTarget, Brush>();

		static ColorSettings()
		{
			SetClassicTheme();
		}
		public ColorSettings()
		{
			foreach (var target in Enum.GetValues(typeof(ColorTarget)).Cast<ColorTarget>())
			{
				ColorTargets.Add(target, null);
			}
		}

		public Brush this[ColorTarget target]
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
			var res = Application.Current.Resources;
			res[ColorTarget.BaseBackground] = _LightModeBackground;
			res[ColorTarget.BaseForeground] = _LightModeForeground;
			res[ColorTarget.BaseBorder] = _LightModeBorder;
			res[ColorTarget.ButtonBackground] = _LightModeButtonBackground;
			res[ColorTarget.ButtonBorder] = _LightModeButtonBorder;
			res[ColorTarget.ButtonDisabledBackground] = _LightModeButtonDisabledBackground;
			res[ColorTarget.ButtonDisabledForeground] = _LightModeButtonDisabledForeground;
			res[ColorTarget.ButtonDisabledBorder] = _LightModeButtonDisabledBorder;
			res[ColorTarget.ButtonMouseOverBackground] = _LightModeButtonMouseOver;
			res[OtherTarget.Button_Style] = _LightModeButtonStyle;
		}
		private static void SetDarkModeTheme()
		{
			var res = Application.Current.Resources;
			res[ColorTarget.BaseBackground] = _DarkModeBackground;
			res[ColorTarget.BaseForeground] = _DarkModeForeground;
			res[ColorTarget.BaseBorder] = _DarkModeBorder;
			res[ColorTarget.ButtonBackground] = _DarkModeButtonBackground;
			res[ColorTarget.ButtonBorder] = _DarkModeButtonBorder;
			res[ColorTarget.ButtonDisabledBackground] = _DarkModeButtonDisabledBackground;
			res[ColorTarget.ButtonDisabledForeground] = _DarkModeButtonDisabledForeground;
			res[ColorTarget.ButtonDisabledBorder] = _DarkModeButtonDisabledBorder;
			res[ColorTarget.ButtonMouseOverBackground] = _DarkModeButtonMouseOver;
			res[OtherTarget.Button_Style] = _DarkModeButtonStyle;
		}
		private void SetCustomTheme()
		{
			var res = Application.Current.Resources;
			foreach (var kvp in ColorTargets)
			{
				res[kvp.Key] = kvp.Value;
			}
			res[OtherTarget.Button_Style] = AdvobotButton.MakeButtonStyle
			(
				(Brush)res[ColorTarget.BaseBackground],
				(Brush)res[ColorTarget.BaseForeground],
				(Brush)res[ColorTarget.BaseBorder],
				(Brush)res[ColorTarget.ButtonDisabledBackground],
				(Brush)res[ColorTarget.ButtonDisabledForeground],
				(Brush)res[ColorTarget.ButtonDisabledBorder],
				(Brush)res[ColorTarget.ButtonMouseOverBackground]
			);
		}
		public void SaveSettings()
		{
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
						button.SetResourceReference(Button.StyleProperty, OtherTarget.Button_Style);
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
		public static ColorSettings LoadUISettings(bool loaded)
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
			else if (loaded)
			{
				ConsoleActions.WriteLine("The bot UI information file could not be found; using default.");
			}
			return UISettings ?? new ColorSettings();
		}
	}
}