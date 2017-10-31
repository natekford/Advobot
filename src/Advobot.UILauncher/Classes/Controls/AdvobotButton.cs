using Advobot.UILauncher.Actions;
using Advobot.UILauncher.Interfaces;
using System.Windows.Controls;

namespace Advobot.UILauncher.Classes.Controls
{
	/// <summary>
	/// A <see cref="Button"/> which implements some other useful properties and accepts custom colors easily.
	/// </summary>
	internal class AdvobotButton : Button, IFontResizeValue
	{
		private double _FRV;
		public double FontResizeValue
		{
			get => _FRV;
			set
			{
				EntityActions.SetFontResizeProperty(this, value);
				_FRV = value;
			}
		}

		public AdvobotButton()
		{
			this.Background = null;
			this.Foreground = null;
			this.BorderBrush = null;
		}
	}
}
