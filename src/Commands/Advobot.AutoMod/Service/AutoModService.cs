using Advobot.AutoMod.Context;
using Advobot.AutoMod.Database;
using Advobot.AutoMod.Models;
using Advobot.AutoMod.Utils;
using Advobot.Punishments;
using Advobot.Services.Time;
using Advobot.Utilities;

using AdvorangesUtils;

using Discord;
using Discord.WebSocket;

using System.Collections.Concurrent;

namespace Advobot.AutoMod.Service;

public sealed class AutoModService
{
	private static readonly RequestOptions _AutoMod
		= DiscordUtils.GenerateRequestOptions("Auto mod.");
	private static readonly Punishment _Ban = new()
	{
		PunishmentType = PunishmentType.Ban,
	};
	private static readonly RequestOptions _BannedName
		= DiscordUtils.GenerateRequestOptions("Banned name.");
	private static readonly RequestOptions _BannedPhrase
		= DiscordUtils.GenerateRequestOptions("Banned phrase.");
	private static readonly RequestOptions _PersistentRoles
		= DiscordUtils.GenerateRequestOptions("Persistent roles.");
	private static readonly RequestOptions _RaidPrev
		= DiscordUtils.GenerateRequestOptions("Raid prevention.");
	private static readonly RequestOptions _SpamPrev
		= DiscordUtils.GenerateRequestOptions("Spam prevention.");

	private readonly IAutoModDatabase _Db;
	private readonly GuildSpecific<ulong, EnumMapped<PunishmentType, int>> _Phrases = new();
	private readonly IPunisher _Punisher;
	private readonly GuildSpecific<RaidType, HashSet<ulong>> _Raid = new();
	private readonly GuildSpecific<ulong, EnumMapped<SpamType, SortedSet<ulong>>> _Spam = new();
	private readonly ITime _Time;
	private ConcurrentDictionary<ulong, (ConcurrentBag<ulong>, ITextChannel)> _Messages = new();

	public AutoModService(
		BaseSocketClient client,
		IAutoModDatabase db,
		ITime time,
		IPunisher punisher)
	{
		_Db = db;
		_Time = time;
		_Punisher = punisher;

		client.MessageReceived += OnMessageReceived;
		client.MessageUpdated += OnMessageUpdated;
		client.UserJoined += OnUserJoined;

		_ = Task.Run(async () =>
		{
			while (true)
			{
				var messageGroups = Interlocked.Exchange(ref _Messages, new());
				foreach (var (_, (messages, channel)) in messageGroups)
				{
					try
					{
						await channel.DeleteMessagesAsync(messages, _AutoMod).CAF();
					}
					catch (Exception e)
					{
						e.Write();
					}
				}

				await Task.Delay(TimeSpan.FromSeconds(1)).CAF();
			}
		});
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
		var isAllowed = await ProcessChannelSettings(context).CAF();
		if (isSpam || isBannedPhrase || !isAllowed)
		{
			QueueMessageForDeletion(context.Channel, message);
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
#warning make this work eventually
#pragma warning disable RCS1163 // Unused parameter.
#pragma warning disable IDE0060 // Remove unused parameter
		static bool IsRaid(RaidPrevention prev, IGuildUser user)
			=> false;

		static bool ShouldPunish(RaidPrevention prev, IEnumerable<ulong> users)
			=> false;
#pragma warning restore IDE0060 // Remove unused parameter
#pragma warning restore RCS1163 // Unused parameter.

		var prevs = await _Db.GetRaidPreventionAsync(context.Guild.Id).CAF();

		var isRaid = false;
		foreach (var raidPrev in prevs)
		{
			var instances = _Raid.Get(context.Guild, raidPrev.RaidType);
			instances.Add(context.User.Id);

			if (IsRaid(raidPrev, context.User))
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
			if (!ShouldPunish(raidPrev, instances))
			{
				continue;
			}

			foreach (var instance in instances)
			{
				await PunishAsync(context.Guild, instance, raidPrev, _RaidPrev).CAF();
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
				await PunishAsync(context.Guild, context.User.Id, _Ban, _BannedName).CAF();
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
					await PunishAsync(context.Guild, context.User.Id, punishment, _BannedPhrase).CAF();
				}
			}
		}
		return true;
	}

	private async Task<bool> ProcessChannelSettings(IAutoModMessageContext context)
	{
		var settings = await _Db.GetChannelSettingsAsync(context.Channel.Id).CAF();
		if (settings is null)
		{
			return true;
		}

		return !settings.IsImageOnly || context.Message.GetImageCount() > 0;
	}

	private async Task<bool> ProcessPersistentRolesAsync(IAutoModContext context)
	{
		var persistent = await _Db.GetPersistentRolesAsync(context.Guild.Id, context.User.Id).CAF();
		if (persistent.Count == 0)
		{
			return false;
		}

		var roles = persistent
			.Select(x => context.Guild.GetRole(x.RoleId))
			.Where(x => x != null);
		await context.User.SmartAddRolesAsync(roles, _PersistentRoles).CAF();
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
				await PunishAsync(context.Guild, context.User.Id, spamPrev, _SpamPrev).CAF();
			}
		}
		return true;
	}

	private Task PunishAsync(
		IGuild guild,
		ulong userId,
		Punishment punishment,
		RequestOptions options)
	{
		var context = new DynamicPunishmentContext(guild, userId, true, punishment.PunishmentType)
		{
			RoleId = punishment.RoleId,
			Options = options,
		};
		return _Punisher.HandleAsync(context);
	}

	private void QueueMessageForDeletion(ITextChannel channel, IMessage message)
	{
		_Messages
			.GetOrAdd(message.Channel.Id, _ => (new(), channel))
			.Item1.Add(message.Id);
	}
}