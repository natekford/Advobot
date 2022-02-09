using System.Globalization;

namespace Advobot.UI.AbstractUI.Converters;

/// <summary>
/// Returns true if the object is not null or whitespace.
/// </summary>
public abstract class NullToBoolConverter
{
	/// <summary>
	/// Checks if the value is null or whitespace.
	/// </summary>
	/// <param name="value"></param>
	/// <param name="_"></param>
	/// <param name="_2"></param>
	/// <param name="_3"></param>
	/// <returns></returns>
	public object Convert(object value, Type _, object _2, CultureInfo _3)
		=> value is string s ? !string.IsNullOrWhiteSpace(s) : value != null;

	/// <summary>
	/// Not implemented.
	/// </summary>
	/// <param name="_"></param>
	/// <param name="_2"></param>
	/// <param name="_3"></param>
	/// <param name="_4"></param>
	/// <returns></returns>
	/// <exception cref="NotImplementedException"></exception>
	public object ConvertBack(object _, Type _2, object _3, CultureInfo _4)
		=> throw new NotImplementedException();
}