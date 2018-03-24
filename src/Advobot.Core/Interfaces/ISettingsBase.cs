using Discord;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Advobot.Core.Interfaces
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
		/// Returns all non-public instance fields with the setting attribute.
		/// </summary>
		/// <returns></returns>
		IReadOnlyDictionary<string, FieldInfo> GetSettings();
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
		/// <param name="field"></param>
		/// <returns></returns>
		string Format(IDiscordClient client, IGuild guild, FieldInfo field);
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
		/// <param name="field"></param>
		/// <returns></returns>
		object ResetSetting(FieldInfo field);
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
