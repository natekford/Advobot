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
				private static readonly Brush LightModeBackground = UIModification.MakeBrush("#FFFFFF");
				[JsonIgnore]
				private static readonly Brush LightModeForeground = UIModification.MakeBrush("#000000");
				[JsonIgnore]
				private static readonly Brush LightModeBorder = UIModification.MakeBrush("#ABADB3");
				[JsonIgnore]
				private static readonly Brush LightModeButtonBackground = UIModification.MakeBrush("#DDDDDD");
				[JsonIgnore]
				private static readonly Brush LightModeButtonBorder = UIModification.MakeBrush("#707070");
				[JsonIgnore]
				private static readonly Brush LightModeButtonDisabledBackground = UIModification.MakeBrush("#F4F4F4");
				[JsonIgnore]
				private static readonly Brush LightModeButtonDisabledForeground = UIModification.MakeBrush("#888888");
				[JsonIgnore]
				private static readonly Brush LightModeButtonDisabledBorder = UIModification.MakeBrush("#ADB2B5");
				[JsonIgnore]
				private static readonly Brush LightModeButtonMouseOver = UIModification.MakeBrush("#BEE6FD");
				[JsonIgnore]
				private static readonly Style LightModeButtonStyle = UIModification.MakeButtonStyle
					(
					LightModeButtonBackground,
					LightModeForeground,
					LightModeButtonBorder,
					LightModeButtonDisabledBackground,
					LightModeButtonDisabledForeground,
					LightModeButtonDisabledBorder,
					LightModeButtonMouseOver
					);

				[JsonIgnore]
				private static readonly Brush DarkModeBackground = UIModification.MakeBrush("#1C1C1C");
				[JsonIgnore]
				private static readonly Brush DarkModeForeground = UIModification.MakeBrush("#E1E1E1");
				[JsonIgnore]
				private static readonly Brush DarkModeBorder = UIModification.MakeBrush("#ABADB3");
				[JsonIgnore]
				private static readonly Brush DarkModeButtonBackground = UIModification.MakeBrush("#151515");
				[JsonIgnore]
				private static readonly Brush DarkModeButtonBorder = UIModification.MakeBrush("#ABADB3");
				[JsonIgnore]
				private static readonly Brush DarkModeButtonDisabledBackground = UIModification.MakeBrush("#343434");
				[JsonIgnore]
				private static readonly Brush DarkModeButtonDisabledForeground = UIModification.MakeBrush("#A0A0A0");
				[JsonIgnore]
				private static readonly Brush DarkModeButtonDisabledBorder = UIModification.MakeBrush("#ADB2B5");
				[JsonIgnore]
				private static readonly Brush DarkModeButtonMouseOver = UIModification.MakeBrush("#303333");
				[JsonIgnore]
				private static readonly Style DarkModeButtonStyle = UIModification.MakeButtonStyle
					(
					DarkModeButtonBackground,
					DarkModeForeground,
					DarkModeButtonBorder,
					DarkModeButtonDisabledBackground,
					DarkModeButtonDisabledForeground,
					DarkModeButtonDisabledBorder,
					DarkModeButtonMouseOver
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
					res.Add(ColorTarget.Base_Background, LightModeBackground);
					res.Add(ColorTarget.Base_Foreground, LightModeForeground);
					res.Add(ColorTarget.Base_Border, LightModeBorder);
					res.Add(ColorTarget.Button_Background, LightModeButtonBackground);
					res.Add(ColorTarget.Button_Border, LightModeButtonBorder);
					res.Add(ColorTarget.Button_Disabled_Background, LightModeButtonDisabledBackground);
					res.Add(ColorTarget.Button_Disabled_Foreground, LightModeButtonDisabledForeground);
					res.Add(ColorTarget.Button_Disabled_Border, LightModeButtonDisabledBorder);
					res.Add(ColorTarget.Button_Mouse_Over_Background, LightModeButtonMouseOver);
					res.Add(OtherTarget.Button_Style, LightModeButtonStyle);
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
					res[ColorTarget.Base_Background] = LightModeBackground;
					res[ColorTarget.Base_Foreground] = LightModeForeground;
					res[ColorTarget.Base_Border] = LightModeBorder;
					res[ColorTarget.Button_Background] = LightModeButtonBackground;
					res[ColorTarget.Button_Border] = LightModeButtonBorder;
					res[ColorTarget.Button_Disabled_Background] = LightModeButtonDisabledBackground;
					res[ColorTarget.Button_Disabled_Foreground] = LightModeButtonDisabledForeground;
					res[ColorTarget.Button_Disabled_Border] = LightModeButtonDisabledBorder;
					res[ColorTarget.Button_Mouse_Over_Background] = LightModeButtonMouseOver;
					res[OtherTarget.Button_Style] = LightModeButtonStyle;
				}
				private void ActivateDarkMode()
				{
					var res = Application.Current.Resources;
					res[ColorTarget.Base_Background] = DarkModeBackground;
					res[ColorTarget.Base_Foreground] = DarkModeForeground;
					res[ColorTarget.Base_Border] = DarkModeBorder;
					res[ColorTarget.Button_Background] = DarkModeButtonBackground;
					res[ColorTarget.Button_Border] = DarkModeButtonBorder;
					res[ColorTarget.Button_Disabled_Background] = DarkModeButtonDisabledBackground;
					res[ColorTarget.Button_Disabled_Foreground] = DarkModeButtonDisabledForeground;
					res[ColorTarget.Button_Disabled_Border] = DarkModeButtonDisabledBorder;
					res[ColorTarget.Button_Mouse_Over_Background] = DarkModeButtonMouseOver;
					res[OtherTarget.Button_Style] = DarkModeButtonStyle;
				}
				private void ActivateUserMade()
				{
					var res = Application.Current.Resources;
					foreach (var kvp in ColorTargets)
					{
						res[kvp.Key] = kvp.Value;
					}
					res[OtherTarget.Button_Style] = UIModification.MakeButtonStyle
						(
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
