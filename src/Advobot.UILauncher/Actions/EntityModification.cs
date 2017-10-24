using Advobot.Core.Actions;
using Advobot.Core.Interfaces;
using Advobot.UILauncher.Enums;
using Advobot.UILauncher.Classes;
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
using System.Reflection;
using Advobot.Core.Actions.Formatting;
using System.Text;
using Advobot.Core.Classes;
using System.Windows.Input;
using Discord.WebSocket;
using Advobot.UILauncher.Interfaces;
using Advobot.Core;

namespace Advobot.UILauncher.Actions
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
			AddElement(parent, new AdvobotTextBox { IsReadOnly = true, }, rowStart, rowLength, columnStart, columnLength);
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
		public static void PutInBGWithMouseUpEvent(Grid parent, UIElement child, Brush brush = null, MouseButtonEventHandler handler = null)
		{
			//Because setting the entire layout with the MouseUp event meant the empty combobox when clicked would trigger it even when IsHitTestVisible = True. No idea why, but this is the workaround.
			var BGPoints = FigureOutWhereToPutBG(parent, child);
			for (int i = 0; i < BGPoints[0].Length; ++i)
			{
				var temp = new Grid { Background = brush ?? Brushes.Transparent, SnapsToDevicePixels = true, };
				if (handler != null)
				{
					temp.MouseUp += handler;
				}
				AddElement(parent, temp, BGPoints[i][0], BGPoints[i][1], BGPoints[i][2], BGPoints[i][3]);
			}
		}

		public static void SetFontResizeProperty(Control control, double size)
		{
			if (!GetTopMostParent(control, out Grid parent, out int ancestorLevel))
			{
				throw new ArgumentException($"{control.Name} must be inside a grid if {nameof(IFontResizeValue.FontResizeValue)} is set.");
			}

			control.SetBinding(Control.FontSizeProperty, new Binding
			{
				Path = new PropertyPath("ActualHeight"),
				RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Grid), ancestorLevel),
				Converter = new FontResizer(size),
			});
		}
		/// <summary>
		/// Returns true if the supplied type is any parent of the supplied element.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="element"></param>
		/// <param name="parent"></param>
		/// <param name="ancestorLevel"></param>
		/// <returns></returns>
		public static bool GetTopMostParent<T>(FrameworkElement element, out T parent, out int ancestorLevel) where T : FrameworkElement
		{
			parent = default;
			ancestorLevel = 0;

			var currLevel = 0;
			while (element.Parent != null)
			{
				++currLevel;
				if (element.Parent is T tParent)
				{
					parent = tParent;
					ancestorLevel = currLevel;
				}

				if (!(element.Parent is FrameworkElement p))
				{
					//If grid is in weird object then just use the inside grid and
					//don't bother going higher
					break;
				}
				element = p;
			}
			return ancestorLevel > 0;
		}

		public static Hyperlink MakeHyperlink(string link, string name)
		{
			//Make sure the input is a valid link
			if (!GetActions.GetIfStringIsValidUrl(link))
			{
				ConsoleActions.WriteLine(new ErrorReason("Invalid URL.").ToString());
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
		public static Viewbox MakeStandardViewBox(string text)
		{
			return new Viewbox
			{
				Child = new AdvobotTextBox
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
			var directoryInfo = GetActions.GetBaseBotDirectory();
			if (directoryInfo == null || !directoryInfo.Exists)
			{
				return tv;
			}
			//Remove this treeview from its parent if it has one
			else if (tv.Parent != null && tv.Parent is InlineUIContainer parent)
			{
				parent.Child = null;
			}

			tv.BorderThickness = new Thickness(0);
			tv.Background = (Brush)Application.Current.Resources[ColorTarget.BaseBackground];
			tv.Foreground = (Brush)Application.Current.Resources[ColorTarget.BaseForeground];
			tv.ItemsSource = directoryInfo.GetDirectories().Select(dir =>
			{
				//Make sure the id leads to a valid non null guild
				if (!ulong.TryParse(dir.Name, out ulong Id) || !(guilds.FirstOrDefault(x => x.Id == Id) is SocketGuild guild))
				{
					return null;
				}

				var items = dir.GetFiles().Select(file =>
				{
					var fileType = UIBotWindowLogic.GetFileType(Path.GetFileNameWithoutExtension(file.Name));
					return fileType == default ? null : new TreeViewItem
					{
						Header = file.Name,
						Tag = new FileInformation(fileType, file),
						Background = (Brush)Application.Current.Resources[ColorTarget.BaseBackground],
						Foreground = (Brush)Application.Current.Resources[ColorTarget.BaseForeground],
					};
				}).Where(x => x != null);

				return !items.Any() ? null : new TreeViewItem
				{
					Header = guild.FormatGuild(),
					Tag = new GuildFileInformation(guild.Id, guild.Name, guild.MemberCount),
					Background = (Brush)Application.Current.Resources[ColorTarget.BaseBackground],
					Foreground = (Brush)Application.Current.Resources[ColorTarget.BaseForeground],
					ItemsSource = items,
				};
			})
			.Where(x => x != null)
			.OrderByDescending(x => x.Tag is GuildFileInformation gfi ? gfi.MemberCount : 0);

			return tv;
		}
		public static FlowDocument MakeMainMenu()
		{
			var assemblies = AppDomain.CurrentDomain.GetAssemblies();
			var coreAssembly = assemblies.FirstOrDefault(x => x.GetTypes().Contains(typeof(IAdvobotCommandContext)));
			var coreAssemblyVersion = coreAssembly.GetCustomAttribute<AssemblyFileVersionAttribute>().Version;
			var text =
				"Latency:\n\tTime it takes for a command to reach the bot.\n" +
				"Memory:\n\tAmount of RAM the program is using.\n\t(This is wrong most of the time.)\n" +
				"Threads:\n\tWhere all the actions in the bot happen.\n" +
				"Shards:\n\tHold all the guilds a bot has on its client.\n\tThere is a limit of 2500 guilds per shard.\n\n" +
				$"API Wrapper Version: {Constants.API_VERSION}\n" +
				$"Bot Version: {coreAssemblyVersion}\n" +
				$"GitHub Repository: ";

			var temp = new Paragraph();
			temp.Inlines.Add(new Run(text));
			temp.Inlines.Add(MakeHyperlink(Constants.REPO, "Advobot"));
			temp.Inlines.Add(new Run("\n\nNeed additional help? Join the Discord server: "));
			temp.Inlines.Add(MakeHyperlink(Constants.DISCORD_INV, "Here"));

			return new FlowDocument(temp);
		}
		public static FlowDocument MakeInfoMenu(ILogService logService)
		{
			var text = new StringBuilder()
				.AppendLine($"Uptime: {TimeFormatting.FormatUptime()}")
				.AppendLine($"Logged Commands:\n{logService.FormatLoggedCommands(true)}")
				.AppendLine($"Logged User Actions:\n{logService.FormatLoggedUserActions(true)}")
				.AppendLine($"Logged Message Actions:\n{logService.FormatLoggedMessageActions(true)}")
				.ToString().RemoveAllMarkdown().RemoveDuplicateNewLines();
			return new FlowDocument(new Paragraph(new Run(text)) { TextAlignment = TextAlignment.Center, });
		}
		public static Grid MakeColorDisplayer(ColorSettings UISettings, Grid g, Button b, double fontResize)
		{
			g.Children.Clear();

			var title = AdvobotTextBox.CreateTitleBox("Themes:", "");
			title.FontResizeValue = fontResize;
			var combo = AdvobotComboBox.CreateEnumComboBox<ColorTheme>(null);
			combo.SelectedItem = combo.Items.OfType<TextBox>().FirstOrDefault(x => x?.Tag is ColorTheme t && t == UISettings.Theme);

			var c = 0;
			foreach (ColorTarget e in Enum.GetValues(typeof(ColorTarget)))
			{
				var name = AdvobotTextBox.CreateTitleBox($"{e.EnumName().Remove('_')}:", "");
				name.FontResizeValue = fontResize;

				var set = new AdvobotTextBox
				{
					VerticalContentAlignment = VerticalAlignment.Center,
					Tag = e,
					MaxLength = 10,
					Text = FormatBrush(UISettings.ColorTargets[e]),
					FontResizeValue = fontResize,
				};

				AddElement(g, name, c * 5 + 7, 5, 10, 55);
				AddElement(g, set, c * 5 + 7, 5, 65, 25);
				++c;
			}

			AddPlaceHolderTB(g, 0, 100, 0, 100);
			AddElement(g, title, 2, 5, 10, 55);
			AddElement(g, combo, 2, 5, 65, 25);
			AddElement(g, b, 95, 5, 0, 100);

			ColorSettings.SwitchElementColorOfChildren(g);

			return g;
		}

		public static bool AppendTextToTextEditorIfPathExists(TextEditor display, TreeViewItem treeItem)
		{
			if (treeItem.Tag is FileInformation fi)
			{
				var fileInfo = fi.FileInfo;
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
			}

			ConsoleActions.WriteLine("Unable to bring up the file.");
			return false;
		}
	}
}
