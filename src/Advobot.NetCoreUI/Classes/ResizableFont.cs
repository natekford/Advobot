using System;
using Advobot.NetCoreUI.Classes.Converters;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;

namespace Advobot.NetCoreUI.Classes
{
	/// <summary>
	/// This is used to resize the font inside something with text.
	/// </summary>
	public static class ResizableFont
	{
		public static readonly StyledProperty<double> FontResizeProperty =
			AvaloniaProperty.Register<TemplatedControl, double>("FontResize");

		public static void SetFontResize(TemplatedControl obj, double value)
		{
			if (value < 0)
			{
				throw new ArgumentException("FontResize must be greater than or equal to 0.");
			}
			if (value == 0)
			{
				throw new NotImplementedException("Unable to find out how to remove bindings for FontResize.");
			}
			else
			{
				var binding = new Binding
				{
					Path = nameof(Window.Height),
					RelativeSource = new RelativeSource
					{
						Mode = RelativeSourceMode.FindAncestor,
						AncestorType = typeof(Window),
					},
					Converter = new NetCoreFontResizeConverter(value),
				};
				obj.Bind(TemplatedControl.FontSizeProperty, binding);
			}
			obj.SetValue(FontResizeProperty, value);
		}
		public static double GetFontResize(TemplatedControl obj)
		{
			return obj.GetValue(FontResizeProperty);
		}
	}
}