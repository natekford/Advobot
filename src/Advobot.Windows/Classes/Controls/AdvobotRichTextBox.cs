using System.Windows;
using System.Windows.Controls;
using Advobot.Windows.Interfaces;
using Advobot.Windows.Utilities;

namespace Advobot.Windows.Classes.Controls
{
	/// <summary>
	/// A <see cref="RichTextBox"/> which implements some other useful properties and accepts custom colors easily.
	/// </summary>
	public class AdvobotRichTextBox : RichTextBox, IFontResizeValue
	{
		/// <summary>
		/// <see cref="DependencyProperty"/> for <see cref="FontResizeValue"/>.
		/// </summary>
		public static readonly DependencyProperty FontResizeValueProperty = DependencyProperty.Register(nameof(FontResizeValue), typeof(double), typeof(AdvobotRichTextBox), new PropertyMetadata(ElementUtils.SetFontResizeProperty));
		/// <inheritdoc />
		public double FontResizeValue
		{
			get => (double)GetValue(FontResizeValueProperty);
			set => SetValue(FontResizeValueProperty, value);
		}
	}
}
