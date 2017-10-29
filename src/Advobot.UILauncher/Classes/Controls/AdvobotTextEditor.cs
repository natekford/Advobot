using Advobot.UILauncher.Actions;
using Advobot.UILauncher.Interfaces;
using ICSharpCode.AvalonEdit;

namespace Advobot.UILauncher.Classes.Controls
{
	internal class AdvobotTextEditor : TextEditor, IFontResizeValue
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

		public AdvobotTextEditor()
		{
			this.Background = null;
			this.Foreground = null;
			this.BorderBrush = null;
		}
	}
}
