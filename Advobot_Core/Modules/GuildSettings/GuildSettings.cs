using Advobot.Actions;
using Advobot.Interfaces;
using Advobot.Classes;
using Discord;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Advobot.Modules.GuildSettings
{
	public sealed class MyGuildSettingsModule : IGuildSettingsModule
	{
		private readonly Dictionary<ulong, IGuildSettings> _GuildSettings = new Dictionary<ulong, IGuildSettings>();
		private readonly Type _GuildSettingsType;

		public MyGuildSettingsModule(Type guildSettingsType)
		{
			_GuildSettingsType = guildSettingsType;
		}

		public async Task AddGuild(IGuild guild)
		{
			_GuildSettings.Add(guild.Id, await CreateGuildSettings(_GuildSettingsType, guild));
		}
		public Task RemoveGuild(ulong guildId)
		{
			if (_GuildSettings.ContainsKey(guildId))
			{
				_GuildSettings.Remove(guildId);
			}
			return Task.FromResult(0);
		}
		public IGuildSettings GetSettings(ulong guildId)
		{
			return _GuildSettings[guildId];
		}
		public IEnumerable<IGuildSettings> GetAllSettings()
		{
			return _GuildSettings.Values;
		}
		public bool TryGetSettings(ulong guildId, out IGuildSettings settings)
		{
			return _GuildSettings.TryGetValue(guildId, out settings);
		}
		public bool ContainsGuild(ulong guildId)
		{
			return _GuildSettings.ContainsKey(guildId);
		}

		private async Task<IGuildSettings> CreateGuildSettings(Type guildSettingsType, IGuild guild)
		{
			if (_GuildSettings.TryGetValue(guild.Id, out IGuildSettings guildSettings))
			{
				return guildSettings;
			}

			var fileInfo = GetActions.GetServerDirectoryFile(guild.Id, Constants.GUILD_SETTINGS_LOCATION);
			if (fileInfo.Exists)
			{
				try
				{
					using (var reader = new StreamReader(fileInfo.FullName))
					{
						guildSettings = (IGuildSettings)JsonConvert.DeserializeObject(reader.ReadToEnd(), guildSettingsType);
					}
					ConsoleActions.WriteLine($"The guild information for {guild.FormatGuild()} has successfully been loaded.");
				}
				catch (Exception e)
				{
					ConsoleActions.ExceptionToConsole(e);
				}
			}
			else
			{
				ConsoleActions.WriteLine($"The guild information file for {guild.FormatGuild()} could not be found; using default.");
			}
			guildSettings = guildSettings ?? (IGuildSettings)Activator.CreateInstance(guildSettingsType);

			var unsetCmdSwitches = Constants.HELP_ENTRIES.Where(x => !guildSettings.CommandSwitches.Select(y => y.Name).CaseInsContains(x.Name)).Select(x => new CommandSwitch(x.Name, x.DefaultEnabled));
			guildSettings.CommandSwitches.AddRange(unsetCmdSwitches);
			guildSettings.CommandSwitches.RemoveAll(x => String.IsNullOrWhiteSpace(x.Name));
			guildSettings.CommandsDisabledOnUser.RemoveAll(x => String.IsNullOrWhiteSpace(x.Name));
			guildSettings.CommandsDisabledOnRole.RemoveAll(x => String.IsNullOrWhiteSpace(x.Name));
			guildSettings.CommandsDisabledOnChannel.RemoveAll(x => String.IsNullOrWhiteSpace(x.Name));
			guildSettings.Invites.AddRange((await InviteActions.GetInvites(guild)).Select(x => new BotInvite(x.Code, x.Uses)));

			if (guildSettings is MyGuildSettings)
			{
				(guildSettings as MyGuildSettings).PostDeserialize(guild);
			}

			return guildSettings;
		}
	}
}
