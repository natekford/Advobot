using System.Windows;
using System.Windows.Controls;
using Advobot.Windows.Interfaces;
using Advobot.Windows.Utilities;

namespace Advobot.Windows.Classes.Controls
{
	/// <summary>
	/// A <see cref="Grid"/> which implements some other useful properties and accepts custom colors easily.
	/// </summary>
	public class AdvobotGrid : Grid, IFontResizeValue
	{
		/// <summary>
		/// <see cref="DependencyProperty"/> for <see cref="FontResizeValue"/>.
		/// </summary>
		public static readonly DependencyProperty FontResizeValueProperty = DependencyProperty.Register(nameof(FontResizeValue), typeof(double), typeof(AdvobotGrid), new PropertyMetadata(SetAllChildrenToFontSizeProperty));
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
			SetAllChildrenToFontSizeProperty(this, new DependencyPropertyChangedEventArgs(FontResizeValueProperty, default, FontResizeValue));
		}
		private static void SetAllChildrenToFontSizeProperty(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			foreach (var child in d.GetChildren())
			{
				//Don't set it on controls with it already set
				if (child is IFontResizeValue frv && frv.FontResizeValue == default)
				{
					frv.FontResizeValue = (double)e.NewValue;
				}
				SetAllChildrenToFontSizeProperty(child, e);
			}
		}
	}
}
