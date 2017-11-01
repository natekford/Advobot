using Advobot.UILauncher.Actions;
using Advobot.UILauncher.Interfaces;
using System.Windows;
using System.Windows.Controls;

namespace Advobot.UILauncher.Classes.Controls
{
	/// <summary>
	/// A <see cref="Grid"/> which implements some other useful properties and accepts custom colors easily.
	/// </summary>
	internal class AdvobotGrid : Grid, IFontResizeValue
	{
		private double _FRV;
		public double FontResizeValue
		{
			get => _FRV;
			set
			{
				SetAllChildrenToFontSizeProperty(this);
				_FRV = value;
			}
		}

		public override void EndInit()
		{
			if (_FRV > 0)
			{
				SetAllChildrenToFontSizeProperty(this);
			}
			base.EndInit();
		}

		private void SetAllChildrenToFontSizeProperty(DependencyObject parent)
		{
			foreach (var child in parent.GetChildren())
			{
				//Don't set it on controls with it already set
				if (child is IFontResizeValue frv && frv.FontResizeValue == default)
				{
					frv.FontResizeValue = _FRV;
				}
				SetAllChildrenToFontSizeProperty(child);
			}
		}
	}
}
