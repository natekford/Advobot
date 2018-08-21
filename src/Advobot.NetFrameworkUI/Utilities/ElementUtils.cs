using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Advobot.NetFrameworkUI.Classes.Converters;
using Advobot.NetFrameworkUI.Interfaces;

namespace Advobot.NetFrameworkUI.Utilities
{
	internal static class ElementUtils
	{
		/// <summary>
		/// Sets the <see cref="Grid.RowSpanProperty"/> to either 1 or the supplied length.
		/// </summary>
		/// <param name="item"></param>
		/// <param name="length"></param>
		public static void SetRowSpan(UIElement item, int length)
		{
			Grid.SetRowSpan(item, Math.Max(1, length));
		}
		/// <summary>
		/// Sets the <see cref="Grid.ColumnSpanProperty"/> to either 1 or the supplied length.
		/// </summary>
		/// <param name="item"></param>
		/// <param name="length"></param>
		public static void SetColSpan(UIElement item, int length)
		{
			Grid.SetColumnSpan(item, Math.Max(1, length));
		}
		/// <summary>
		/// Sets the <see cref="Control.FontSizeProperty"/> to a number that changes based off of the top most grid's height.
		/// Zero removes any binding on the <see cref="Control.FontSizeProperty"/>.
		/// </summary>
		/// <param name="d"></param>
		/// <param name="e"></param>
		/// <exception cref="ArgumentException">If <paramref name="d"/> is not inside a grid.</exception>
		/// <exception cref="ArgumentException">If <paramref name="e"/>'s new value is less than zero.</exception>
		public static void SetFontResizeProperty(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (!(d is FrameworkElement element))
			{
				throw new ArgumentException("is not a FrameworkElement", nameof(d));
			}
			if (!(e.NewValue is double size))
			{
				throw new ArgumentException("is not a double", nameof(e.NewValue));
			}
			if (!TryGetTopMostParent(element, out Grid parent, out var ancestorLevel))
			{
				throw new ArgumentException($"must be inside a grid if {nameof(IFontResizeValue.FontResizeValue)} is set", element.Name);
			}
			if (size < 0)
			{
				throw new ArgumentException("must be greater than or equal to 0", nameof(size));
			}
			if (size == 0)
			{
				BindingOperations.ClearBinding(element, Control.FontSizeProperty);
				return;
			}

			element.SetBinding(Control.FontSizeProperty, new Binding
			{
				Path = new PropertyPath(nameof(FrameworkElement.ActualHeight)),
				RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Grid), ancestorLevel),
				Converter = new NetFrameworkFontResizeConverter(size)
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
		public static bool TryGetTopMostParent<T>(this DependencyObject element, out T parent, out int ancestorLevel) where T : DependencyObject
		{
			parent = null;
			ancestorLevel = 0;

			var currElement = element;
			while (currElement != null)
			{
				currElement = VisualTreeHelper.GetParent(currElement);
				if (currElement is T tParent)
				{
					parent = tParent;
					++ancestorLevel;
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
			for (var i = 0; i < VisualTreeHelper.GetChildrenCount(parent); ++i)
			{
				yield return VisualTreeHelper.GetChild(parent, i);
			}
		}
	}
}
