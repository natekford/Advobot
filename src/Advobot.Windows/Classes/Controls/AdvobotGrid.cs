using System.Windows;
using System.Windows.Controls;
using Advobot.Windows.Enums;
using Advobot.Windows.Interfaces;
using Advobot.Windows.Utilities;

namespace Advobot.Windows.Classes.Controls
{
	/// <summary>
	/// A <see cref="Grid"/> which implements some other useful properties and accepts custom colors easily.
	/// </summary>
	internal class AdvobotGrid : Grid, IFontResizeValue, IAdvobotControl
	{
		private double _FRV;
		public double FontResizeValue
		{
			get => _FRV;
			set => SetAllChildrenToFontSizeProperty(this, _FRV = value);
		}

		public AdvobotGrid()
		{
			SetResourceReferences();
		}

		public override void EndInit()
		{
			base.EndInit();
			if (_FRV > 0)
			{
				SetAllChildrenToFontSizeProperty(this, _FRV);
			}
		}
		public void SetResourceReferences()
		{
			SetResourceReference(Control.BackgroundProperty, ColorTarget.BaseBackground);
			SetResourceReference(Control.ForegroundProperty, ColorTarget.BaseForeground);
			SetResourceReference(Control.BorderBrushProperty, ColorTarget.BaseBorder);
		}
		private static void SetAllChildrenToFontSizeProperty(DependencyObject parent, double value)
		{
			foreach (var child in parent.GetChildren())
			{
				//Don't set it on controls with it already set
				if (child is IFontResizeValue frv && frv.FontResizeValue == default)
				{
					frv.FontResizeValue = value;
				}
				SetAllChildrenToFontSizeProperty(child, value);
			}
		}
	}
}
