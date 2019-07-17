using System.Collections.Generic;
using System.ComponentModel;
using Advobot.Classes.Formatting;

namespace Advobot.Interfaces
{
	/// <summary>
	/// Abstraction for something which has settings.
	/// </summary>
	public interface ISettingsBase : ISavable, INotifyPropertyChanged
	{
		/// <summary>
		/// Returns the names of settings.
		/// </summary>
		/// <returns></returns>
		IReadOnlyCollection<string> GetSettingNames();
		/// <summary>
		/// Formats the settings so they are readable by a human.
		/// </summary>
		/// <returns></returns>
		IDiscordFormattableString Format();
		/// <summary>
		/// Formats a specific setting.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		IDiscordFormattableString FormatSetting(string name);
		/// <summary>
		/// Formats a specific value.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		IDiscordFormattableString FormatValue(object? value);
	}
}
