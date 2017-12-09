using Advobot.UILauncher.Actions;
using Advobot.UILauncher.Enums;
using Advobot.UILauncher.Interfaces;
using System.Windows;
using System.Windows.Controls;

namespace Advobot.UILauncher.Classes.Controls
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
			set
			{
				SetAllChildrenToFontSizeProperty(this);
				this._FRV = value;
			}
		}

		public AdvobotGrid()
		{
			SetResourceReferences();
		}

		public override void EndInit()
		{
			base.EndInit();
			if (this._FRV > 0)
			{
				SetAllChildrenToFontSizeProperty(this);
			}
		}
		public void SetResourceReferences()
		{
			this.SetResourceReference(Control.BackgroundProperty, ColorTarget.BaseBackground);
			this.SetResourceReference(Control.ForegroundProperty, ColorTarget.BaseForeground);
			this.SetResourceReference(Control.BorderBrushProperty, ColorTarget.BaseBorder);
		}

		private void SetAllChildrenToFontSizeProperty(DependencyObject parent)
		{
			foreach (var child in parent.GetChildren())
			{
				//Don't set it on controls with it already set
				if (child is IFontResizeValue frv && frv.FontResizeValue == default)
				{
					frv.FontResizeValue = this._FRV;
				}
				SetAllChildrenToFontSizeProperty(child);
			}
		}
	}
}
