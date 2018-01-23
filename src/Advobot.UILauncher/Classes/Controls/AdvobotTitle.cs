using System.Windows;
using System.Windows.Controls;
using Advobot.Core.Utilities;
using Advobot.Core.Utilities.Formatting;
using Advobot.UILauncher.Interfaces;
using Advobot.UILauncher.Utilities;

namespace Advobot.UILauncher.Classes.Controls
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

		public AdvobotTitle()
		{
			VerticalAlignment = VerticalAlignment.Center;
			TextWrapping = TextWrapping.WrapWithOverflow;
		}

		public override void EndInit()
		{
			base.EndInit();
			Text = Name.FormatTitle().CaseInsReplace("title", "").Trim() + ":";
		}
	}
}
