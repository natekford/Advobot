using Advobot.UILauncher.Actions;
using Advobot.UILauncher.Interfaces;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Advobot.UILauncher.Classes.Controls
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

		public override void EndInit()
		{
			SetAllChildrenToFontSizeProperty(this);
			base.EndInit();
		}

		private void SetAllChildrenToFontSizeProperty(DependencyObject parent)
		{
			foreach (var child in parent.GetChildren())
			{
				if (child.GetChildren().Any())
				{
					SetAllChildrenToFontSizeProperty(child);
				}

				//Don't set on things that it can't be set on
				if (child is CheckBox)
				{
					continue;
				}
				//Don't set it on controls with it already set
				else if (child is Control c && c.GetBindingExpression(Control.FontSizeProperty) == null)
				{
					EntityActions.SetFontResizeProperty(c, _FRV);
				}
			}
		}
	}
}
