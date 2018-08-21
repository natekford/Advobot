using System.Windows.Data;
using System.Windows.Media;
using Advobot.SharedUI.Converters;

namespace Advobot.NetFrameworkUI.Classes.Converters
{
	/// <summary>
	/// Converts a <see cref="SolidColorBrush"/>.
	/// </summary>
	public sealed class NetFrameworkColorConverter : ColorConverter<SolidColorBrush, NetFrameworkBrushFactory>, IValueConverter { }
}