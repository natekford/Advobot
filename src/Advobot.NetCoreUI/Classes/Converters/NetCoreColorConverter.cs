using Advobot.NetCoreUI.Classes.Colors;
using Advobot.SharedUI.Converters;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace Advobot.NetCoreUI.Classes.Converters
{
	/// <summary>
	/// Converts a <see cref="SolidColorBrush"/>.
	/// </summary>
	public sealed class NetCoreColorConverter : ColorConverter<SolidColorBrush, NetCoreBrushFactory>, IValueConverter { }
}