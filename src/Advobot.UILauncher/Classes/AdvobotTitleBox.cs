using Advobot.Core.Actions;
using Advobot.Core.Actions.Formatting;
using Advobot.UILauncher.Actions;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Advobot.UILauncher.Classes
{
	internal class AdvobotTitleBox : AdvobotTextBox
	{
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
			this.VerticalContentAlignment = VerticalAlignment.Center;
			this.HorizontalAlignment = HorizontalAlignment.Left;
			this.TextWrapping = TextWrapping.WrapWithOverflow;
		}
		protected override void OnInitialized(EventArgs e)
		{
			this.Text = String.IsNullOrWhiteSpace(this.Text)
				? this.Name.FormatTitle().CaseInsReplace("title", "").Trim() + ":"
				: this.Text;
			base.OnInitialized(e);
		}
	}
}
