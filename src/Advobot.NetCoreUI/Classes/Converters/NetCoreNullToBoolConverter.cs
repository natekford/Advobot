using Advobot.SharedUI.Converters;
using Avalonia.Data.Converters;

namespace Advobot.NetCoreUI.Classes.Converters
{
	/// <summary>
	/// Returns true if the object is not null or whitespace.
	/// </summary>
	public sealed class NetCoreNullToBoolConverter : NullToBoolConverter, IValueConverter { }
}
