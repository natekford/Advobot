using Advobot.Actions;
using Advobot.Graphics.HelperFunctions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace Advobot
{
	namespace Graphics
	{
		namespace Colors
		{
			internal class UISettings
			{
				[JsonIgnore]
				private static readonly Brush _LightModeBackground = UIModification.MakeBrush("#FFFFFF");
				[JsonIgnore]
				private static readonly Brush _LightModeForeground = UIModification.MakeBrush("#000000");
				[JsonIgnore]
				private static readonly Brush _LightModeBorder = UIModification.MakeBrush("#ABADB3");
				[JsonIgnore]
				private static readonly Brush _LightModeButtonBackground = UIModification.MakeBrush("#DDDDDD");
				[JsonIgnore]
				private static readonly Brush _LightModeButtonBorder = UIModification.MakeBrush("#707070");
				[JsonIgnore]
				private static readonly Brush _LightModeButtonDisabledBackground = UIModification.MakeBrush("#F4F4F4");
				[JsonIgnore]
				private static readonly Brush _LightModeButtonDisabledForeground = UIModification.MakeBrush("#888888");
				[JsonIgnore]
				private static readonly Brush _LightModeButtonDisabledBorder = UIModification.MakeBrush("#ADB2B5");
				[JsonIgnore]
				private static readonly Brush _LightModeButtonMouseOver = UIModification.MakeBrush("#BEE6FD");
				[JsonIgnore]
				private static readonly Style _LightModeButtonStyle = UIModification.MakeButtonStyle(
					_LightModeButtonBackground,
					_LightModeForeground,
					_LightModeButtonBorder,
					_LightModeButtonDisabledBackground,
					_LightModeButtonDisabledForeground,
					_LightModeButtonDisabledBorder,
					_LightModeButtonMouseOver
					);

				[JsonIgnore]
				private static readonly Brush _DarkModeBackground = UIModification.MakeBrush("#1C1C1C");
				[JsonIgnore]
				private static readonly Brush _DarkModeForeground = UIModification.MakeBrush("#E1E1E1");
				[JsonIgnore]
				private static readonly Brush _DarkModeBorder = UIModification.MakeBrush("#ABADB3");
				[JsonIgnore]
				private static readonly Brush _DarkModeButtonBackground = UIModification.MakeBrush("#151515");
				[JsonIgnore]
				private static readonly Brush _DarkModeButtonBorder = UIModification.MakeBrush("#ABADB3");
				[JsonIgnore]
				private static readonly Brush _DarkModeButtonDisabledBackground = UIModification.MakeBrush("#343434");
				[JsonIgnore]
				private static readonly Brush _DarkModeButtonDisabledForeground = UIModification.MakeBrush("#A0A0A0");
				[JsonIgnore]
				private static readonly Brush _DarkModeButtonDisabledBorder = UIModification.MakeBrush("#ADB2B5");
				[JsonIgnore]
				private static readonly Brush _DarkModeButtonMouseOver = UIModification.MakeBrush("#303333");
				[JsonIgnore]
				private static readonly Style _DarkModeButtonStyle = UIModification.MakeButtonStyle(
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
					SavingAndLoadingActions.OverWriteFile(GetActions.GetBaseBotDirectory(Constants.UI_INFO_LOCATION), SavingAndLoadingActions.Serialize(this));
				}
				public void InitializeColors()
				{
					var res = Application.Current.Resources;
					res.Add(ColorTarget.Base_Background, _LightModeBackground);
					res.Add(ColorTarget.Base_Foreground, _LightModeForeground);
					res.Add(ColorTarget.Base_Border, _LightModeBorder);
					res.Add(ColorTarget.Button_Background, _LightModeButtonBackground);
					res.Add(ColorTarget.Button_Border, _LightModeButtonBorder);
					res.Add(ColorTarget.Button_Disabled_Background, _LightModeButtonDisabledBackground);
					res.Add(ColorTarget.Button_Disabled_Foreground, _LightModeButtonDisabledForeground);
					res.Add(ColorTarget.Button_Disabled_Border, _LightModeButtonDisabledBorder);
					res.Add(ColorTarget.Button_Mouse_Over_Background, _LightModeButtonMouseOver);
					res.Add(OtherTarget.Button_Style, _LightModeButtonStyle);
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
					var UISettings = new UISettings();
					var path = GetActions.GetBaseBotDirectory(Constants.UI_INFO_LOCATION);
					if (!File.Exists(path))
					{
						if (loaded)
						{
							ConsoleActions.WriteLine("The bot UI information file does not exist.");
						}
						return UISettings;
					}

					try
					{
						using (var reader = new StreamReader(path))
						{
							UISettings = JsonConvert.DeserializeObject<UISettings>(reader.ReadToEnd());
						}
					}
					catch (Exception e)
					{
						ConsoleActions.ExceptionToConsole(e);
					}
					return UISettings;
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

	}
}
