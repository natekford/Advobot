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
				(this as Control).SetBinding(Control.FontSizeProperty, new Binding
				{
					Path = new PropertyPath("ActualHeight"),
					RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Grid), 1),
					Converter = new FontResizer(value),
				});
				_FRV = value;
			}
		}
	}
}
