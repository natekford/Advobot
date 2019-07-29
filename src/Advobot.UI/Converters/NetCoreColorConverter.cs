using Avalonia.Data.Converters;
using Avalonia.Media;
using Advobot.UI.AbstractUI.Converters;
using Advobot.UI.Colors;

namespace Advobot.UI.Converters
{
	/// <summary>
	/// Converts a <see cref="SolidColorBrush"/>.
	/// </summary>
	public sealed class NetCoreColorConverter : ColorConverter<ISolidColorBrush, NetCoreBrushFactory>, IValueConverter { }
}