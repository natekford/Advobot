using Advobot.Actions;
using Advobot.Graphics.Colors;
using Advobot.Interfaces;
using Discord;
using ICSharpCode.AvalonEdit;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;

namespace Advobot.Graphics
{
	internal class UIModification
	{
		private static CancellationTokenSource _ToolTipCancellationTokenSource;

		public static void AddRows(Grid grid, int amount)
		{
			for (int i = 0; i < amount; ++i)
			{
				grid.RowDefinitions.Add(new RowDefinition());
			}
		}
		public static void AddCols(Grid grid, int amount)
		{
			for (int i = 0; i < amount; ++i)
			{
				grid.ColumnDefinitions.Add(new ColumnDefinition());
			}
		}
		public static void SetRowAndSpan(UIElement item, int start = 0, int length = 1)
		{
			Grid.SetRow(item, Math.Max(0, start));
			Grid.SetRowSpan(item, Math.Max(1, length));
		}
		public static void SetColAndSpan(UIElement item, int start = 0, int length = 1)
		{
			Grid.SetColumn(item, Math.Max(0, start));
			Grid.SetColumnSpan(item, Math.Max(1, length));
		}
		public static void AddElement(Grid parent, Grid child, int rowStart, int rowLength, int columnStart, int columnLength, int setRows = 0, int setColumns = 0)
		{
			AddRows(child, setRows);
			AddCols(child, setColumns);
			parent.Children.Add(child);
			SetRowAndSpan(child, rowStart, rowLength);
			SetColAndSpan(child, columnStart, columnLength);
		}
		public static void AddElement(Grid parent, UIElement child, int rowStart, int rowLength, int columnStart, int columnLength)
		{
			parent.Children.Add(child);
			SetRowAndSpan(child, rowStart, rowLength);
			SetColAndSpan(child, columnStart, columnLength);
		}
		public static void AddPlaceHolderTB(Grid parent, int rowStart, int rowLength, int columnStart, int columnLength)
		{
			AddElement(parent, new MyTextBox { IsReadOnly = true, }, rowStart, rowLength, columnStart, columnLength);
		}

