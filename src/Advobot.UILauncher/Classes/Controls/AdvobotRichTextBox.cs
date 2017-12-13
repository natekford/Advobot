using Advobot.UILauncher.Utilities;
using Advobot.UILauncher.Enums;
using Advobot.UILauncher.Interfaces;
using System.Windows.Controls;

namespace Advobot.UILauncher.Classes.Controls
{
	/// <summary>
	/// A <see cref="RichTextBox"/> which implements some other useful properties and accepts custom colors easily.
	/// </summary>
	internal class AdvobotRichTextBox : RichTextBox, IFontResizeValue, IAdvobotControl
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

		public AdvobotRichTextBox()
		{
			SetResourceReferences();
		}

		public void SetResourceReferences()
		{
			SetResourceReference(Control.BackgroundProperty, ColorTarget.BaseBackground);
			SetResourceReference(Control.ForegroundProperty, ColorTarget.BaseForeground);
			SetResourceReference(Control.BorderBrushProperty, ColorTarget.BaseBorder);
		}
	}
}
