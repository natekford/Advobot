using Advobot.UILauncher.Actions;
using Advobot.UILauncher.Interfaces;
using ICSharpCode.AvalonEdit;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Advobot.UILauncher.Classes
{
	public class AdvobotTextEditor : TextEditor, IFontResizeValue
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
	}
}
