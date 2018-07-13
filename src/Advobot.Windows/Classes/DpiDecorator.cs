using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Advobot.Windows.Classes
{
	/// <summary>
	/// Changes the dpi of the program to account for other dpi of the monitor.
	/// </summary>
	public class DpiDecorator : Decorator
	{
		/// <summary>
		/// Creates an instance of dpidecorator.
		/// </summary>
		public DpiDecorator()
		{
			Loaded += (sender, e) =>
			{
				var matrix = PresentationSource.FromVisual(this).CompositionTarget.TransformToDevice;
				var dpiTransform = new ScaleTransform(1 / matrix.M11, 1 / matrix.M22);
				if (dpiTransform.CanFreeze)
				{
					dpiTransform.Freeze();
				}
				LayoutTransform = dpiTransform;
			};
		}
	}
}
