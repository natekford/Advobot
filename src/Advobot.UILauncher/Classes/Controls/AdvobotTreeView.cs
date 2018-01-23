﻿using System.Windows;
using System.Windows.Controls;
using Advobot.UILauncher.Enums;
using Advobot.UILauncher.Interfaces;
using Advobot.UILauncher.Utilities;

namespace Advobot.UILauncher.Classes.Controls
{
	/// <summary>
	/// A <see cref="TreeView"/> which implements some other useful properties and accepts custom colors easily.
	/// </summary>
	internal class AdvobotTreeView : TreeView, IFontResizeValue, IAdvobotControl
	{
		public static readonly DependencyProperty FontResizeValueProperty = DependencyProperty.Register("FontResizeValue", typeof(double), typeof(AdvobotTreeView), new PropertyMetadata(ElementUtils.SetFontResizeProperty));
		public double FontResizeValue
		{
			get => (double)GetValue(FontResizeValueProperty);
			set => SetValue(FontResizeValueProperty, value);
		}

		public AdvobotTreeView()
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
