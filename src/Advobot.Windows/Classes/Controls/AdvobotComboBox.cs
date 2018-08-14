using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Advobot.Windows.Enums;
using Advobot.Windows.Interfaces;
using Advobot.Windows.Utilities;

namespace Advobot.Windows.Classes.Controls
{
	/// <summary>
	/// A <see cref="ComboBox"/> which implements some other useful properties and accepts custom colors easily.
	/// </summary>
	internal class AdvobotComboBox : ComboBox, IFontResizeValue
	{
		public static readonly DependencyProperty FontResizeValueProperty = DependencyProperty.Register("FontResizeValue", typeof(double), typeof(AdvobotComboBox), new PropertyMetadata(ElementUtils.SetFontResizeProperty));
		public double FontResizeValue
		{
			get => (double)GetValue(FontResizeValueProperty);
			set => SetValue(FontResizeValueProperty, value);
		}
		public static readonly DependencyProperty SourceEnumProperty = DependencyProperty.Register("SourceEnum", typeof(Type), typeof(AdvobotComboBox), new PropertyMetadata(SourceEnumCallback));
		public Type SourceEnum
		{
			get => (Type)GetValue(SourceEnumProperty);
			set => SetValue(SourceEnumProperty, value);
		}

		private static void SourceEnumCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (!(d is AdvobotComboBox cb))
			{
				return;
			}

			cb.ItemsSource = Enum.GetValues((Type)e.NewValue);
		}
	}
}
