using System.Globalization;

namespace Advobot.Services.Localization
{
	/// <summary>
	/// Localizes the display names for types.
	/// </summary>
	public interface ITypeLocalizer
	{
		/// <summary>
		/// Adds a localization value for <typeparamref name="T"/> with <see cref="CultureInfo.CurrentUICulture"/>.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="value"></param>
		/// <param name="overwrite"></param>
		void Add<T>(string value, bool overwrite = false);

		/// <summary>
		/// Attempts to find a localization value for <typeparamref name="T"/> with <see cref="CultureInfo.CurrentUICulture"/>.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="output"></param>
		/// <returns></returns>
		bool TryGet<T>(out string? output);
	}
}