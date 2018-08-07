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
		ImmutableDictionary<string, PropertyInfo> GetSettings();
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
		object ResetSetting(string name);
		/// <summary>
		/// Serializes this object and then overwrites the file.
		/// </summary>
		void SaveSettings(ILowLevelConfig config);
		/// <summary>
		/// Gets the path for the file of these settings.
		/// </summary>
		/// <param name="config"></param>
		/// <returns></returns>
		FileInfo GetPath(ILowLevelConfig config);
	}
}
