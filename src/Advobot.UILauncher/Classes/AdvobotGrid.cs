using Advobot.UILauncher.Actions;
using Advobot.UILauncher.Interfaces;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Advobot.UILauncher.Classes
{
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

		private void SetAllChildrenToFontSizeProperty(DependencyObject parent)
		{
			foreach (var child in parent.GetChildren())
			{
				if (child.GetChildren().Count() > 0)
				{
					SetAllChildrenToFontSizeProperty(child);
				}
				if (child is Control c)
				{
					UIModification.SetFontResizeProperty(c, _FRV);
				}
			}
		}

		public override void EndInit()
		{
			SetAllChildrenToFontSizeProperty(this);
			base.EndInit();
		}
	}
}
