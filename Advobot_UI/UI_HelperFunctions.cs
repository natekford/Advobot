using Advobot.Actions;
using Advobot.Enums;
using Advobot.Graphics.Colors;
using Advobot.Graphics.UserInterface;
using Advobot.Interfaces;
using Advobot.Structs;
using Discord;
using ICSharpCode.AvalonEdit;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;

namespace Advobot
{
	namespace Graphics
	{
		namespace HelperFunctions
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
					return String.Format("#{0}{1}{2}{3}", c.A.ToString("X2"), c.R.ToString("X2"), c.G.ToString("X2"), c.B.ToString("X2"));
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
						catch (Exception e)
						{
							ConsoleActions.ExceptionToConsole(e);
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
				public static TextBox MakeSetting(SettingOnBot setting, int length)
				{
					return new MyTextBox
					{
						VerticalContentAlignment = VerticalAlignment.Center,
						Tag = setting,
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

					return new MyTextBox
					{
						Text = String.Format("'{0}#{1}' ({2})", (user.Username.AllCharactersAreWithinUpperLimit(Constants.MAX_UTF16_VAL_FOR_NAMES) ? user.Username : "Non-Standard Name"), user.Discriminator, user.Id),
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
					var directory = GetActions.GetBaseBotDirectory();
					if (directory == null || !Directory.Exists(directory))
						return tv;

					//Remove its parent so it can be added back to something
					var parent = tv.Parent;
					if (parent != null)
					{
						(parent as InlineUIContainer).Child = null;
					}

					tv.BorderThickness = new Thickness(0);
					tv.Background = (Brush)Application.Current.Resources[ColorTarget.Base_Background];
					tv.Foreground = (Brush)Application.Current.Resources[ColorTarget.Base_Foreground];
					tv.ItemsSource = Directory.GetDirectories(directory).Select(guildDir =>
					{
						//Separate the ID from the rest of the directory
						var strID = guildDir.Substring(guildDir.LastIndexOf('\\') + 1);
						//Make sure the ID is valid
						if (!ulong.TryParse(strID, out ulong ID))
							return null;

						var guild = guilds.FirstOrDefault(x => x.Id == ID);
						if (guild == null)
							return null;

						//Get all of the files
						var listOfFiles = new List<TreeViewItem>();
						Directory.GetFiles(guildDir).ToList().ForEach(fileLoc =>
						{
							var fileType = GetActions.GetFileType(Path.GetFileNameWithoutExtension(fileLoc));
							if (!fileType.HasValue)
								return;

							var fileItem = new TreeViewItem
							{
								Header = Path.GetFileName(fileLoc),
								Tag = new FileInformation(fileType.Value, fileLoc),
								Background = (Brush)Application.Current.Resources[ColorTarget.Base_Background],
								Foreground = (Brush)Application.Current.Resources[ColorTarget.Base_Foreground],
							};
							listOfFiles.Add(fileItem);
						});

						//If no items then don't bother adding in the guild to the treeview
						if (!listOfFiles.Any())
							return null;

						//Create the guild item
						var guildItem = new TreeViewItem
						{
							Header = guild.FormatGuild(),
							Tag = new GuildFileInformation(ID, guild.Name, (guild as Discord.WebSocket.SocketGuild).MemberCount),
							Background = (Brush)Application.Current.Resources[ColorTarget.Base_Background],
							Foreground = (Brush)Application.Current.Resources[ColorTarget.Base_Foreground],
						};
						listOfFiles.ForEach(x =>
						{
							guildItem.Items.Add(x);
						});

						return guildItem;
					}).Where(x => x != null).OrderByDescending(x => ((GuildFileInformation)x.Tag).MemberCount);

					return tv;
				}
				public static TreeView MakeDMTreeView(TreeView tv, IEnumerable<IDMChannel> dms)
				{
					//Remove its parent so it can be added back to something
					var parent = tv.Parent;
					if (parent != null)
					{
						(parent as InlineUIContainer).Child = null;
					}

					tv.BorderThickness = new Thickness(0);
					tv.Background = (Brush)Application.Current.Resources[ColorTarget.Base_Background];
					tv.Foreground = (Brush)Application.Current.Resources[ColorTarget.Base_Foreground];
					tv.ItemsSource = dms.Select(x =>
					{
						var user = x.Recipient;
						if (user == null)
							return null;

						return new TreeViewItem
						{
							Header = String.Format("'{0}#{1}' ({2})", (user.Username.AllCharactersAreWithinUpperLimit(Constants.MAX_UTF16_VAL_FOR_NAMES) ? user.Username : "Non-Standard Name"), user.Discriminator, user.Id),
							Tag = x,
							Background = (Brush)Application.Current.Resources[ColorTarget.Base_Background],
							Foreground = (Brush)Application.Current.Resources[ColorTarget.Base_Foreground],
						};
					}).Where(x => x != null);

					if (tv.ItemsSource.Cast<object>().Count() == 0)
					{
						var temp = new TreeViewItem
						{
							Header = "No DMs",
							Background = (Brush)Application.Current.Resources[ColorTarget.Base_Background],
							Foreground = (Brush)Application.Current.Resources[ColorTarget.Base_Foreground],
						};
						tv.ItemsSource = new[] { temp };
					}

					return tv;
				}
				public static FlowDocument MakeMainMenu()
				{
					var defs1 = "Latency:\n\tTime it takes for a command to reach the bot.\nMemory:\n\tAmount of RAM the program is using.\n\t(This is wrong most of the time.)";
					var defs2 = "Threads:\n\tWhere all the actions in the bot happen.\nShards:\n\tHold all the guilds a bot has on its client.\n\tThere is a limit of 2500 guilds per shard.";
					var vers = String.Format("\nAPI Wrapper Version: {0}\nBot Version: {1}\nGitHub Repository: ", Constants.API_VERSION, Constants.BOT_VERSION);
					var help = "\n\nNeed additional help? Join the Discord server: ";
					var all = String.Join("\n", defs1, defs2, vers);

					var temp = new Paragraph();
					temp.Inlines.Add(new Run(all));
					temp.Inlines.Add(MakeHyperlink(Constants.REPO, "Advobot"));
					temp.Inlines.Add(new Run(help));
					temp.Inlines.Add(MakeHyperlink(Constants.DISCORD_INV, "Here"));

					return new FlowDocument(temp);
				}
				public static FlowDocument MakeInfoMenu(string botUptime, string formattedLoggedCommands, string formattedLoggedThings)
				{
					var uptime = String.Format("Uptime: {0}", botUptime);
					var cmds = String.Format("Logged Commands:\n{0}", formattedLoggedCommands);
					var logs = String.Format("Logged Actions:\n{0}", formattedLoggedThings);
					var str = FormattingActions.RemoveMarkdownChars(String.Format("{0}\r\r{1}\r\r{2}", uptime, cmds, logs), true);
					var paragraph = new Paragraph(new Run(str))
					{
						TextAlignment = TextAlignment.Center,
					};
					return new FlowDocument(paragraph);
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

						var title = MakeTitle(String.Format("{0}:", key.EnumName()), "");
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
				public static async Task SaveSettings(Grid parent, IDiscordClient client, IBotSettings botSettings)
				{
					//Go through each setting and update them
					for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); ++i)
					{
						var ele = VisualTreeHelper.GetChild(parent, i);
						var setting = (ele as FrameworkElement)?.Tag;
						if (setting is SettingOnBot)
						{
							var fuckYouForTellingMeToPatternMatch = setting as SettingOnBot?;
							var castSetting = (SettingOnBot)fuckYouForTellingMeToPatternMatch;

							if (!SaveSetting(ele, castSetting, botSettings))
							{
								ConsoleActions.WriteLine(String.Format("Failed to save: {0}", castSetting.EnumName()));
							}
						}
					}

					await ClientActions.SetGame(client, botSettings);
				}
				private static bool SaveSetting(object obj, SettingOnBot setting, IBotSettings botSettings)
				{
					if (obj is Grid)
					{
						return SaveSetting(obj as Grid, setting, botSettings);
					}
					else if (obj is TextBox)
					{
						return SaveSetting(obj as TextBox, setting, botSettings);
					}
					else if (obj is Viewbox)
					{
						return SaveSetting(obj as Viewbox, setting, botSettings);
					}
					else if (obj is CheckBox)
					{
						return SaveSetting(obj as CheckBox, setting, botSettings);
					}
					else if (obj is ComboBox)
					{
						return SaveSetting(obj as ComboBox, setting, botSettings);
					}
					else
					{
						return true;
					}
				}
				private static bool SaveSetting(Grid g, SettingOnBot setting, IBotSettings botSettings)
				{
					var children = g.Children;
					foreach (var child in children)
					{
						return SaveSetting(child, setting, botSettings);
					}
					return true;
				}
				private static bool SaveSetting(TextBox tb, SettingOnBot setting, IBotSettings botSettings)
				{
					var text = tb.Text;
					switch (setting)
					{
						case SettingOnBot.Prefix:
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
						case SettingOnBot.BotOwnerID:
						{
							if (!ulong.TryParse(text, out ulong id))
							{
								return false;
							}
							else if (botSettings.BotOwnerId != id)
							{
								botSettings.BotOwnerId = id;
							}
							return true;
						}
						case SettingOnBot.Game:
						{
							if (botSettings.Game != text)
							{
								botSettings.Game = text;
							}
							return true;
						}
						case SettingOnBot.Stream:
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
						case SettingOnBot.ShardCount:
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
						case SettingOnBot.MessageCacheCount:
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
						case SettingOnBot.MaxUserGatherCount:
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
						case SettingOnBot.MaxMessageGatherSize:
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
						default:
						{
							return true;
						}
					}
				}
				private static bool SaveSetting(Viewbox vb, SettingOnBot setting, IBotSettings botSettings)
				{
					return SaveSetting(vb.Child, setting, botSettings);
				}
				private static bool SaveSetting(CheckBox cb, SettingOnBot setting, IBotSettings botSettings)
				{
					var isChecked = cb.IsChecked.Value;
					switch (setting)
					{
						case SettingOnBot.AlwaysDownloadUsers:
						{
							if (botSettings.AlwaysDownloadUsers != isChecked)
							{
								botSettings.AlwaysDownloadUsers = isChecked;
							}
							return true;
						}
						default:
						{
							return true;
						}
					}
				}
				private static bool SaveSetting(ComboBox cb, SettingOnBot setting, IBotSettings botSettings)
				{
					switch (setting)
					{
						case SettingOnBot.LogLevel:
						{
							var selectedLogLevel = (LogSeverity)(cb.SelectedItem as TextBox).Tag;
							if (botSettings.LogLevel != selectedLogLevel)
							{
								botSettings.LogLevel = selectedLogLevel;
							}
							return true;
						}
						case SettingOnBot.TrustedUsers:
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
							return true;
						}
					}
				}

				public static string GetReasonTextFromToolTipReason(ToolTipReason reason)
				{
					switch (reason)
					{
						case ToolTipReason.FileSavingFailure:
						{
							return "Failed to save the file.";
						}
						case ToolTipReason.FileSavingSuccess:
						{
							return "Successfully saved the file.";
						}
						case ToolTipReason.InvalidFilePath:
						{
							return "Unable to gather the path for this file.";
						}
						default:
						{
							return null;
						}
					}
				}
				public static ToolTipReason SaveFile(TextEditor tb)
				{
					var path = tb.Tag.ToString();
					if (String.IsNullOrWhiteSpace(path) || !File.Exists(path))
					{
						return ToolTipReason.InvalidFilePath;
					}

					var fileAndExtension = Path.GetFileName(path);
					if (fileAndExtension.Equals(Constants.GUILD_SETTINGS_LOCATION))
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

					using (var writer = new StreamWriter(path))
					{
						writer.WriteLine(tb.Text);
					}

					return ToolTipReason.FileSavingSuccess;
				}
				public static ToolTipReason SaveOutput(TextBox tb)
				{
					var path = GetActions.GetBaseBotDirectory("Output_Log_" + DateTime.UtcNow.ToString("MM-dd_HH-mm-ss") + Constants.GENERAL_FILE_EXTENSION);
					if (String.IsNullOrWhiteSpace(path))
					{
						return ToolTipReason.FileSavingFailure;
					}

					using (StreamWriter writer = new StreamWriter(path))
					{
						writer.Write(tb.Text);
					}

					return ToolTipReason.FileSavingSuccess;
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

				public static bool AppendTextToTextEditorIfPathExistsAndReturnIfHappened(TextEditor display, TreeViewItem treeItem)
				{
					var fileLocation = ((FileInformation)treeItem.Tag).FileLocation;
					if (fileLocation != null && File.Exists(fileLocation))
					{
						display.Clear();
						display.Tag = fileLocation;
						using (var reader = new StreamReader(fileLocation))
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
							if (await SavingAndLoadingActions.ValidateBotKey(client, input))
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
						ConsoleActions.WriteLine(String.Format("The given input '{0}' is not a valid ID.", input));
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
					var codeLen = true;
					if (codeLen)
					{
						var totalChars = 0;
						var totalLines = 0;
						foreach (var file in Directory.GetFiles(Path.GetFullPath(Path.Combine(System.Reflection.Assembly.GetExecutingAssembly().Location, @"..\..\..\"))))
						{
							if (".cs".CaseInsEquals(Path.GetExtension(file)))
							{
								totalChars += File.ReadAllText(file).Length;
								totalLines += File.ReadAllLines(file).Count();
							}
						}
						ConsoleActions.WriteLine(String.Format("Current Totals:{0}\t\t\t Chars: {1}{0}\t\t\t Lines: {2}", Environment.NewLine, totalChars, totalLines));
					}
					var resetInfo = false;
					if (resetInfo)
					{
						MiscActions.ResetSettings();
						MiscActions.DisconnectBot();
					}
#endif
				}
			}

			internal class UITextBoxStreamWriter : TextWriter
			{
				private TextBoxBase _Output;
				private bool _IgnoreNewLines;
				private string _CurrentLineText;

				public UITextBoxStreamWriter(TextBoxBase output)
				{
					_Output = output;
					_IgnoreNewLines = output is RichTextBox;
				}

				public override void Write(char value)
				{
					if (value.Equals('\n'))
					{
						Write(_CurrentLineText);
						_CurrentLineText = null;
					}
					//Done because crashes program without exception. Could not for the life of me figure out why; something in the .dlls themself.
					else if (value.Equals('﷽'))
					{
						return;
					}
					else
					{
						_CurrentLineText += value;
					}
				}
				public override void Write(string value)
				{
					if (value == null || (_IgnoreNewLines && value.Equals('\n')))
						return;

					_Output.Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new Action(() =>
					{
						_Output.AppendText(value);
					}));
				}
				public override Encoding Encoding
				{
					get { return Encoding.UTF8; }
				}
			}

			internal class UIFontResizer : IValueConverter
			{
				private double _ConvertFactor;

				public UIFontResizer(double convertFactor)
				{
					_ConvertFactor = convertFactor;
				}

				public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
				{
					var converted = (int)(System.Convert.ToInt16(value) * _ConvertFactor);
					return Math.Max(converted, -1);
				}
				public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
				{
					throw new NotImplementedException();
				}
			}
		}
	}
}
