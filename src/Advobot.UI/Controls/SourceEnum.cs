using System;

using Avalonia;
using Avalonia.Controls;

namespace Advobot.UI.Controls
{
	/// <summary>
	/// This is used to set the items of a drop down.
	/// </summary>
	public static class SourceEnum
	{
		public static readonly StyledProperty<Type> SourceEnumProperty =
			AvaloniaProperty.Register<ComboBox, Type>("SourceEnum");

		public static Type GetSourceEnum(ComboBox obj)
			=> obj.GetValue(SourceEnumProperty);

		public static void SetSourceEnum(ComboBox obj, Type value)
		{
			if (!value.IsEnum)
			{
				throw new ArgumentException($"Cannot set SourceEnum to a non enum type: {value.Name}");
			}

			obj.Items = value.GetEnumValues();
			obj.SetValue(SourceEnumProperty, value);
		}
	}
}