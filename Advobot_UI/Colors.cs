using Advobot.Actions;
using Advobot.Graphics.HelperActions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace Advobot.Graphics.Colors
{
	internal class UISettings
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
		private static Style _LightModeButtonStyle = UIModification.MakeButtonStyle(
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
		private static Style _DarkModeButtonStyle = UIModification.MakeButtonStyle(
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

		public UISettings()
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
		public void SaveSettings()
		{
			SavingAndLoadingActions.OverWriteFile(GetActions.GetBaseBotDirectoryFile(Constants.UI_INFO_LOCATION), SavingAndLoadingActions.Serialize(this));
		}
		public void ActivateTheme()
		{
			switch (Theme)
			{
				case ColorTheme.Classic:
				{
					ActivateClassic();
					return;
				}
				case ColorTheme.Dark_Mode:
				{
					ActivateDarkMode();
					return;
				}
				case ColorTheme.User_Made:
				{
					ActivateUserMade();
					return;
				}
			}
		}
		private void ActivateClassic()
		{
			var res = Application.Current.Resources;
			res[ColorTarget.Base_Background]				= _LightModeBackground;
			res[ColorTarget.Base_Foreground]				= _LightModeForeground;
			res[ColorTarget.Base_Border]					= _LightModeBorder;
			res[ColorTarget.Button_Background]				= _LightModeButtonBackground;
			res[ColorTarget.Button_Border]					= _LightModeButtonBorder;
			res[ColorTarget.Button_Disabled_Background]		= _LightModeButtonDisabledBackground;
			res[ColorTarget.Button_Disabled_Foreground]		= _LightModeButtonDisabledForeground;
			res[ColorTarget.Button_Disabled_Border]			= _LightModeButtonDisabledBorder;
			res[ColorTarget.Button_Mouse_Over_Background]	= _LightModeButtonMouseOver;
			res[OtherTarget.Button_Style]					= _LightModeButtonStyle;
		}
		private void ActivateDarkMode()
		{
			var res = Application.Current.Resources;
			res[ColorTarget.Base_Background]				= _DarkModeBackground;
			res[ColorTarget.Base_Foreground]				= _DarkModeForeground;
			res[ColorTarget.Base_Border]					= _DarkModeBorder;
			res[ColorTarget.Button_Background]				= _DarkModeButtonBackground;
			res[ColorTarget.Button_Border]					= _DarkModeButtonBorder;
			res[ColorTarget.Button_Disabled_Background]		= _DarkModeButtonDisabledBackground;
			res[ColorTarget.Button_Disabled_Foreground]		= _DarkModeButtonDisabledForeground;
			res[ColorTarget.Button_Disabled_Border]			= _DarkModeButtonDisabledBorder;
			res[ColorTarget.Button_Mouse_Over_Background]	= _DarkModeButtonMouseOver;
			res[OtherTarget.Button_Style]					= _DarkModeButtonStyle;
		}
		private void ActivateUserMade()
		{
			var res = Application.Current.Resources;
			foreach (var kvp in ColorTargets)
			{
				res[kvp.Key] = kvp.Value;
			}
			res[OtherTarget.Button_Style] = UIModification.MakeButtonStyle(
				(Brush)res[ColorTarget.Base_Background],
				(Brush)res[ColorTarget.Base_Foreground],
				(Brush)res[ColorTarget.Base_Border],
				(Brush)res[ColorTarget.Button_Disabled_Background],
				(Brush)res[ColorTarget.Button_Disabled_Foreground],
				(Brush)res[ColorTarget.Button_Disabled_Border],
				(Brush)res[ColorTarget.Button_Mouse_Over_Background]
				);
		}

		public static UISettings LoadUISettings(bool loaded)
		{
			UISettings UISettings = null;
			var fileInfo = GetActions.GetBaseBotDirectoryFile(Constants.UI_INFO_LOCATION);
			if (fileInfo.Exists)
			{
				try
				{
					using (var reader = new StreamReader(fileInfo.FullName))
					{
						UISettings = JsonConvert.DeserializeObject<UISettings>(reader.ReadToEnd());
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
			return UISettings ?? new UISettings();
		}
	}

	internal struct BrushTargetAndValue
	{
		[JsonProperty]
		public ColorTarget Target { get; private set; }
		[JsonProperty]
		public Brush Brush { get; private set; }

		public BrushTargetAndValue(ColorTarget target, string colorString)
		{
			Target = target;
			Brush = UIModification.MakeBrush(colorString);
		}
	}

	[Flags]
	internal enum ColorTheme : uint
	{
		Classic = (1U << 0),
		Dark_Mode = (1U << 1),
		User_Made = (1U << 2),
	}

	[Flags]
	internal enum ColorTarget : uint
	{
		Base_Background = (1U << 0),
		Base_Foreground = (1U << 1),
		Base_Border = (1U << 2),
		Button_Background = (1U << 3),
		Button_Border = (1U << 4),
		Button_Disabled_Background = (1U << 5),
		Button_Disabled_Foreground = (1U << 6),
		Button_Disabled_Border = (1U << 7),
		Button_Mouse_Over_Background = (1U << 8),
	}

	[Flags]
	internal enum OtherTarget : uint
	{
		Button_Style = (1U << 0),
	}
}
