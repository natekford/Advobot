using System.Windows;
using System.Windows.Controls.Primitives;
using Advobot.Windows.Interfaces;
using Advobot.Windows.Utilities;

namespace Advobot.Windows.Classes.Controls
{
	/// <summary>
	/// A <see cref="RepeatButton"/> which implements some other useful properties.
	/// </summary>
	internal class AdvobotRepeatButton : RepeatButton, IFontResizeValue
	{
		public static readonly DependencyProperty FontResizeValueProperty = DependencyProperty.Register("FontResizeValue", typeof(double), typeof(AdvobotRepeatButton), new PropertyMetadata(ElementUtils.SetFontResizeProperty));
		public double FontResizeValue
		{
			get => (double)GetValue(FontResizeValueProperty);
			set => SetValue(FontResizeValueProperty, value);
		}
	}
}
