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
	internal static class UIModification
	{
		public static void SetRowSpan(UIElement item, int length)
		{
			Grid.SetRowSpan(item, Math.Max(1, length));
		}
		public static void SetColSpan(UIElement item, int length)
		{
			Grid.SetColumnSpan(item, Math.Max(1, length));
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

		public static bool TryCreateBrush(string input, out SolidColorBrush brush)
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
					brush = CreateBrush(input);
				}
				catch
				{
					brush = null;
					return false;
				}
			}
			return true;
		}
		public static SolidColorBrush CreateBrush(string input)
		{
			return (SolidColorBrush)new BrushConverter().ConvertFrom(input);
		}
		public static bool CheckIfSameBrush(SolidColorBrush b1, SolidColorBrush b2)
		{
			if (b1 == null)
			{
				return b2 == null;
			}
			else if (b2 == null)
			{
				return false;
			}

			return b1.Color == b2.Color && b1.Opacity == b2.Opacity;
		}

		/// <summary>
		/// Returns true if the supplied type is any parent of the supplied element.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="element"></param>
		/// <param name="parent"></param>
		/// <param name="ancestorLevel"></param>
		/// <returns></returns>
		public static bool GetTopMostParent<T>(this DependencyObject element, out T parent, out int ancestorLevel) where T : DependencyObject
		{
			parent = null;
			ancestorLevel = 0;

			var currElement = element;
			var currLevel = 0;
			while (true)
			{
				++currLevel;
				currElement = VisualTreeHelper.GetParent(currElement);
				if (currElement is T tParent)
				{
					parent = tParent;
					ancestorLevel = currLevel;
				}
				else if (currElement == null)
				{
					break;
				}
			}
			return ancestorLevel > 0;
		}
		/// <summary>
		/// Returns every child <paramref name="parent"/> has.
		/// </summary>
		/// <param name="parent"></param>
		/// <returns></returns>
		public static IEnumerable<DependencyObject> GetChildren(this DependencyObject parent)
		{
			for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); ++i)
			{
				yield return VisualTreeHelper.GetChild(parent, i);
			}
		}

		public static bool TryGetFileText(object sender, out string text, out FileInfo fileInfo)
		{
			text = null;
			fileInfo = null;
			if (sender is FrameworkElement element && element.Tag is FileInfo fi && fi.Exists)
			{
				using (var reader = new StreamReader(fi.FullName))
				{
					text = reader.ReadToEnd();
					fileInfo = fi;
				}
				return true;
			}

			ConsoleActions.WriteLine("Unable to bring up the file.");
			return false;
		}
	}
}
