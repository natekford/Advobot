using Advobot.Windows.Interfaces;
using Advobot.Windows.Utilities;
using AdvorangesUtils;
using System.Windows;
using System.Windows.Controls;

namespace Advobot.Windows.Classes.Controls
{
	/// <summary>
	/// A <see cref="TextBlock"/> which implements some other useful properties.
	/// </summary>
	internal class AdvobotTitle : TextBlock, IFontResizeValue
	{
		public static readonly DependencyProperty FontResizeValueProperty = DependencyProperty.Register("FontResizeValue", typeof(double), typeof(AdvobotTitle), new PropertyMetadata(ElementUtils.SetFontResizeProperty));
		public double FontResizeValue
		{
			get => (double)GetValue(FontResizeValueProperty);
			set => SetValue(FontResizeValueProperty, value);
		}

		public override void EndInit()
		{
			base.EndInit();
			Text = Name.FormatTitle().CaseInsReplace("title", "").Trim() + ":";
		}
	}
}
