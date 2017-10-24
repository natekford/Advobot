using Advobot.UILauncher.Actions;
using Advobot.UILauncher.Interfaces;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Advobot.UILauncher.Classes
{
	internal class AdvobotRichTextBox : RichTextBox, IFontResizeValue
	{
		private double _FRV;
		public double FontResizeValue
		{
			get => _FRV;
			set
			{
				UIModification.SetFontResizeProperty(this, value);
				_FRV = value;
			}
		}

		public AdvobotRichTextBox()
		{
			this.Background = null;
			this.Foreground = null;
			this.BorderBrush = null;
		}
	}
}
