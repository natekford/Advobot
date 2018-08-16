using System.Windows;
using System.Windows.Controls.Primitives;
using Advobot.NetFrameworkUI.Interfaces;
using Advobot.NetFrameworkUI.Utilities;

namespace Advobot.NetFrameworkUI.Classes.Controls
{
	/// <summary>
	/// A <see cref="RepeatButton"/> which implements some other useful properties.
	/// </summary>
	public class AdvobotRepeatButton : RepeatButton, IFontResizeValue
	{
		/// <summary>
		/// <see cref="DependencyProperty"/> for <see cref="FontResizeValue"/>.
		/// </summary>
		public static readonly DependencyProperty FontResizeValueProperty = DependencyProperty.Register(nameof(FontResizeValue), typeof(double), typeof(AdvobotRepeatButton), new PropertyMetadata(ElementUtils.SetFontResizeProperty));
		/// <inheritdoc />
		public double FontResizeValue
		{
			get => (double)GetValue(FontResizeValueProperty);
			set => SetValue(FontResizeValueProperty, value);
		}
	}
}
