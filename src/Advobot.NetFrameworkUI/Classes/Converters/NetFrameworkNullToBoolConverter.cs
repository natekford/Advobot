using Advobot.SharedUI.Converters;
using System.Windows.Data;

namespace Advobot.NetFrameworkUI.Classes.Converters
{
	/// <summary>
	/// Returns true if the object is not null or whitespace.
	/// </summary>
	public sealed class NetFrameworkNullToBoolConverter : NullToBoolConverter, IValueConverter { }
}
