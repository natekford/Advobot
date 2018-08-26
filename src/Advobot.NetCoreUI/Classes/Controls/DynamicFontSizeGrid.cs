using System;
using System.Collections.Concurrent;
using System.Linq;
using Advobot.NetCoreUI.Classes.Converters;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;

namespace Advobot.NetCoreUI.Classes.Controls
{
	/// <summary>
	/// Allows the font size of any <see cref="TemplatedControl"/> inside this to be based on the window's height.
	/// </summary>
	public class DynamicFontSizeGrid : Grid
	{
        private static readonly ConcurrentDictionary<TemplatedControl, IDisposable> _Bindings
			= new ConcurrentDictionary<TemplatedControl, IDisposable>();

		public static readonly StyledProperty<double> DynamicFontSizeProperty =
			AvaloniaProperty.Register<DynamicFontSizeGrid, double>("DynamicFontSize");

		public static void SetDynamicFontSize(DynamicFontSizeGrid obj, double value)
		{
			SetAllChildren(obj, value);
			obj.SetValue(DynamicFontSizeProperty, value);
		}
		public static double GetDynamicFontSize(DynamicFontSizeGrid obj)
		{
			return obj.GetValue(DynamicFontSizeProperty);
		}

		public static readonly AttachedProperty<double> OverrideDynamicFontSizeProperty =
			AvaloniaProperty.RegisterAttached<DynamicFontSizeGrid, Control, double>("OverrideDynamicFontSize");

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
            obj.SetValue(OverrideDynamicFontSizeProperty, value);
		}
		public static double GetOverrideDynamicFontSize(Control obj)
		{
			return obj.GetValue(OverrideDynamicFontSizeProperty);
		}

		public override void EndInit()
		{
			var val = GetDynamicFontSize(this);
			if (val > 0)
			{
				SetAllChildren(this, val);
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
				if (!(overrideValue > 0) && child is DynamicFontSizeGrid dynamicFontSizeGrid)
				{
					overrideValue = GetDynamicFontSize(dynamicFontSizeGrid);
				}
				if (double.IsNaN(overrideValue))
				{
					continue;
				}
				SetAllChildren(child, overrideValue > 0 ? overrideValue : value);
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
					Path = nameof(Window.Height),
					RelativeSource = new RelativeSource
					{
						Mode = RelativeSourceMode.FindAncestor,
						AncestorType = typeof(Window),
					},
					Converter = new NetCoreFontResizeConverter(value),
				};
				//If a binding already exists, remove it then add in the new one
				var newBinding = obj.Bind(TemplatedControl.FontSizeProperty, binding);
				_Bindings.AddOrUpdate(obj, newBinding, (k, v) =>
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