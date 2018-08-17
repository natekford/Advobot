﻿using System.Windows;
using Advobot.NetFrameworkUI.Interfaces;
using Advobot.NetFrameworkUI.Utilities;
using ICSharpCode.AvalonEdit;

namespace Advobot.NetFrameworkUI.Classes.Controls
{
	/// <summary>
	/// A <see cref="TextEditor"/> which implements some other useful properties and accepts custom colors easily.
	/// </summary>
	public class AdvobotTextEditor : TextEditor, IFontResizeValue
	{
		/// <summary>
		/// <see cref="DependencyProperty"/> for <see cref="FontResizeValue"/>.
		/// </summary>
		public static readonly DependencyProperty FontResizeValueProperty = DependencyProperty.Register(nameof(FontResizeValue), typeof(double), typeof(AdvobotTextEditor), new PropertyMetadata(ElementUtils.SetFontResizeProperty));
		/// <inheritdoc />
		public double FontResizeValue
		{
			get => (double)GetValue(FontResizeValueProperty);
			set => SetValue(FontResizeValueProperty, value);
		}
	}
}