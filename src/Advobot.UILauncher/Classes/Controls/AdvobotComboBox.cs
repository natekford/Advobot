using Advobot.UILauncher.Utilities;
using Advobot.UILauncher.Enums;
using Advobot.UILauncher.Interfaces;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Advobot.UILauncher.Classes.Controls
{
	/// <summary>
	/// A <see cref="ComboBox"/> which implements some other useful properties and accepts custom colors easily.
	/// </summary>
	internal class AdvobotComboBox : ComboBox, IFontResizeValue, IAdvobotControl
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

		public AdvobotComboBox()
		{
			VerticalContentAlignment = VerticalAlignment.Center;
			HorizontalContentAlignment = HorizontalAlignment.Center;
			SetResourceReferences();
		}
		public void SetResourceReferences()
		{
			SetResourceReference(Control.BackgroundProperty, ColorTarget.BaseBackground);
			SetResourceReference(Control.ForegroundProperty, ColorTarget.BaseForeground);
			SetResourceReference(Control.BorderBrushProperty, ColorTarget.BaseBorder);
		}

		private static void SourceEnumCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var enumType = (Type)e.NewValue;

			var source = new List<object>();
			foreach (var enumVal in Enum.GetValues(enumType) ?? throw new ArgumentException("must be an enum.", nameof(enumType)))
			{
				source.Add(AdvobotTextBox.CreateComboBoxItem(Enum.GetName(enumType, enumVal), enumVal));
			}

			((AdvobotComboBox)d).ItemsSource = source;
		}
		/// <summary>
		/// Returns textboxes with the text as the string and the tag as the string too.
		/// </summary>
		/// <param name="strings"></param>
		/// <returns></returns>
		public static IEnumerable<TextBox> CreateComboBoxSourceOutOfStrings(params string[] strings)
		{
			foreach (var s in strings)
			{
				yield return AdvobotTextBox.CreateComboBoxItem(s, s);
			}
		}
	}
}
