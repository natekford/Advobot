using System.ComponentModel;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace Advobot.Interfaces
{
	/// <summary>
	/// Abstraction for something which has settings.
	/// </summary>
	public interface ISettingsBase : ISavable, INotifyPropertyChanged
	{
		/// <summary>
		/// Formats the settings so they are readable by a human.
		/// </summary>
		/// <param name="client"></param>
		/// <param name="guild"></param>
		/// <returns></returns>
		string Format(BaseSocketClient client, SocketGuild guild);
		/// <summary>
		/// Formats a specific setting.
		/// </summary>
		/// <param name="client"></param>
		/// <param name="guild"></param>
		/// <param name="name"></param>
		/// <returns></returns>
		string FormatSetting(BaseSocketClient client, SocketGuild guild, string name);
		/// <summary>
		/// Formats a specific value.
		/// </summary>
		/// <param name="client"></param>
		/// <param name="guild"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		string FormatValue(BaseSocketClient client, SocketGuild guild, object? value);
		/// <summary>
		/// Formats the settings asynchronously so they are readable by a human.
		/// </summary>
		/// <param name="client"></param>
		/// <param name="guild"></param>
		/// <returns></returns>
		Task<string> FormatAsync(IDiscordClient client, IGuild guild);
		/// <summary>
		/// Formats a specific setting asynchronously.
		/// </summary>
		/// <param name="client"></param>
		/// <param name="guild"></param>
		/// <param name="name"></param>
		/// <returns></returns>
		Task<string> FormatSettingAsync(IDiscordClient client, IGuild guild, string name);
		/// <summary>
		/// Formats a specific value asynchronously.
		/// </summary>
		/// <param name="client"></param>
		/// <param name="guild"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		Task<string> FormatValueAsync(IDiscordClient client, IGuild guild, object? value);
		/// <summary>
		/// Returns true if the supplied name is the name of a setting.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		bool IsSetting(string name);
		/// <summary>
		/// Returns the names of settings.
		/// </summary>
		/// <returns></returns>
		string[] GetSettingNames();
	}
}
