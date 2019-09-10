using System.Collections.Generic;

using Advobot.Formatting;

namespace Advobot.Settings
{
	/// <summary>
	/// Abstraction for something which has settings.
	/// </summary>
	public interface ISettingsBase : ISavable
	{
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

		/// <summary>
		/// Returns the names of settings.
		/// </summary>
		/// <returns></returns>
		IReadOnlyCollection<string> GetSettingNames();

		/// <summary>
		/// Resets the value of the specified setting to its default value.
		/// </summary>
		/// <param name="name"></param>
		void ResetSetting(string name);
	}
}