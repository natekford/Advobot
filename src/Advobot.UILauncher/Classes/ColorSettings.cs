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

		[JsonProperty("Theme")]
		public ColorTheme Theme { get; private set; } = ColorTheme.Classic;
		[JsonProperty("ColorTargets")]
		public Dictionary<ColorTarget, Brush> ColorTargets { get; private set; } = new Dictionary<ColorTarget, Brush>();

		public ColorSettings()
		{
			foreach (var target in Enum.GetValues(typeof(ColorTarget)).Cast<ColorTarget>())
			{
				ColorTargets.Add(target, null);
			}
		}

		public void SetTheme(ColorTheme theme)
		{
			Theme = theme;
		}
		public void ActivateTheme()
		{
			var res = Application.Current.Resources;
			switch (Theme)
			{
				case ColorTheme.Classic:
				{
					res[ColorTarget.Base_Background] = _LightModeBackground;
					res[ColorTarget.Base_Foreground] = _LightModeForeground;
					res[ColorTarget.Base_Border] = _LightModeBorder;
					res[ColorTarget.Button_Background] = _LightModeButtonBackground;
					res[ColorTarget.Button_Border] = _LightModeButtonBorder;
					res[ColorTarget.Button_Disabled_Background] = _LightModeButtonDisabledBackground;
					res[ColorTarget.Button_Disabled_Foreground] = _LightModeButtonDisabledForeground;
					res[ColorTarget.Button_Disabled_Border] = _LightModeButtonDisabledBorder;
					res[ColorTarget.Button_Mouse_Over_Background] = _LightModeButtonMouseOver;
					res[OtherTarget.Button_Style] = _LightModeButtonStyle;
					return;
				}
				case ColorTheme.Dark_Mode:
				{
					res[ColorTarget.Base_Background] = _DarkModeBackground;
					res[ColorTarget.Base_Foreground] = _DarkModeForeground;
					res[ColorTarget.Base_Border] = _DarkModeBorder;
					res[ColorTarget.Button_Background] = _DarkModeButtonBackground;
					res[ColorTarget.Button_Border] = _DarkModeButtonBorder;
					res[ColorTarget.Button_Disabled_Background] = _DarkModeButtonDisabledBackground;
					res[ColorTarget.Button_Disabled_Foreground] = _DarkModeButtonDisabledForeground;
					res[ColorTarget.Button_Disabled_Border] = _DarkModeButtonDisabledBorder;
					res[ColorTarget.Button_Mouse_Over_Background] = _DarkModeButtonMouseOver;
					res[OtherTarget.Button_Style] = _DarkModeButtonStyle;
					return;
				}
				case ColorTheme.User_Made:
				{
					foreach (var kvp in ColorTargets)
					{
						res[kvp.Key] = kvp.Value;
					}
					res[OtherTarget.Button_Style] = AdvobotButton.MakeButtonStyle
					(
						(Brush)res[ColorTarget.Base_Background],
						(Brush)res[ColorTarget.Base_Foreground],
						(Brush)res[ColorTarget.Base_Border],
						(Brush)res[ColorTarget.Button_Disabled_Background],
						(Brush)res[ColorTarget.Button_Disabled_Foreground],
						(Brush)res[ColorTarget.Button_Disabled_Border],
						(Brush)res[ColorTarget.Button_Mouse_Over_Background]
					);
					return;
				}
			}
		}

		public static void SwitchElementColorOfChildren(DependencyObject parent)
		{
			foreach (var child in parent.GetChildren())
			{
				if (child is Grid grid)
				{
					SwitchElementColorOfChildren(grid);
				}
				else if (child is AdvobotButton button)
				{
					if (button.Style == null)
					{
						button.SetResourceReference(Button.StyleProperty, OtherTarget.Button_Style);
					}
					if (button.Foreground == null)
					{
						button.SetResourceReference(Button.ForegroundProperty, ColorTarget.Base_Foreground);
					}
				}
				else if (child is Control control)
				{
					if (control.Background == null)
					{
						control.SetResourceReference(Control.BackgroundProperty, ColorTarget.Base_Background);
					}
					if (control.Foreground == null)
					{
						control.SetResourceReference(Control.ForegroundProperty, ColorTarget.Base_Foreground);
					}
					if (control.BorderBrush == null)
					{
						control.SetResourceReference(Control.BorderBrushProperty, ColorTarget.Base_Border);
					}
				}
			}
		}

		public void SaveSettings()
		{
			SavingAndLoadingActions.OverWriteFile(GetActions.GetBaseBotDirectoryFile(Constants.UI_INFO_LOCATION), SavingAndLoadingActions.Serialize(this));
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