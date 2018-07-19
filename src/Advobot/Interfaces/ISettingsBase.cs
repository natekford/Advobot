using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Discord;

namespace Advobot.Interfaces
{
	/// <summary>
	/// Abstraction for the already abstract class.
	/// </summary>
	public interface ISettingsBase
	{
		/// <summary>
		/// The location to save the serialized file.
		/// </summary>
		FileInfo FileLocation { get; }

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
		string Format(IDiscordClient client, IGuild guild);
		/// <summary>
		/// Formats a specific setting.
		/// </summary>
		/// <param name="client"></param>
		/// <param name="guild"></param>
		/// <param name="name"></param>
		/// <returns></returns>
		string Format(IDiscordClient client, IGuild guild, string name);
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
		void SaveSettings();
	}
}
