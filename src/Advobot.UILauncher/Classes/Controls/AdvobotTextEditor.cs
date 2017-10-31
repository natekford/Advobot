using Advobot.UILauncher.Actions;
using Advobot.UILauncher.Interfaces;
using ICSharpCode.AvalonEdit;

namespace Advobot.UILauncher.Classes.Controls
{
	/// <summary>
	/// A <see cref="TextEditor"/> which implements some other useful properties and accepts custom colors easily.
	/// </summary>
	internal class AdvobotTextEditor : TextEditor, IFontResizeValue
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

		public AdvobotTextEditor()
		{
			this.Background = null;
			this.Foreground = null;
			this.BorderBrush = null;
		}
	}
}
