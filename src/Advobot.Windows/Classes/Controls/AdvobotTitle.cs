using System.Windows;
using System.Windows.Controls;
using Advobot.Windows.Interfaces;
using Advobot.Windows.Utilities;
using AdvorangesUtils;

namespace Advobot.Windows.Classes.Controls
{
	/// <summary>
	/// A <see cref="TextBlock"/> which implements some other useful properties.
	/// </summary>
	public class AdvobotTitle : TextBlock, IFontResizeValue
	{
		/// <summary>
		/// <see cref="DependencyProperty"/> for <see cref="FontResizeValue"/>.
		/// </summary>
		public static readonly DependencyProperty FontResizeValueProperty = DependencyProperty.Register(nameof(FontResizeValue), typeof(double), typeof(AdvobotTitle), new PropertyMetadata(ElementUtils.SetFontResizeProperty));
		/// <inheritdoc />
		public double FontResizeValue
		{
			get => (double)GetValue(FontResizeValueProperty);
			set => SetValue(FontResizeValueProperty, value);
		}

		/// <inheritdoc />
		public override void EndInit()
		{
			base.EndInit();
			Text = Name.FormatTitle().CaseInsReplace("title", "").Trim() + ":";
		}
	}
}