		public static void SetColorMode(DependencyObject parent)
		{
			for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); ++i)
			{
				var element = VisualTreeHelper.GetChild(parent, i) as DependencyObject;
				if (element is Control)
				{
					if (element is CheckBox || element is ComboBox)
					{
						continue;
					}
					else if (element is MyButton)
					{
						SwitchElementColor((MyButton)element);
					}
					else
					{
						SwitchElementColor((Control)element);
					}
				}
				SetColorMode(element);
			}
		}
		public static void SwitchElementColor(Control element)
		{
			var eleBackground = element.Background as SolidColorBrush;
			if (eleBackground == null)
			{
				element.SetResourceReference(Control.BackgroundProperty, ColorTarget.Base_Background);
			}
			var eleForeground = element.Foreground as SolidColorBrush;
			if (eleForeground == null)
			{
				element.SetResourceReference(Control.ForegroundProperty, ColorTarget.Base_Foreground);
			}
			var eleBorder = element.BorderBrush as SolidColorBrush;
			if (eleBorder == null)
			{
				element.SetResourceReference(Control.BorderBrushProperty, ColorTarget.Base_Border);
			}
		}
		public static void SwitchElementColor(MyButton element)
		{
			var style = element.Style;
			if (style == null)
			{
				element.SetResourceReference(Button.StyleProperty, OtherTarget.Button_Style);
			}
			var eleForeground = element.Foreground as SolidColorBrush;
			if (eleForeground == null)
			{
				element.SetResourceReference(Control.ForegroundProperty, ColorTarget.Base_Foreground);
			}
		}

		public static Style MakeButtonStyle(Brush regBG, Brush regFG, Brush regB, Brush disabledBG, Brush disabledFG, Brush disabledB, Brush mouseOverBG)
		{
			//Yes, this is basically the old XAML of a button put into code.
			var templateContentPresenter = new FrameworkElementFactory
			{
				Type = typeof(ContentPresenter),
			};
			templateContentPresenter.SetValue(ContentPresenter.MarginProperty, new Thickness(2));
			templateContentPresenter.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
			templateContentPresenter.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);
			templateContentPresenter.SetValue(ContentPresenter.RecognizesAccessKeyProperty, true);

			var templateBorder = new FrameworkElementFactory
			{
				Type = typeof(Border),
				Name = "Border",
			};
			templateBorder.SetValue(Border.BorderThicknessProperty, new Thickness(1));
			templateBorder.SetValue(Border.BackgroundProperty, regBG);
			templateBorder.SetValue(Border.BorderBrushProperty, regB);
			templateBorder.AppendChild(templateContentPresenter);

			//Create the template
			var template = new ControlTemplate
			{
				TargetType = typeof(Button),
				VisualTree = templateBorder,
			};
			//Add in the triggers
			MakeButtonTriggers(regBG, regFG, regB, disabledBG, disabledFG, disabledB, mouseOverBG).ForEach(x => template.Triggers.Add(x));

			var buttonFocusRectangle = new FrameworkElementFactory
			{
				Type = typeof(System.Windows.Shapes.Rectangle),
			};
			buttonFocusRectangle.SetValue(System.Windows.Shapes.Shape.MarginProperty, new Thickness(2));
			buttonFocusRectangle.SetValue(System.Windows.Shapes.Shape.StrokeThicknessProperty, 1.0);
			buttonFocusRectangle.SetValue(System.Windows.Shapes.Shape.StrokeProperty, UIModification.MakeBrush("#60000000"));
			buttonFocusRectangle.SetValue(System.Windows.Shapes.Shape.StrokeDashArrayProperty, new DoubleCollection { 1.0, 2.0 });

			var buttonFocusBorder = new FrameworkElementFactory
			{
				Type = typeof(Border),
			};
			buttonFocusBorder.AppendChild(buttonFocusRectangle);

			var buttonFocusVisual = new Style();
			new List<Setter>
			{
				new Setter
				{
					Property = Control.TemplateProperty,
					Value = new ControlTemplate
					{
						VisualTree = buttonFocusBorder,
					}
				},
			}.ForEach(x => buttonFocusVisual.Setters.Add(x));

			//Add in the template
			var buttonStyle = new Style();
			new List<Setter>
			{
				new Setter
				{
					Property = Button.SnapsToDevicePixelsProperty,
					Value = true,
				},
				new Setter
				{
					Property = Button.OverridesDefaultStyleProperty,
					Value = true,
				},
				new Setter
				{
					Property = Button.FocusVisualStyleProperty,
					Value = buttonFocusVisual,
				},
				new Setter
				{
					Property = Button.TemplateProperty,
					Value = template,
				},
			}.ForEach(x => buttonStyle.Setters.Add(x));

			return buttonStyle;
		}
		public static List<Trigger> MakeButtonTriggers(Brush regBG, Brush regFG, Brush regB, Brush disabledBG, Brush disabledFG, Brush disabledB, Brush mouseOverBG)
		{
			//This used to have 5 triggers until I realized how useless a lot of them were.
			var isMouseOverTrigger = new Trigger
			{
				Property = Button.IsMouseOverProperty,
				Value = true,
			};
			new List<Setter>
			{
				new Setter
				{
					TargetName = "Border",
					Property = Border.BackgroundProperty,
					Value = mouseOverBG,
				},
			}.ForEach(x => isMouseOverTrigger.Setters.Add(x));

			var isEnabledTrigger = new Trigger
			{
				Property = Button.IsEnabledProperty,
				Value = false,
			};
			new List<Setter>
			{
				new Setter
				{
					TargetName = "Border",
					Property = Border.BackgroundProperty,
					Value = disabledBG,
				},
				new Setter
				{
					TargetName = "Border",
					Property = Border.BorderBrushProperty,
					Value = disabledB,
				},
				new Setter
				{
					Property = Button.ForegroundProperty,
					Value = disabledFG,
				},
			}.ForEach(x => isEnabledTrigger.Setters.Add(x));

			return new List<Trigger> { isMouseOverTrigger, isEnabledTrigger };
		}
		public static Brush MakeBrush(string color)
		{
			return (SolidColorBrush)new BrushConverter().ConvertFrom(color);
		}
		public static bool CheckIfTwoBrushesAreTheSame(Brush b1, Brush b2)
		{
			var nullableColor1 = ((SolidColorBrush)b1)?.Color;
			var nullableColor2 = ((SolidColorBrush)b2)?.Color;
			var color1IsNull = !nullableColor1.HasValue;
			var color2IsNull = !nullableColor2.HasValue;
			if (color1IsNull || color2IsNull)
			{
				return color1IsNull && color2IsNull;
			}

			var color1 = nullableColor1.Value;
			var color2 = nullableColor2.Value;

			var a = color1.A == color2.A;
			var r = color1.R == color2.R;
			var g = color1.G == color2.G;
			var b = color1.B == color2.B;
			return a && r && g && b;
		}
		public static string FormatBrush(Brush b)
		{
			var color = ((SolidColorBrush)b)?.Color;
			if (!color.HasValue)
				return "";

			var c = color.Value;
			return $"#{c.A.ToString("X2")}{c.R.ToString("X2")}{c.G.ToString("X2")}{c.B.ToString("X2")}";
		}

		public static void SetFontSizeProperties(double size, params IEnumerable<UIElement>[] elements)
		{
			foreach (var ele in elements.SelectMany(x => x))
			{
				SetFontSizeProperty(ele, size);
			}
		}
		public static void SetFontSizeProperty(UIElement element, double size)
		{
			if (element is Control)
			{
				(element as Control).SetBinding(Control.FontSizeProperty, new Binding
				{
					Path = new PropertyPath("ActualHeight"),
					RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Grid), 1),
					Converter = new UIFontResizer(size),
				});
			}
			else if (element is Grid)
			{
				foreach (var child in (element as Grid).Children.OfType<Control>())
				{
					SetFontSizeProperty(child, size);
				}
			}
		}

		public static void ToggleToolTip(ToolTip ttip)
		{
			ttip.IsOpen = !ttip.IsOpen;
		}
		public static async Task MakeFollowingToolTip(UIElement baseElement, ToolTip tt, string text, int timeInMS = 2500)
		{
			tt.Content = text ?? "Blank.";
			tt.IsOpen = true;
			baseElement.MouseMove += (sender, e) =>
			{
				var point = System.Windows.Forms.Control.MousePosition;
				tt.HorizontalOffset = point.X;
				tt.VerticalOffset = point.Y;
			};

			if (_ToolTipCancellationTokenSource != null)
			{
				_ToolTipCancellationTokenSource.Cancel();
			}
			_ToolTipCancellationTokenSource = new CancellationTokenSource();

			await baseElement.Dispatcher.InvokeAsync(async () =>
			{
				try
				{
					await Task.Delay(timeInMS, _ToolTipCancellationTokenSource.Token);
				}
				catch (TaskCanceledException)
				{
					return;
				}

				tt.IsOpen = false;
			});
		}

		public static int[][] FigureOutWhereToPutBG(Grid parent, UIElement child)
		{
			var rowTotal = parent.RowDefinitions.Count;
			var columnTotal = parent.ColumnDefinitions.Count;

			var rowStart = Grid.GetRow(child);
			var rowSpan = Grid.GetRowSpan(child);
			var columnStart = Grid.GetColumn(child);
			var columnSpan = Grid.GetColumnSpan(child);

			var start = 0;
			var temp = new int[4][];

			/* Example:
				* Row start		  0		10		 90		10
				* Row span			 10		80		 10		80
				* Column start		  0		 0		  0		90
				* Column span		100		10		100		10
				*/

			var a1p1 = start;
			var a1p2 = rowStart;
			var a1p3 = start;
			var a1p4 = columnTotal;
			temp[0] = new[] { a1p1, a1p2, a1p3, a1p4, };

			var a2p1 = rowStart;
			var a2p2 = rowSpan;
			var a2p3 = start;
			var a2p4 = columnStart;
			temp[1] = new[] { a2p1, a2p2, a2p3, a2p4, };

			var a3p1 = rowStart + rowSpan;
			var a3p2 = rowTotal - a3p1;
			var a3p3 = start;
			var a3p4 = columnTotal;
			temp[2] = new[] { a3p1, a3p2, a3p3, a3p4, };

			var a4p1 = rowStart;
			var a4p2 = rowSpan;
			var a4p3 = columnStart + columnSpan;
			var a4p4 = columnTotal - a4p3;
			temp[3] = new[] { a4p1, a4p2, a4p3, a4p4, };

			return temp;
		}
		public static void PutInBG(Grid parent, UIElement child, Brush brush)
		{
			PutInBGWithMouseUpEvent(parent, child, brush);
		}
		public static void PutInBGWithMouseUpEvent(Grid parent, UIElement child, Brush brush = null, RoutedEventHandler action = null)
		{
			//Because setting the entire layout with the MouseUp event meant the empty combobox when clicked would trigger it even when IsHitTestVisible = True. No idea why, but this is the workaround.
			var BGPoints = FigureOutWhereToPutBG(parent, child);
			for (int i = 0; i < BGPoints.GetLength(0); ++i)
			{
				var temp = new Grid { Background = brush ?? Brushes.Transparent, SnapsToDevicePixels = true, };
				if (action != null)
				{
					temp.MouseUp += (sender, e) => action(sender, e);
				}
				AddElement(parent, temp, BGPoints[i][0], BGPoints[i][1], BGPoints[i][2], BGPoints[i][3]);
			}
		}

		public static Hyperlink MakeHyperlink(string link, string name)
		{
			//Make sure the input is a valid link
			if (!UploadActions.ValidateURL(link))
			{
				ConsoleActions.WriteLine(FormattingActions.ERROR("Invalid URL."));
				return null;
			}
			//Create the hyperlink
			var hyperlink = new Hyperlink(new Run(name))
			{
				NavigateUri = new Uri(link),
				IsEnabled = true,
			};
			//Make it work when clicked
			hyperlink.RequestNavigate += (sender, e) =>
			{
				Process.Start(e.Uri.ToString());
				e.Handled = true;
			};
			return hyperlink;
		}
		public static TextBox MakeTitle(string text, string summary)
		{
			ToolTip tt = null;
			if (!String.IsNullOrWhiteSpace(summary))
			{
				tt = new ToolTip
				{
					Content = summary,
				};
			}

			var tb = new MyTextBox
			{
				Text = text,
				IsReadOnly = true,
				BorderThickness = new Thickness(0),
				VerticalAlignment = VerticalAlignment.Center,
				HorizontalAlignment = HorizontalAlignment.Left,
				TextWrapping = TextWrapping.WrapWithOverflow,
			};

			if (tt != null)
			{
				tb.MouseEnter += (sender, e) =>
				{
					ToggleToolTip(tt);
				};
				tb.MouseLeave += (sender, e) =>
				{
					ToggleToolTip(tt);
				};
			}

			return tb;
		}
		public static TextBox MakeSetting(string settingName, int length)
		{
			return new MyTextBox
			{
				VerticalContentAlignment = VerticalAlignment.Center,
				Tag = settingName,
				MaxLength = length
			};
		}
		public static TextBox MakeSysInfoBox()
		{
			return new MyTextBox
			{
				IsReadOnly = true,
				BorderThickness = new Thickness(0, .5, 0, .5),
				Background = null,
			};
		}
		public static TextBox MakeTextBoxFromUserID(IUser user)
		{
			if (user == null)
			{
				return null;
			}

			var userName = user.Username.AllCharactersAreWithinUpperLimit(Constants.MAX_UTF16_VAL_FOR_NAMES) ? user.Username : "Non-Standard Name";
			return new MyTextBox
			{
				Text = $"'{userName}#{user.Discriminator}' ({user.Id})",
				Tag = user.Id,
				IsReadOnly = true,
				IsHitTestVisible = false,
				BorderThickness = new Thickness(0),
				Background = Brushes.Transparent,
				Foreground = Brushes.Black,
			};
		}
		public static Viewbox MakeStandardViewBox(string text)
		{
			return new Viewbox
			{
				Child = new MyTextBox
				{
					Text = text,
					VerticalAlignment = VerticalAlignment.Bottom,
					IsReadOnly = true,
					BorderThickness = new Thickness(0)
				},
				HorizontalAlignment = HorizontalAlignment.Left
			};
		}
		public static TreeView MakeGuildTreeView(TreeView tv, IEnumerable<IGuild> guilds)
		{
			//Get the directory
			var directoryInfo = GetActions.GetBaseBotDirectory();
			if (directoryInfo == null || !directoryInfo.Exists)
			{
				return tv;
			}

			//Remove its parent so it can be added back to something
			var parent = tv.Parent;
			if (parent != null)
			{
				(parent as InlineUIContainer).Child = null;
			}

			tv.BorderThickness = new Thickness(0);
			tv.Background = (Brush)Application.Current.Resources[ColorTarget.Base_Background];
			tv.Foreground = (Brush)Application.Current.Resources[ColorTarget.Base_Foreground];
			tv.ItemsSource = directoryInfo.GetDirectories().Select(guildDir =>
			{
				//Make sure the ID is valid
				if (!ulong.TryParse(guildDir.Name, out ulong Id))
				{
					return null;
				}
				//Make sure a guild has that Id
				var guild = guilds.FirstOrDefault(x => x.Id == Id);
				if (guild == null)
				{
					return null;
				}

				//Get all of the files
				var listOfFiles = guildDir.GetFiles().Select(fileInfo =>
				{
					var fileType = UIBotWindowLogic.GetFileType(Path.GetFileNameWithoutExtension(fileInfo.Name));
					if (!fileType.HasValue)
					{
						return null;
					}

					return new TreeViewItem
					{
						Header = fileInfo.Name,
						Tag = new FileInformation(fileType.Value, fileInfo),
						Background = (Brush)Application.Current.Resources[ColorTarget.Base_Background],
						Foreground = (Brush)Application.Current.Resources[ColorTarget.Base_Foreground],
					};
				}).Where(x => x != null);

				//If no items then don't bother adding in the guild to the treeview
				if (!listOfFiles.Any())
				{
					return null;
				}

				//Create the guild item
				return new TreeViewItem
				{
					Header = guild.FormatGuild(),
					Tag = new GuildFileInformation(Id, guild.Name, (guild as Discord.WebSocket.SocketGuild).MemberCount),
					Background = (Brush)Application.Current.Resources[ColorTarget.Base_Background],
					Foreground = (Brush)Application.Current.Resources[ColorTarget.Base_Foreground],
					ItemsSource = listOfFiles,
				};
			}).Where(x => x != null).OrderByDescending(x => ((GuildFileInformation)x.Tag).MemberCount);

			return tv;
		}
		public static FlowDocument MakeMainMenu()
		{
			var defs1 = "Latency:\n\tTime it takes for a command to reach the bot.\nMemory:\n\tAmount of RAM the program is using.\n\t(This is wrong most of the time.)";
			var defs2 = "Threads:\n\tWhere all the actions in the bot happen.\nShards:\n\tHold all the guilds a bot has on its client.\n\tThere is a limit of 2500 guilds per shard.";
			var vers = $"\nAPI Wrapper Version: {Constants.API_VERSION}\nBot Version: {Constants.BOT_VERSION}\nGitHub Repository: ";

			var temp = new Paragraph();
			temp.Inlines.Add(new Run(String.Join("\n", defs1, defs2, vers)));
			temp.Inlines.Add(MakeHyperlink(Constants.REPO, "Advobot"));
			temp.Inlines.Add(new Run("\n\nNeed additional help? Join the Discord server: "));
			temp.Inlines.Add(MakeHyperlink(Constants.DISCORD_INV, "Here"));

			return new FlowDocument(temp);
		}
		public static FlowDocument MakeInfoMenu(string botUptime, string formattedLoggedCommands, string formattedLoggedThings)
		{
			var uptime = $"Uptime: {botUptime}";
			var cmds = $"Logged Commands:\n{formattedLoggedCommands}";
			var logs = $"Logged Actions:\n{formattedLoggedThings}";
			return new FlowDocument(new Paragraph(new Run(String.Join("\r\r", new[] { uptime, cmds, logs }).RemoveAllMarkdown().RemoveDuplicateNewLines())) { TextAlignment = TextAlignment.Center, });
		}
		public static Grid MakeColorDisplayer(UISettings UISettings, Grid child, Button button, double fontSizeProperty)
		{
			child.Children.Clear();
			AddPlaceHolderTB(child, 0, 100, 0, 100);

			var themeTitle = MakeTitle("Themes:", "");
			SetFontSizeProperty(themeTitle, fontSizeProperty);
			AddElement(child, themeTitle, 2, 5, 10, 55);

			var themeComboBox = new MyComboBox
			{
				VerticalContentAlignment = VerticalAlignment.Center,
				ItemsSource = MakeComboBoxSourceOutOfEnum(typeof(ColorTheme)),
			};
			themeComboBox.SelectedItem = themeComboBox.Items.Cast<TextBox>().FirstOrDefault(x => (ColorTheme)x.Tag == UISettings.Theme);
			AddElement(child, themeComboBox, 2, 5, 65, 25);

			var colorResourceKeys = Enum.GetValues(typeof(ColorTarget)).Cast<ColorTarget>().ToArray();
			for (int i = 0; i < colorResourceKeys.Length; ++i)
			{
				var key = colorResourceKeys[i];
				var value = FormatBrush(UISettings.ColorTargets[key]);

				var title = MakeTitle($"{key.EnumName()}:", "");
				var setting = new MyTextBox
				{
					VerticalContentAlignment = VerticalAlignment.Center,
					Tag = key,
					MaxLength = 10,
					Text = value,
				};
				AddElement(child, title, i * 5 + 7, 5, 10, 55);
				AddElement(child, setting, i * 5 + 7, 5, 65, 25);
				SetFontSizeProperties(fontSizeProperty, new[] { title, setting });
			}

			AddElement(child, button, 95, 5, 0, 100);
			SetColorMode(child);

			return child;
		}
		public static IEnumerable<TextBox> MakeComboBoxSourceOutOfEnum(Type type)
		{
			return Enum.GetValues(type).Cast<object>().Select(x =>
			{
				return new MyTextBox
				{
					Text = Enum.GetName(type, x),
					Tag = x,
					IsReadOnly = true,
					IsHitTestVisible = false,
					BorderThickness = new Thickness(0),
					Background = Brushes.Transparent,
					Foreground = Brushes.Black,
				};
			});
		}
		public static IEnumerable<TextBox> MakeComboBoxSourceOutOfStrings(IEnumerable<string> strings)
		{
			return strings.Select(x =>
			{
				return new MyTextBox
				{
					Text = x,
					Tag = x,
					IsReadOnly = true,
					IsHitTestVisible = false,
					BorderThickness = new Thickness(0),
					Background = Brushes.Transparent,
					Foreground = Brushes.Black,
				};
			});
		}
	}

	internal class UIBotWindowLogic
	{
		private static Dictionary<ToolTipReason, string> _ToolTipReasons = new Dictionary<ToolTipReason, string>
		{
			{ ToolTipReason.FileSavingFailure, "Failed to save the file." },
			{ ToolTipReason.FileSavingSuccess, "Successfully saved the file." },
			{ ToolTipReason.InvalidFilePath, "Unable to gather the path for this file." },
		};

		public static async Task SaveSettings(Grid parent, IDiscordClient client, IBotSettings botSettings)
		{
			//Go through each setting and update them
			for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); ++i)
			{
				var ele = VisualTreeHelper.GetChild(parent, i);
				var settingName = (ele as FrameworkElement)?.Tag;
				if (settingName is string && !SaveSetting(ele, settingName.ToString(), botSettings))
				{
					ConsoleActions.WriteLine($"Failed to save: {settingName.ToString()}");
				}
			}

			await ClientActions.UpdateGame(client, botSettings);
		}
		private static bool SaveSetting(object obj, string settingName, IBotSettings botSettings)
		{
			if (obj is Grid)
			{
				return SaveSetting(obj as Grid, settingName, botSettings);
			}
			else if (obj is TextBox)
			{
				return SaveSetting(obj as TextBox, settingName, botSettings);
			}
			else if (obj is Viewbox)
			{
				return SaveSetting(obj as Viewbox, settingName, botSettings);
			}
			else if (obj is CheckBox)
			{
				return SaveSetting(obj as CheckBox, settingName, botSettings);
			}
			else if (obj is ComboBox)
			{
				return SaveSetting(obj as ComboBox, settingName, botSettings);
			}
			else
			{
				throw new ArgumentException("Invalid object provided when attempting to save settings.");
			}
		}
		private static bool SaveSetting(Grid g, string settingName, IBotSettings botSettings)
		{
			var children = g.Children;
			foreach (var child in children)
			{
				return SaveSetting(child, settingName, botSettings);
			}
			return true;
		}
		private static bool SaveSetting(TextBox tb, string settingName, IBotSettings botSettings)
		{
			if (tb.IsReadOnly)
			{
				return true;
			}

			var text = tb.Text;
			switch (settingName)
			{
				case nameof(IBotSettings.Prefix):
				{
					if (String.IsNullOrWhiteSpace(text))
					{
						return false;
					}
					else if (botSettings.Prefix != text)
					{
						botSettings.Prefix = text;
					}
					return true;
				}
				case nameof(IBotSettings.Game):
				{
					if (botSettings.Game != text)
					{
						botSettings.Game = text;
					}
					return true;
				}
				case nameof(IBotSettings.Stream):
				{
					if (!MiscActions.MakeSureInputIsValidTwitchAccountName(text))
					{
						return false;
					}
					else if (botSettings.Stream != text)
					{
						botSettings.Stream = text;
					}
					return true;
				}
				case nameof(IBotSettings.ShardCount):
				{
					if (!uint.TryParse(text, out uint num))
					{
						return false;
					}
					else if (botSettings.ShardCount != num)
					{
						botSettings.ShardCount = num;
					}
					return true;
				}
				case nameof(IBotSettings.MessageCacheCount):
				{
					if (!uint.TryParse(text, out uint num))
					{
						return false;
					}
					else if (botSettings.MessageCacheCount != num)
					{
						botSettings.MessageCacheCount = num;
					}
					return true;
				}
				case nameof(IBotSettings.MaxUserGatherCount):
				{
					if (!uint.TryParse(text, out uint num))
					{
						return false;
					}
					else if (botSettings.MaxUserGatherCount != num)
					{
						botSettings.MaxUserGatherCount = num;
					}
					return true;
				}
				case nameof(IBotSettings.MaxMessageGatherSize):
				{
					if (!uint.TryParse(text, out uint num))
					{
						return false;
					}
					else if (botSettings.MaxMessageGatherSize != num)
					{
						botSettings.MaxMessageGatherSize = num;
					}
					return true;
				}
				case nameof(IBotSettings.TrustedUsers):
				{
					return true;
				}
				default:
				{
					throw new ArgumentException($"Invalid object provided when attempting to save settings for a {tb.GetType().Name}.");
				}
			}
		}
		private static bool SaveSetting(Viewbox vb, string settingName, IBotSettings botSettings)
		{
			return SaveSetting(vb.Child, settingName, botSettings);
		}
		private static bool SaveSetting(CheckBox cb, string settingName, IBotSettings botSettings)
		{
			var isChecked = cb.IsChecked.Value;
			switch (settingName)
			{
				case nameof(IBotSettings.AlwaysDownloadUsers):
				{
					if (botSettings.AlwaysDownloadUsers != isChecked)
					{
						botSettings.AlwaysDownloadUsers = isChecked;
					}
					return true;
				}
				default:
				{
					throw new ArgumentException($"Invalid object provided when attempting to save settings for a {cb.GetType().Name}.");
				}
			}
		}
		private static bool SaveSetting(ComboBox cb, string settingName, IBotSettings botSettings)
		{
			switch (settingName)
			{
				case nameof(IBotSettings.LogLevel):
				{
					var selectedLogLevel = (LogSeverity)(cb.SelectedItem as TextBox).Tag;
					if (botSettings.LogLevel != selectedLogLevel)
					{
						botSettings.LogLevel = selectedLogLevel;
					}
					return true;
				}
				case nameof(IBotSettings.TrustedUsers):
				{
					var updatedTrustedUsers = cb.Items.OfType<TextBox>().Select(x => (ulong)x.Tag).ToList();
					var removedUsers = botSettings.TrustedUsers.Except(updatedTrustedUsers);
					var addedUsers = updatedTrustedUsers.Except(botSettings.TrustedUsers);
					if (removedUsers.Any() || addedUsers.Any())
					{
						botSettings.TrustedUsers = updatedTrustedUsers;
					}
					return true;
				}
				default:
				{
					throw new ArgumentException($"Invalid object provided when attempting to save settings for a {cb.GetType().Name}.");
				}
			}
		}

		public static string GetReasonTextFromToolTipReason(ToolTipReason reason)
		{
			return _ToolTipReasons[reason];
		}
		public static ToolTipReason SaveFile(TextEditor tb)
		{
			var fileInfo = ((FileInformation)tb.Tag).FileInfo;
			if (fileInfo == null || !fileInfo.Exists)
			{
				return ToolTipReason.InvalidFilePath;
			}
			else if (fileInfo.Name.Equals(Constants.GUILD_SETTINGS_LOCATION))
			{
				//Make sure the guild info stays valid
				try
				{
					var throwaway = JsonConvert.DeserializeObject(tb.Text, Constants.GUILDS_SETTINGS_TYPE);
				}
				catch (Exception exc)
				{
					ConsoleActions.ExceptionToConsole(exc);
					return ToolTipReason.FileSavingFailure;
				}
			}

			try
			{
				SavingAndLoadingActions.OverWriteFile(fileInfo, tb.Text);
				return ToolTipReason.FileSavingSuccess;
			}
			catch
			{
				return ToolTipReason.FileSavingFailure;
			}
		}
		public static ToolTipReason SaveOutput(TextBox tb)
		{
			var fileInfo = GetActions.GetBaseBotDirectoryFile("Output_Log_" + FormattingActions.FormatDateTimeForSaving() + Constants.GENERAL_FILE_EXTENSION);
			try
			{
				SavingAndLoadingActions.OverWriteFile(fileInfo, tb.Text);
				return ToolTipReason.FileSavingSuccess;
			}
			catch
			{
				return ToolTipReason.FileSavingFailure;
			}
		}

		public static FileType? GetFileType(string file)
		{
			return Enum.TryParse(file, true, out FileType type) ? type as FileType? : null;
		}

		public static void PauseBot(IBotSettings botSettings)
		{
			if (botSettings.Pause)
			{
				ConsoleActions.WriteLine("The bot is now unpaused.");
				botSettings.TogglePause();
			}
			else
			{
				ConsoleActions.WriteLine("The bot is now paused.");
				botSettings.TogglePause();
			}
		}

		public static bool AppendTextToTextEditorIfPathExists(TextEditor display, TreeViewItem treeItem)
		{
			var fileInfo = ((FileInformation)treeItem.Tag).FileInfo;
			if (fileInfo != null && fileInfo.Exists)
			{
				display.Clear();
				display.Tag = fileInfo;
				using (var reader = new StreamReader(fileInfo.FullName))
				{
					display.AppendText(reader.ReadToEnd());
				}
				return true;
			}
			else
			{
				ConsoleActions.WriteLine("Unable to bring up the file.");
				return false;
			}
		}
		public static async Task DoStuffWithInput(string input, IDiscordClient client, IBotSettings botSettings)
		{
			//Make sure both the path and key are set
			if (!botSettings.GotPath || !botSettings.GotKey)
			{
				if (!botSettings.GotPath)
				{
					if (SavingAndLoadingActions.ValidatePath(input, botSettings.Windows))
					{
						botSettings.SetGotPath();
					}
				}
				else if (!botSettings.GotKey)
				{
					if (await ClientActions.ValidateBotKey(client, input))
					{
						botSettings.SetGotKey();
					}
				}
				await ClientActions.MaybeStartBot(client, botSettings);
			}
			else
			{
				UICommandHandler.HandleCommand(input, botSettings.Prefix);
			}
		}

		public static async Task AddTrustedUserToComboBox(ComboBox cb, IDiscordClient client, string input)
		{
			if (String.IsNullOrWhiteSpace(input))
			{
				return;
			}
			else if (ulong.TryParse(input, out ulong userID))
			{
				var currTBs = cb.Items.Cast<TextBox>().ToList();
				if (currTBs.Any(x => (ulong)x.Tag == userID))
					return;

				var tb = UIModification.MakeTextBoxFromUserID(await client.GetUserAsync(userID));
				if (tb != null)
				{
					currTBs.Add(tb);
					cb.ItemsSource = currTBs;
				}
			}
			else
			{
				ConsoleActions.WriteLine($"The given input '{input}' is not a valid ID.");
			}
		}
		public static void RemoveTrustedUserFromComboBox(ComboBox cb)
		{
			if (cb.SelectedItem == null)
				return;

			cb.ItemsSource = cb.Items.Cast<TextBox>().Where(x => (ulong)x.Tag != (ulong)((TextBox)cb.SelectedItem).Tag).ToList();
		}
	}

	internal class UICommandHandler
	{
		public static string GatherInput(TextBox tb, Button b)
		{
			var text = tb.Text.Trim(new[] { '\r', '\n' });
			if (text.Contains("﷽"))
			{
				text += "This program really doesn't like that long Arabic character for some reason. Whenever there are a lot of them it crashes the program completely.";
			}

			ConsoleActions.WriteLine(text);

			tb.Text = "";
			b.IsEnabled = false;

			return text;
		}
		public static void HandleCommand(string input, string prefix)
		{
			if (input.CaseInsStartsWith(prefix))
			{
				var inputArray = input.Substring(prefix.Length)?.Split(new[] { ' ' }, 2);
				if (!FindCommand(inputArray[0], inputArray.Length > 1 ? inputArray[1] : null))
				{
					ConsoleActions.WriteLine("No command could be found with that name.");
				}
			}
		}
		public static bool FindCommand(string cmd, string args)
		{
			//Find what command it belongs to
			if ("test".CaseInsEquals(cmd))
			{
				UITest();
			}
			else
			{
				return false;
			}
			return true;
		}
		public static void UITest()
		{
#if DEBUG
			var resetInfo = false;
			if (resetInfo)
			{
				ClientActions.ResetSettingsSavedInPropertiesSettings();
				ClientActions.DisconnectBot();
			}
#endif
		}
	}
}
