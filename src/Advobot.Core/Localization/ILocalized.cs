using System.Resources;

namespace Advobot.Localization
{
	/// <summary>
	/// Something which is localized.
	/// </summary>
	public interface ILocalized
	{
		/// <summary>
		/// The resource manager containing the localization strings.
		/// </summary>
		ResourceManager ResourceManager { get; }
	}
}