using Advobot.UILauncher.Utilities;
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
				ElementUtils.SetFontResizeProperty(this, value);
				_FRV = value;
			}
		}

		public AdvobotButton()
		{
			SetResourceReferences();
		}

		public void SetResourceReferences() => SetResourceReference(Button.StyleProperty, OtherTarget.ButtonStyle);
	}
}
