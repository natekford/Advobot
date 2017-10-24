using Advobot.Core.Actions.Formatting;
using Advobot.UILauncher.Actions;
using Advobot.UILauncher.Interfaces;
using System.Windows;
using System.Windows.Controls;

namespace Advobot.UILauncher.Classes
{
	internal class AdvobotTitleBox : AdvobotTextBox, ITitle
	{
		public string Title
		{
			set => base.Text = value.FormatTitle();
		}
		private string _S;
		public string Summary
		{
			get => _S;
			set
			{
				ToolTip = new ToolTip { Content = value, };
				this.MouseEnter += (sender, e) => UIModification.ToggleToolTip((ToolTip)ToolTip);
				this.MouseLeave += (sender, e) => UIModification.ToggleToolTip((ToolTip)ToolTip);
				_S = value;
			}
		}

		public AdvobotTitleBox() : base()
		{
			this.IsReadOnly = true;
			this.BorderThickness = new Thickness(0);
			this.VerticalAlignment = VerticalAlignment.Center;
			this.HorizontalAlignment = HorizontalAlignment.Left;
			this.TextWrapping = TextWrapping.WrapWithOverflow;
		}
	}
}
