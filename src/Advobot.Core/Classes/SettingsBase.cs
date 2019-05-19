using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Advobot.Interfaces;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace Advobot.Classes
{
	/// <summary>
	/// Abstract class for settings.
	/// </summary>
	internal abstract class SettingsBase : ISettingsBase
	{
		private readonly Dictionary<string, PropertyInfo> _Settings;

		public IReadOnlyCollection<string> SettingNames => _Settings.Keys.ToArray();

		/// <inheritdoc />
		public event PropertyChangedEventHandler PropertyChanged;

		public SettingsBase()
		{
#warning make setting attribute?
			_Settings = GetType().GetProperties()
				.Where(x => x.GetCustomAttribute<JsonPropertyAttribute>() != null)
				.ToDictionary(x => x.Name, x => x, StringComparer.OrdinalIgnoreCase);
		}

		/// <inheritdoc />
		public string Format(BaseSocketClient client, SocketGuild guild)
		{
			var settings = _Settings.Select(x => (x.Key, Format(client, guild, x.Value.GetValue(this))));
			return JoinFormattedSettings(settings);
		}
		/// <inheritdoc />
		public string FormatSetting(BaseSocketClient client, SocketGuild guild, string name)
			=> Format(client, guild, _Settings[name].GetValue(this));
		/// <inheritdoc />
		public string FormatValue(BaseSocketClient client, SocketGuild guild, object? value)
			=> Format(client, guild, value);
		/// <inheritdoc />
		public async Task<string> FormatAsync(IDiscordClient client, IGuild guild)
		{
			var tasks = _Settings.Select(async x => (x.Key, await FormatAsync(client, guild, x.Value.GetValue(this)).CAF()));
			var settings = await Task.WhenAll(tasks).CAF();
			return JoinFormattedSettings(settings);
		}
		/// <inheritdoc />
		public Task<string> FormatSettingAsync(IDiscordClient client, IGuild guild, string name)
		{
			if (client is BaseSocketClient socketClient && guild is SocketGuild socketGuild)
			{
				return Task.FromResult(FormatSetting(socketClient, socketGuild, name));
			}
			return FormatAsync(client, guild, _Settings[name].GetValue(this));
		}
		/// <inheritdoc />
		public Task<string> FormatValueAsync(IDiscordClient client, IGuild guild, object? value)
		{
			if (client is BaseSocketClient socketClient && guild is SocketGuild socketGuild)
			{
				return Task.FromResult(FormatValue(socketClient, socketGuild, value));
			}
			return FormatAsync(client, guild, value);
		}
		/// <inheritdoc />
		public void Save(IBotDirectoryAccessor accessor)
			=> IOUtils.SafeWriteAllText(GetFile(accessor), IOUtils.Serialize(this));
		/// <inheritdoc />
		public abstract FileInfo GetFile(IBotDirectoryAccessor accessor);
		/// <inheritdoc />
		protected void RaisePropertyChanged([CallerMemberName] string caller = "")
			=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(caller));
		/// <summary>
		/// Sets the field and raises property changed.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="field"></param>
		/// <param name="value"></param>
		/// <param name="caller"></param>
		protected void SetValue<T>(ref T field, T value, [CallerMemberName] string caller = "")
		{
			field = value;
			RaisePropertyChanged(caller);
		}
		/// <summary>
		/// Throws an argument exception if the condition is true.
		/// </summary>
		/// <param name="field"></param>
		/// <param name="value"></param>
		/// <param name="condition"></param>
		/// <param name="msg"></param>
		/// <param name="caller"></param>
		protected void ThrowIfElseSet<T>(ref T field, T value, Func<T, bool> condition, string msg, [CallerMemberName] string caller = "")
		{
			if (condition(value))
			{
				throw new ArgumentException(msg, caller);
			}
			field = value;
			RaisePropertyChanged(caller);
		}
		private string JoinFormattedSettings(IEnumerable<(string Name, string FormattedValue)> settings)
		{
			var sb = new StringBuilder();
			foreach (var (Name, FormattedValue) in settings)
			{
				if (string.IsNullOrWhiteSpace(FormattedValue))
				{
					continue;
				}

				sb.AppendLineFeed($"**{Name.FormatTitle()}**:");
				sb.AppendLineFeed(FormattedValue);
				sb.AppendLineFeed();
			}
			return sb.ToString();
		}
		private string Format(BaseSocketClient client, SocketGuild guild, object? value) => value switch
		{
			object obj when IsCommonFormatting(obj, out var s) => s,
			ulong id when guild?.GetChannel(id) is IChannel tempChannel => $"`{tempChannel.Format()}`",
			ulong id when guild?.GetRole(id) is IRole tempRole => $"`{tempRole.Format()}`",
			ulong id when guild?.GetUser(id) is IUser tempUser => $"`{tempUser.Format()}`",
			ulong id when client?.GetUser(id) is IUser tempUser => $"`{tempUser.Format()}`",
			ulong id when client?.GetGuild(id) is IGuild tempGuild => $"`{tempGuild.Format()}`",
			IGuildFormattable formattable => formattable.Format(guild),
			IDictionary dict => dict.Keys.Cast<object>().Join("\n", x => $"{Format(client, guild, x)}: {Format(client, guild, dict[x])}"),
			IEnumerable enumerable => enumerable.Cast<object>().Join("\n", x => Format(client, guild, x)),
			_ => $"`{value}`",
		};
		private async Task<string> FormatAsync(IDiscordClient client, IGuild guild, object? value) => value switch
		{
			object obj when IsCommonFormatting(obj, out var s) => s,
			ulong id when await guild?.GetChannelAsync(id) is IChannel tempChannel => $"`{tempChannel.Format()}`",
			ulong id when guild?.GetRole(id) is IRole tempRole => $"`{tempRole.Format()}`",
			ulong id when await guild?.GetUserAsync(id) is IUser tempUser => $"`{tempUser.Format()}`",
			ulong id when await client?.GetUserAsync(id) is IUser tempUser => $"`{tempUser.Format()}`",
			ulong id when await client?.GetGuildAsync(id) is IGuild tempGuild => $"`{tempGuild.Format()}`",
			//IGuildFormattable formattable => formattable.Format(guild),
			IDictionary dict => await FormatDictionaryAsync(dict, client, guild).CAF(),
			IEnumerable enumerable => await FormatEnumerableAsync(enumerable, client, guild).CAF(),
			_ => $"`{value}`",
		};
		private bool IsCommonFormatting(object? value, out string? response)
		{
			response = value switch
			{
				null => "`Nothing`",
				string s => string.IsNullOrWhiteSpace(s) ? "`Nothing`" : $"`{s}`",
				MemberInfo _ => throw new ArgumentException($"{nameof(value)} must not be a {nameof(MemberInfo)}."),
				_ => null,
			};
			return response != null;
		}
		private async Task<string> FormatDictionaryAsync(IDictionary dict, IDiscordClient client, IGuild guild)
		{
			var parts = new string[dict.Count];
			var index = 0;
			foreach (var key in dict.Keys)
			{
				parts[index++] = await FormatAsync(client, guild, dict[key]).CAF();
			}
			return parts.Join("\n");
		}
		private async Task<string> FormatEnumerableAsync(IEnumerable e, IDiscordClient client, IGuild guild)
		{
			var parts = new List<string>();
			foreach (var value in e)
			{
				parts.Add(await FormatAsync(client, guild, value).CAF());
			}
			return parts.Join("\n");
		}
	}
}