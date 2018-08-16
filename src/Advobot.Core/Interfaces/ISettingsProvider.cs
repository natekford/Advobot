using System.Collections.Generic;
using System.Reflection;

namespace Advobot.Interfaces
{
	/// <summary>
	/// Abstraction for getting the type of settings this either directly is or is a factory for.
	/// </summary>
	public interface ISettingsProvider<TSettings> where TSettings : ISettingsBase
	{
		/// <summary>
		/// Gets the settings that <typeparamref name="TSettings"/> uses.
		/// </summary>
		/// <returns></returns>
		IReadOnlyDictionary<string, PropertyInfo> GetSettings();
	}
}