using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Advobot.AutoMod.Context;
using Advobot.AutoMod.Database;
using Advobot.AutoMod.Models;
using Advobot.AutoMod.ReadOnlyModels;
using Advobot.AutoMod.Utils;
using Advobot.Classes;
using Advobot.Services.GuildSettings.Settings;
using Advobot.Services.Time;
using Advobot.Services.Timers;
using Advobot.Utilities;

using AdvorangesSettingParser.Implementation.Instance;

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

		private async Task OnMessageReceived(IMessage message)
		{
			var context = message.CreateContext();
			if (context is null)
			{
				return;
			}

			var settings = await _Db.GetAutoModSettingsAsync(context.Guild.Id).CAF();
			var ts = _Time.UtcNow - message.CreatedAt.UtcDateTime;
			if (!await settings.ShouldScanMessageAsync(message, ts).CAF())
			{
				return;
			}

			var isSpam = await ProcessSpamAsync(context).CAF();
			var isBannedPhrase = await ProcessBannedPhrasesAsync(context).CAF();
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

			var isAllowed = await ProcessChannelSettings(context).CAF();
			if (!isAllowed)
			{
				await message.DeleteAsync(_ImageOnly).CAF();
				return;
			}
		}

		private Task OnMessageUpdated(Cacheable<IMessage, ulong> cached, IMessage message, ISocketMessageChannel channel)
			=> OnMessageReceived(message);

		private async Task OnUserJoined(IGuildUser user)
		{
			var context = user.CreateContext();
			if (context is null)
			{
				return;
			}

			var isBannedName = await ProcessBannedNamesAsync(context).CAF();
			if (isBannedName)
			{
				return;
			}

			var isRaid = await ProcessAntiRaidAsync(context).CAF();
			if (isRaid)
			{
				return;
			}

			await ProcessPersistentRolesAsync(context).CAF();
		}

		private async Task<bool> ProcessAntiRaidAsync(IAutoModContext context)
		{
			var prevs = await _Db.GetRaidPreventionAsync(context.Guild.Id).CAF();

			var isRaid = false;
			foreach (var raidPrev in prevs)
			{
				var instances = _Raid.Get(context.Guild, raidPrev.RaidType);
				instances.Add(context.User.Id);

				if (raidPrev.IsRaid(context.User))
				{
					isRaid = true;
				}
			}
			if (!isRaid)
			{
				return false;
			}

			foreach (var raidPrev in prevs)
			{
				var instances = _Raid.Get(context.Guild, raidPrev.RaidType);
				if (!raidPrev.ShouldPunish(instances))
				{
					continue;
				}

				foreach (var instance in instances)
				{
					await PunishAsync(context.Guild, instance.AsAmbiguous(), raidPrev, _RaidPrev).CAF();
				}
			}
			return true;
		}

		private async Task<bool> ProcessBannedNamesAsync(IAutoModContext context)
		{
			var names = await _Db.GetBannedNamesAsync(context.Guild.Id).CAF();
			foreach (var name in names)
			{
				if (name.IsMatch(context.User.Username))
				{
					await PunishAsync(context, _Ban, _BannedName).CAF();
					return true;
				}
			}
			return false;
		}

		private async Task<bool> ProcessBannedPhrasesAsync(IAutoModMessageContext context)
		{
			var phrases = await _Db.GetBannedPhrasesAsync(context.Guild.Id).CAF();
			var instances = _Phrases.Get(context.Guild, context.User.Id);

			var isDirty = false;
			foreach (var phrase in phrases)
			{
				if (phrase.IsMatch(context.Message.Content))
				{
					++instances[phrase.PunishmentType];
					isDirty = true;
				}
			}
			if (!isDirty)
			{
				return false;
			}

			var punishments = await _Db.GetPunishmentsAsync(context.Guild.Id).CAF();
			foreach (var punishment in punishments)
			{
				foreach (var instance in instances)
				{
					if (punishment.PunishmentType == instance.Key &&
						punishment.Instances == instance.Value)
					{
						await PunishAsync(context, punishment, _BannedPhrase).CAF();
					}
				}
			}
			return true;
		}

		private async Task<bool> ProcessChannelSettings(IAutoModMessageContext context)
		{
			var settings = await _Db.GetChannelSettingsAsync(context.Channel.Id).CAF();
			return settings?.IsAllowed(context.Message) != false;
		}

		private async Task<bool> ProcessPersistentRolesAsync(IAutoModContext context)
		{
			var persistent = await _Db.GetPersistentRolesAsync(context.Guild.Id, context.User.Id).CAF();
			if (persistent.Count == 0)
			{
				return false;
			}

			var roles = persistent.Select(x => context.Guild.GetRole(x.RoleId));
			await context.User.AddRolesAsync(roles, _PersistentRoles).CAF();
			return true;
		}

		private async Task<bool> ProcessSpamAsync(IAutoModMessageContext context)
		{
			var prevs = await _Db.GetSpamPreventionAsync(context.Guild.Id).CAF();
			var instances = _Spam.Get(context.Guild, context.User.Id);

			var isSpam = false;
			foreach (var spamPrev in prevs)
			{
				if (spamPrev.IsSpam(context.Message))
				{
					instances[spamPrev.SpamType].Add(context.Message.Id);
					isSpam = true;
				}
			}
			if (!isSpam)
			{
				return false;
			}

			foreach (var spamPrev in prevs)
			{
				if (spamPrev.ShouldPunish(instances[spamPrev.SpamType]))
				{
					await PunishAsync(context, spamPrev, _SpamPrev).CAF();
				}
			}
			return true;
		}

		private Task PunishAsync(
			IAutoModContext context,
			IReadOnlyPunishment punishment,
			RequestOptions options)
			=> PunishAsync(context.Guild, context.User.AsAmbiguous(), punishment, options);

		private Task PunishAsync(
			IGuild guild,
			AmbiguousUser user,
			IReadOnlyPunishment punishment,
			RequestOptions options)
		{
			var punisher = _Punishers.GetOrAdd(guild.Id, _ => new PunishmentManager(guild, _Timers));
			return punisher.GiveAsync(punishment, user, options);
		}
	}
}