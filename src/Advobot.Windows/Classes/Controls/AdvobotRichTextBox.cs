﻿using System.Windows;
using System.Windows.Controls;
using Advobot.Windows.Enums;
using Advobot.Windows.Interfaces;
using Advobot.Windows.Utilities;

namespace Advobot.Windows.Classes.Controls
{
	/// <summary>
	/// A <see cref="RichTextBox"/> which implements some other useful properties and accepts custom colors easily.
	/// </summary>
	internal class AdvobotRichTextBox : RichTextBox, IFontResizeValue, IAdvobotControl
	{
		public static readonly DependencyProperty FontResizeValueProperty = DependencyProperty.Register("FontResizeValue", typeof(double), typeof(AdvobotRichTextBox), new PropertyMetadata(ElementUtils.SetFontResizeProperty));
		public double FontResizeValue
		{
			get => (double)GetValue(FontResizeValueProperty);
			set => SetValue(FontResizeValueProperty, value);
		}

		public AdvobotRichTextBox()
		{
			SetResourceReferences();
		}

		public void SetResourceReferences()
		{
			SetResourceReference(BackgroundProperty, ColorTarget.BaseBackground);
			SetResourceReference(ForegroundProperty, ColorTarget.BaseForeground);
			SetResourceReference(BorderBrushProperty, ColorTarget.BaseBorder);
		}
	}
}