using Discord;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Advobot.Core.Interfaces
{
	public interface ISettingsBase
	{
		FileInfo FileLocation { get; }

		IReadOnlyDictionary<string, FieldInfo> GetSettings();
		string Format(IDiscordClient client, IGuild guild);
		string Format(IDiscordClient client, IGuild guild, FieldInfo field);
		string Format(IDiscordClient client, IGuild guild, string name);
		void ResetSettings();
		object ResetSetting(FieldInfo field);
		object ResetSetting(string name);
		void SaveSettings();
	}
}
