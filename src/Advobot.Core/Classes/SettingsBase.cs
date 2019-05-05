using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
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
		public bool IsSetting(string name)
			=> _Settings.ContainsKey(name);
		/// <inheritdoc />
		public string[] GetSettingNames()
			=> _Settings.Keys.ToArray();
		/// <inheritdoc />
		public string Format(BaseSocketClient client, SocketGuild guild)
		{
			var sb = new StringBuilder();
			foreach (var kvp in _Settings)
			{
				var formatted = Format(client, guild, kvp.Value.GetValue(this));
				if (string.IsNullOrWhiteSpace(formatted))
				{
					continue;
				}

				sb.AppendLineFeed($"**{kvp.Key.FormatTitle()}**:");
				sb.AppendLineFeed($"{formatted}");
				sb.AppendLineFeed();
			}
			return sb.ToString();
		}
		/// <inheritdoc />
		public string FormatSetting(BaseSocketClient client, SocketGuild guild, string name)
			=> Format(client, guild, _Settings[name].GetValue(this));
		/// <inheritdoc />
		public string FormatValue(BaseSocketClient client, SocketGuild guild, object? value)
			=> Format(client, guild, value);
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
		/// <param name="backingField"></param>
		/// <param name="value"></param>
		/// <param name="condition"></param>
		/// <param name="msg"></param>
		/// <param name="caller"></param>
		protected void ThrowIfElseSet<T>(ref T backingField, T value, Func<T, bool> condition, string msg, [CallerMemberName] string caller = "")
		{
			if (condition(value))
			{
				throw new ArgumentException(msg, caller);
			}
			backingField = value;
			RaisePropertyChanged(caller);
		}
		/// <summary>
		/// Recursive function for formatting objects.
		/// </summary>
		/// <param name="client"></param>
		/// <param name="guild"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		private string Format(BaseSocketClient client, SocketGuild guild, object? value) => value switch
		{
			MemberInfo m => throw new ArgumentException($"{nameof(value)} must not be a {nameof(MemberInfo)}."),
			null => "`Nothing`",
			ulong id when guild?.GetChannel(id) is IChannel tempChannel => $"`{tempChannel.Format()}`",
			ulong id when guild?.GetRole(id) is IRole tempRole => $"`{tempRole.Format()}`",
			ulong id when guild?.GetUser(id) is IUser tempUser => $"`{tempUser.Format()}`",
			ulong id when client?.GetUser(id) is IUser tempUser => $"`{tempUser.Format()}`",
			ulong id when client?.GetGuild(id) is IGuild tempGuild => $"`{tempGuild.Format()}`",
			string str => string.IsNullOrWhiteSpace(str) ? "`Nothing`" : $"`{str}`",
			IGuildFormattable formattable => formattable.Format(guild),
			IDictionary dict => dict.Keys.Cast<object>().Join("\n", x => $"{Format(client, guild, x)}: {Format(client, guild, dict[x])}"),
			IEnumerable enumerable => enumerable.Cast<object>().Join("\n", x => Format(client, guild, x)),
			_ => $"`{value}`",
		};
	}
}