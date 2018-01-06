using Advobot.UILauncher.Utilities;
using Advobot.UILauncher.Enums;
using Advobot.UILauncher.Interfaces;
using ICSharpCode.AvalonEdit;
using System.Windows.Controls;
using System.Windows;

namespace Advobot.UILauncher.Classes.Controls
{
	/// <summary>
	/// A <see cref="TextEditor"/> which implements some other useful properties and accepts custom colors easily.
	/// </summary>
	internal class AdvobotTextEditor : TextEditor, IFontResizeValue, IAdvobotControl
	{
		public static readonly DependencyProperty FontResizeValueProperty = DependencyProperty.Register("FontResizeValue", typeof(double), typeof(AdvobotTextEditor), new PropertyMetadata(ElementUtils.SetFontResizeProperty));
		public double FontResizeValue
		{
			get => (double)GetValue(FontResizeValueProperty);
			set => SetValue(FontResizeValueProperty, value);
		}

		public AdvobotTextEditor()
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
