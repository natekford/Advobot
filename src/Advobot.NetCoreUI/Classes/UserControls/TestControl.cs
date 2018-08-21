using System;
using Advobot.NetCoreUI.Classes.Converters;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;

namespace Advobot.NetCoreUI.Classes.UserControls
{
	public sealed class TestControl : TextBox
	{
		public static readonly StyledProperty<double> FontResizeProperty =
			AvaloniaProperty.Register<TestControl, double>(nameof(FontResize));

		public double FontResize
		{
			get => GetValue(FontResizeProperty);
			set
			{
				/*
				if (value < 0)
				{
					throw new ArgumentException($"{nameof(FontResize)} must be greater than or equal to 0.");
				}
				else if (value == 0)
				{
					Bind(FontResizeProperty, null);
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
					this.Bind(FontResizeProperty, binding);
				}*/
				var dog = 1;
				Console.WriteLine(dog.ToString());
				SetValue(FontResizeProperty, value);
			}
		}
	}
}
