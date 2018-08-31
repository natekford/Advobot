using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Advobot.Interfaces
{
	/// <summary>
	/// Abstraction for getting the type of settings this either directly is or is a factory for.
	/// </summary>
	public interface ISettingsProvider<TSettings> where TSettings : ISettingsBase
	{
		/// <summary>
		/// Gets the directory where the settings this provider provides are located.
		/// If the provider is not a factory, but instead an instance of the settings themself, this is not as useful as <see cref="ISettingsBase.GetFile(IBotDirectoryAccessor)"/>
		/// </summary>
		/// <param name="accessor"></param>
		/// <returns></returns>
		DirectoryInfo GetDirectory(IBotDirectoryAccessor accessor);
		/// <summary>
		/// Gets the settings that <typeparamref name="TSettings"/> uses.
		/// </summary>
		/// <returns></returns>
		IReadOnlyDictionary<string, PropertyInfo> GetSettings();
	}
}