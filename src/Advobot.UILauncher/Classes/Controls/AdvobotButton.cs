using Advobot.UILauncher.Actions;
using Advobot.UILauncher.Enums;
using Advobot.UILauncher.Interfaces;
using System.Windows.Controls;

namespace Advobot.UILauncher.Classes.Controls
{
	/// <summary>
	/// A <see cref="Button"/> which implements some other useful properties and accepts custom colors easily.
	/// </summary>
	internal class AdvobotButton : Button, IFontResizeValue, IAdvobotControl
	{
		private double _FRV;
		public double FontResizeValue
		{
			get => _FRV;
			set
			{
				EntityActions.SetFontResizeProperty(this, value);
				this._FRV = value;
			}
		}

		public AdvobotButton()
		{
			SetResourceReferences();
		}

		public void SetResourceReferences() => this.SetResourceReference(Button.StyleProperty, OtherTarget.ButtonStyle);
	}
}
