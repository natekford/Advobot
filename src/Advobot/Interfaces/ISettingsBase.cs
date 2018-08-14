using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Reflection;
using Discord.WebSocket;

namespace Advobot.Interfaces
{
	/// <summary>
	/// Abstraction for the already abstract class.
	/// </summary>
	public interface ISettingsBase
	{
		/// <summary>
		/// Returns all properties with <see cref="Classes.Attributes.SettingAttribute"/>.
		/// </summary>
		/// <returns></returns>
		IReadOnlyDictionary<string, PropertyInfo> GetSettings();
		/// <summary>
		/// Gets the file associated with the settings.
		/// </summary>
		/// <param name="accessor"></param>
		/// <returns></returns>
		FileInfo GetFile(IBotDirectoryAccessor accessor);
		/// <summary>
		/// Formats the settings so they are readable by a human.
		/// </summary>
		/// <param name="client"></param>
		/// <param name="guild"></param>
		/// <returns></returns>
		string ToString(BaseSocketClient client, SocketGuild guild);
		/// <summary>
		/// Formats a specific setting.
		/// </summary>
		/// <param name="client"></param>
		/// <param name="guild"></param>
		/// <param name="name"></param>
		/// <returns></returns>
		string ToString(BaseSocketClient client, SocketGuild guild, string name);
		/// <summary>
		/// Sets every setting back to its default value.
		/// </summary>
		void ResetSettings();
		/// <summary>
		/// Sets a setting back to its default value.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		void ResetSetting(string name);
		/// <summary>
		/// Sets a setting to the specified value.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		void SetSetting(string name, object value);
		/// <summary>
		/// Modifies a list by either adding or removing the specified value.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		/// <param name="add"></param>
		void ModifyList(string name, object value, bool add);
		/// <summary>
		/// Serializes this object and then overwrites the file.
		/// </summary>
		/// <param name="accessor">Where to save the bot files.</param>
		void SaveSettings(IBotDirectoryAccessor accessor);
	}
}
