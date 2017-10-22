using System.Windows;
using System.Windows.Controls;

namespace Advobot.UILauncher.Classes
{
	internal class AdvobotTextBox : TextBox
	{
		public AdvobotTextBox()
		{
			this.Background = null;
			this.Foreground = null;
			this.BorderBrush = null;
			this.TextWrapping = TextWrapping.Wrap;
		}
	}
}
