using Advobot.UILauncher.Utilities;
using Advobot.UILauncher.Enums;
using Advobot.UILauncher.Interfaces;
using System.Windows.Controls;
using System.Windows;

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
			SetResourceReference(Control.BackgroundProperty, ColorTarget.BaseBackground);
			SetResourceReference(Control.ForegroundProperty, ColorTarget.BaseForeground);
			SetResourceReference(Control.BorderBrushProperty, ColorTarget.BaseBorder);
		}
	}
}
