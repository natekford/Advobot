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
using Advobot.UILauncher.Classes.Converters;

namespace Advobot.UILauncher.Actions
{
	internal class UIModification
	{
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
		public static void SetColSpan(UIElement item, int length)
		{
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

		public static bool TryMakeBrush(string input, out SolidColorBrush brush)
		{
			var split = input.Split('/');
			if (split.Length == 3 && byte.TryParse(split[0], out var r) && byte.TryParse(split[1], out var g) && byte.TryParse(split[2], out var b))
			{
				r = Math.Min(r, (byte)255);
				g = Math.Min(g, (byte)255);
				b = Math.Min(b, (byte)255);
				brush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, r, g, b));
			}
			else
			{
				if (!input.StartsWith("#"))
				{
					input = "#" + input;
				}
				try
				{
					brush = MakeSolidColorBrush(input);
				}
				catch
				{
					brush = null;
					return false;
				}
			}
			return true;
		}
		public static SolidColorBrush MakeSolidColorBrush(string input)
		{
			return (SolidColorBrush)new BrushConverter().ConvertFrom(input);
		}
		public static bool CheckIfTwoBrushesAreTheSame(SolidColorBrush b1, SolidColorBrush b2)
		{
			return b1.Color == b2.Color && b1.Opacity == b2.Opacity;
		}

		public static void ToggleToolTip(ToolTip ttip)
		{
			ttip.IsOpen = !ttip.IsOpen;
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
				Converter = new FontResizeConverter(size),
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

		public static IEnumerable<TextBox> MakeGuildTreeViewItemsSource(IEnumerable<IGuild> guilds)
		{
			return null;
			/*
			var directoryInfo = GetActions.GetBaseBotDirectory();
			if (directoryInfo == null || !directoryInfo.Exists)
			{
				return tv;
			}

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
			.OrderByDescending(x => x.Tag is GuildFileInformation gfi ? gfi.MemberCount : 0);*/
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
