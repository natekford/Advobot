using Discord;
using System.IO;
using System.Reflection;

namespace Advobot.Core.Interfaces
{
	public interface ISettingsBase
	{
		FileInfo GetFileLocation();
		string Format(IDiscordClient client, IGuild guild);
		string Format(IDiscordClient client, IGuild guild, FieldInfo field);
		object ResetSetting(FieldInfo field);
		object ResetSetting(string name);
		void ResetSettings();
		void SaveSettings();
	}
}
