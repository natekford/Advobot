using System;
using System.Collections.Concurrent;
using System.Linq;

using Advobot.UI.Converters;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;

namespace Advobot.UI.Controls
{
	/// <summary>
	/// Allows the font size of any <see cref="TemplatedControl"/> inside this to be based on the window's height.
	/// </summary>
	public class DynamicFontSizeGrid : Grid
	{
		public static readonly StyledProperty<double> DynamicFontSizeProperty =
			AvaloniaProperty.Register<DynamicFontSizeGrid, double>(nameof(DynamicFontSize));

		public static readonly AttachedProperty<double> OverrideDynamicFontSizeProperty =
			AvaloniaProperty.RegisterAttached<DynamicFontSizeGrid, Control, double>("OverrideDynamicFontSize");

		private static readonly ConcurrentDictionary<TemplatedControl, IDisposable> _Bindings
			= new ConcurrentDictionary<TemplatedControl, IDisposable>();

		public double DynamicFontSize
		{
			get => GetValue(DynamicFontSizeProperty);
			set
			{
				SetAllChildren(this, value);
				SetValue(DynamicFontSizeProperty, value);
			}
		}

		public static double GetOverrideDynamicFontSize(Control obj)
			=> obj.GetValue(OverrideDynamicFontSizeProperty);

		public static void SetOverrideDynamicFontSize(Control obj, double value)
		{
			if (obj is Panel panel)
			{
				SetAllChildren(panel, value);
			}
			if (obj is TemplatedControl templatedControl)
			{
				SetChild(templatedControl, value);
			}
			obj?.SetValue(OverrideDynamicFontSizeProperty, value);
		}

		public override void EndInit()
		{
			if (DynamicFontSize > 0)
			{
				SetAllChildren(this, DynamicFontSize);
			}
			base.EndInit();
		}

		private static void SetAllChildren(Panel parent, double value)
		{
			foreach (var child in parent.Children.OfType<TemplatedControl>())
			{
				var overrideValue = GetOverrideDynamicFontSize(child);
				//Skip any children where they have explicitly stated not to set anything via NaN
				if (double.IsNaN(overrideValue))
				{
					continue;
				}
				SetChild(child, overrideValue > 0 ? overrideValue : value);
			}
			foreach (var child in parent.Children.OfType<Panel>())
			{
				var overrideValue = GetOverrideDynamicFontSize(child);
				//If the override isn't set, and this is a dynamic font size grid, use its current value
				if (overrideValue <= 0 && child is DynamicFontSizeGrid dynamicFontSizeGrid)
				{
					overrideValue = dynamicFontSizeGrid.DynamicFontSize;
				}
				if (double.IsNaN(overrideValue))
				{
					continue;
				}
				if (child != null)
				{
					SetAllChildren(child, overrideValue > 0 ? overrideValue : value);
				}
			}
		}

		private static void SetChild(TemplatedControl obj, double value)
		{
			if (value < 0)
			{
				throw new ArgumentException("DynamicFontSize must be greater than or equal to 0.");
			}
			else if (value > 0)
			{
				//Set the font size to a percentage of the window size
				var binding = new Binding
				{
					Path = nameof(Height),
					RelativeSource = new RelativeSource
					{
						Mode = RelativeSourceMode.FindAncestor,
						AncestorType = typeof(Window),
					},
					Converter = new NetCoreFontResizeConverter(value),
				};
				//If a binding already exists, remove it then add in the new one
				var newBinding = obj.Bind(TemplatedControl.FontSizeProperty, binding);
				_Bindings.AddOrUpdate(obj, newBinding, (_, v) =>
				{
					v.Dispose();
					return newBinding;
				});
			}
			//If setting the value to 0, remove the binding
			else if (_Bindings.TryGetValue(obj, out var stored))
			{
				stored.Dispose();
			}
		}
	}
}