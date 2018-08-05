using System.Collections.Generic;
using System.Reflection;
using Advobot.Classes;
using Discord.WebSocket;

namespace Advobot.Interfaces
{
	/// <summary>
	/// Abstraction for the already abstract class.
	/// </summary>
	public interface ISettingsBase
	{
		/// <summary>
		/// The name of the file.
		/// </summary>
		string FileName { get; }

		/// <summary>
		/// Returns all members with <see cref="Classes.Attributes.SettingAttribute"/>.
		/// </summary>
		/// <returns></returns>
		IReadOnlyDictionary<string, MemberInfo> GetSettings();
		/// <summary>
		/// Formats the settings so they are readable by a human.
		/// </summary>
		/// <param name="client"></param>
		/// <param name="guild"></param>
		/// <returns></returns>
		string ToString(DiscordShardedClient client, SocketGuild guild);
		/// <summary>
		/// Formats a specific setting.
		/// </summary>
		/// <param name="client"></param>
		/// <param name="guild"></param>
		/// <param name="name"></param>
		/// <returns></returns>
		string ToString(DiscordShardedClient client, SocketGuild guild, string name);
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
		void SaveSettings(LowLevelConfig config);
	}
}
