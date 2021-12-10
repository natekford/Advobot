using System.Globalization;

namespace Advobot.Services.Localization;

/// <summary>
/// Localizes the display names for enums.
/// </summary>
public interface IEnumLocalizer
{
	/// <summary>
	/// Adds localization values for <typeparamref name="T"/> with <see cref="CultureInfo.CurrentUICulture"/>.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="values"></param>
	/// <param name="overwrite"></param>
	void Add<T>(IDictionary<T, string> values, bool overwrite = false) where T : Enum;

	/// <summary>
	/// Attempts to find localization values for <typeparamref name="T"/> with <see cref="CultureInfo.CurrentUICulture"/>.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="output"></param>
	/// <returns></returns>
	bool TryGet<T>(out IReadOnlyCollection<string>? output) where T : Enum;

	/// <summary>
	/// Attempts to find a localization value for <paramref name="value"/> with <see cref="CultureInfo.CurrentUICulture"/>.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="value"></param>
	/// <param name="output"></param>
	/// <returns></returns>
	bool TryGet<T>(T value, out string? output) where T : Enum;
}