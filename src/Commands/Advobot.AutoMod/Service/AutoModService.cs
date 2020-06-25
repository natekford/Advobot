using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Advobot.AutoMod.Database;
using Advobot.AutoMod.Models;
using Advobot.AutoMod.Utils;
using Advobot.Classes;
using Advobot.Services.GuildSettings.Settings;
using Advobot.Services.Time;
using Advobot.Services.Timers;
using Advobot.Utilities;

using AdvorangesUtils;

using Discord;
using Discord.WebSocket;

namespace Advobot.AutoMod.Service
{
	public sealed class AutoModService
	{
		private static readonly Punishment _Ban = new Punishment
		{
			PunishmentType = PunishmentType.Ban,
		};
		private static readonly RequestOptions _BannedName
			= DiscordUtils.GenerateRequestOptions("Banned name.");
		private static readonly RequestOptions _BannedPhrase
			= DiscordUtils.GenerateRequestOptions("Banned phrase.");
		private static readonly RequestOptions _ImageOnly
			= DiscordUtils.GenerateRequestOptions("Image only channel.");
		private static readonly RequestOptions _PersistentRoles
			= DiscordUtils.GenerateRequestOptions("Persistent roles.");
		private static readonly RequestOptions _RaidPrev
			= DiscordUtils.GenerateRequestOptions("Raid prevention.");
		private static readonly RequestOptions _SpamPrev
			= DiscordUtils.GenerateRequestOptions("Spam prevention.");

		private readonly AutoModDatabase _Db;
		private readonly GuildSpecific<ulong, EnumMapped<PunishmentType, int>> _Phrases
			= new GuildSpecific<ulong, EnumMapped<PunishmentType, int>>();
		private readonly ConcurrentDictionary<ulong, PunishmentManager> _Punishers
			= new ConcurrentDictionary<ulong, PunishmentManager>();
		private readonly GuildSpecific<RaidType, HashSet<ulong>> _Raid
			= new GuildSpecific<RaidType, HashSet<ulong>>();
		private readonly GuildSpecific<ulong, EnumMapped<SpamType, SortedSet<ulong>>> _Spam
			= new GuildSpecific<ulong, EnumMapped<SpamType, SortedSet<ulong>>>();

		private readonly ITime _Time;
		private readonly ITimerService _Timers;

		public AutoModService(
			BaseSocketClient client,
			AutoModDatabase db,
			ITime time,
			ITimerService timers)
		{
			_Db = db;
			_Time = time;
			_Timers = timers;

			client.MessageReceived += OnMessageReceived;
			client.MessageUpdated += OnMessageUpdated;
			client.UserJoined += OnUserJoined;
		}

		private PunishmentManager GetPunisher(IGuild guild)
		{
			if (!_Punishers.TryGetValue(guild.Id, out var punisher))
			{
				_Punishers.TryAdd(guild.Id, new PunishmentManager(guild, _Timers));
			}
			return punisher;
		}

		private async Task OnMessageReceived(IMessage message)
		{
			if (!(message.Author is IGuildUser user))
			{
				return;
			}

			var settings = await _Db.GetAutoModSettingsAsync(user.Guild.Id).CAF();
			var ts = _Time.UtcNow - message.CreatedAt.UtcDateTime;
			if (!await settings.ShouldScanMessageAsync(message, ts).CAF())
			{
				return;
			}

			var isSpam = await ProcessSpamAsync(message, user).CAF();
			var isBannedPhrase = await ProcessBannedPhrasesAsync(message, user).CAF();
			if (isSpam)
			{
				await message.DeleteAsync(_SpamPrev).CAF();
				return;
			}
			if (isBannedPhrase)
			{
				await message.DeleteAsync(_BannedPhrase).CAF();
				return;
			}

			var violatesChannelSettings = await ProcessChannelSettings(message, user).CAF();
			if (violatesChannelSettings)
			{
				await message.DeleteAsync(_ImageOnly).CAF();
				return;
			}
		}

		private Task OnMessageUpdated(Cacheable<IMessage, ulong> cached, IMessage message, ISocketMessageChannel channel)
			=> OnMessageReceived(message);

		private async Task OnUserJoined(IGuildUser user)
		{
			var isBannedName = await ProcessBannedNamesAsync(user).CAF();
			if (isBannedName)
			{
				return;
			}

			var isRaid = await ProcessAntiRaidAsync(user).CAF();
			if (isRaid)
			{
				return;
			}

			await ProcessPersistentRolesAsync(user).CAF();
		}

