using System;
using System.Windows;
using System.Windows.Controls;
using Advobot.Windows.Interfaces;
using Advobot.Windows.Utilities;

namespace Advobot.Windows.Classes.Controls
{
	/// <summary>
	/// A <see cref="ComboBox"/> which implements some other useful properties and accepts custom colors easily.
	/// </summary>
	public class AdvobotComboBox : ComboBox, IFontResizeValue
	{
		/// <summary>
		/// <see cref="DependencyProperty"/> for <see cref="FontResizeValue"/>.
		/// </summary>
		public static readonly DependencyProperty FontResizeValueProperty = DependencyProperty.Register(nameof(FontResizeValue), typeof(double), typeof(AdvobotComboBox), new PropertyMetadata(ElementUtils.SetFontResizeProperty));
		/// <inheritdoc />
		public double FontResizeValue
		{
			get => (double)GetValue(FontResizeValueProperty);
			set => SetValue(FontResizeValueProperty, value);
		}
		/// <summary>
		/// <see cref="DependencyProperty"/> for <see cref="SourceEnum"/>.
		/// </summary>
		public static readonly DependencyProperty SourceEnumProperty = DependencyProperty.Register(nameof(SourceEnum), typeof(Type), typeof(AdvobotComboBox), new PropertyMetadata(SourceEnumCallback));
		/// <summary>
		/// An enum to make the item source out of.
		/// </summary>
		public Type SourceEnum
		{
			get => (Type)GetValue(SourceEnumProperty);
			set => SetValue(SourceEnumProperty, value);
		}

		private static void SourceEnumCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((AdvobotComboBox)d).ItemsSource = Enum.GetValues((Type)e.NewValue);
		}
	}
}