		private async Task<bool> ProcessAntiRaidAsync(IGuildUser user)
		{
			var prevs = await _Db.GetRaidPreventionAsync(user.GuildId).CAF();

			var isRaid = false;
			foreach (var raidPrev in prevs)
			{
				var instances = _Raid.Get(user.Guild, raidPrev.RaidType);
				instances.Add(user.Id);

				if (raidPrev.IsRaid(user))
				{
					isRaid = true;
				}
			}
			if (!isRaid)
			{
				return false;
			}

			var punisher = GetPunisher(user.Guild);
			foreach (var raidPrev in prevs)
			{
				var instances = _Raid.Get(user.Guild, raidPrev.RaidType);
				if (!raidPrev.ShouldPunish(instances))
				{
					continue;
				}

				foreach (var instance in instances)
				{
					var ambig = new AmbiguousUser(instance);
					await punisher.GiveAsync(raidPrev, ambig, _RaidPrev).CAF();
				}
			}
			return true;
		}

		private async Task<bool> ProcessBannedNamesAsync(IGuildUser user)
		{
			var names = await _Db.GetBannedNamesAsync(user.GuildId).CAF();
			foreach (var name in names)
			{
				if (name.IsMatch(user.Username))
				{
					var punisher = GetPunisher(user.Guild);
					var ambig = user.AsAmbiguous();
					await punisher.GiveAsync(_Ban, ambig, _BannedName).CAF();
					return true;
				}
			}
			return false;
		}

		private async Task<bool> ProcessBannedPhrasesAsync(IMessage message, IGuildUser user)
		{
			var phrases = await _Db.GetBannedPhrasesAsync(user.GuildId).CAF();
			var instances = _Phrases.Get(user.Guild, user.Id);

			var isDirty = false;
			foreach (var phrase in phrases)
			{
				if (phrase.IsMatch(message.Content))
				{
					++instances[phrase.PunishmentType];
					isDirty = true;
				}
			}
			if (!isDirty)
			{
				return false;
			}

			var punisher = GetPunisher(user.Guild);
			var ambig = user.AsAmbiguous();
			var punishments = await _Db.GetBannedPhrasePunishmentsAsync(user.GuildId).CAF();
			foreach (var punishment in punishments)
			{
				foreach (var instance in instances)
				{
					if (punishment.PunishmentType == instance.Key &&
						punishment.Instances == instance.Value)
					{
						await punisher.GiveAsync(punishment, ambig, _BannedPhrase).CAF();
					}
				}
			}
			return true;
		}

		private async Task<bool> ProcessChannelSettings(IMessage message, IGuildUser user)
		{
			var imgChannels = await _Db.GetImageOnlyChannelsAsync(user.GuildId).CAF();
			return imgChannels.Contains(message.Channel.Id) && message.GetImageCount() == 0;
		}

		private async Task<bool> ProcessPersistentRolesAsync(IGuildUser user)
		{
			var persistent = await _Db.GetPersistentRolesAsync(user.GuildId, user.Id).CAF();
			if (persistent.Count == 0)
			{
				return false;
			}

			var roles = persistent.Select(x => user.Guild.GetRole(x.RoleId));
			await user.AddRolesAsync(roles, _PersistentRoles).CAF();
			return true;
		}

		private async Task<bool> ProcessSpamAsync(IMessage message, IGuildUser user)
		{
			var prevs = await _Db.GetSpamPreventionAsync(user.GuildId).CAF();
			var instances = _Spam.Get(user.Guild, user.Id);

			var isSpam = false;
			foreach (var spamPrev in prevs)
			{
				if (spamPrev.IsSpam(message))
				{
					instances[spamPrev.SpamType].Add(message.Id);
					isSpam = true;
				}
			}
			if (!isSpam)
			{
				return false;
			}

			var punisher = GetPunisher(user.Guild);
			var ambig = user.AsAmbiguous();
			foreach (var spamPrev in prevs)
			{
				if (spamPrev.ShouldPunish(instances[spamPrev.SpamType]))
				{
					await punisher.GiveAsync(spamPrev, ambig, _SpamPrev).CAF();
				}
			}
			return true;
		}
	}
}