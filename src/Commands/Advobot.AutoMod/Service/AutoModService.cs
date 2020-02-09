using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Advobot.AutoMod.Models;
using Advobot.Services.GuildSettings.Settings;
using Advobot.Services.Time;
using Advobot.Services.Timers;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using Discord.WebSocket;

namespace Advobot.AutoMod.Service
{
	public interface IAutoModDatabase
	{
		Task<AutoModSettings> GetAutoModSettingsAsync(ulong id);
	}

	public sealed class AutoModService
	{
		private readonly IAutoModDatabase _Db;

		private readonly GuildSpecific<ulong, EnumMapped<Punishment, int>> _Phrases
			= new GuildSpecific<ulong, EnumMapped<Punishment, int>>();

		private readonly ConcurrentDictionary<ulong, PunishmentManager> _Punishers
			= new ConcurrentDictionary<ulong, PunishmentManager>();

		private readonly GuildSpecific<RaidType, SortedSet<ulong>> _Raid
			= new GuildSpecific<RaidType, SortedSet<ulong>>();

		private readonly GuildSpecific<ulong, EnumMapped<SpamType, SortedSet<ulong>>> _Spam
			= new GuildSpecific<ulong, EnumMapped<SpamType, SortedSet<ulong>>>();

		private readonly ITime _Time;
		private readonly ITimerService _Timers;

		public AutoModService(
			BaseSocketClient client,
			IAutoModDatabase db,
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

		private async Task HandleBannedPhrases(IMessage message, IGuildUser user)
		{
		}

		private async Task HandleSpam(IMessage message, IGuildUser user)
		{
			static int GetSpamCount(SpamType type, IMessage message) => type switch
			{
				SpamType.Message => int.MaxValue,
				SpamType.LongMessage => message.Content?.Length ?? 0,
				SpamType.Link => message.Content?.Split(' ')?.Count(x => Uri.IsWellFormedUriString(x, UriKind.Absolute)) ?? 0,
				SpamType.Image => message.Attachments.Count(x => x.Height != null || x.Width != null) + message.Embeds.Count(x => x.Image != null || x.Video != null),
				SpamType.Mention => message.MentionedUserIds.Distinct().Count(),
				_ => throw new ArgumentOutOfRangeException(nameof(Type)),
			};

			var TEMP = new List<IReadOnlySpamPrevention>();

			var instances = _Spam.Get(user.Guild, user.Id);
			foreach (var prev in TEMP)
			{
				if (GetSpamCount(prev))
			}
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
		}

		private Task OnMessageUpdated(Cacheable<IMessage, ulong> cached, IMessage message, ISocketMessageChannel channel)
			=> OnMessageReceived(message);

		private Task OnUserJoined(IGuildUser user)
		{
		}
	}

	public sealed class EnumMapped<TEnum, TValue>
		where TEnum : Enum
		where TValue : new()
	{
		private static readonly TEnum[] _Values
			= Enum.GetValues(typeof(TEnum)).Cast<TEnum>().ToArray();

		private readonly Dictionary<TEnum, TValue> _Dict
			= new Dictionary<TEnum, TValue>();

		public TValue this[TEnum key]
			=> _Dict[key];

		public EnumMapped()
		{
			foreach (var value in _Values)
			{
				_Dict[value] = new TValue();
			}
		}

		public void Reset(TEnum key)
			=> _Dict[key] = new TValue();

		public void ResetAll()
		{
			foreach (var value in _Values)
			{
				_Dict[value] = new TValue();
			}
		}

		public void Update(TEnum key, TValue value)
			=> _Dict[key] = value;

		public void Update(TEnum key, Func<TValue, TValue> updater)
			=> Update(key, updater(_Dict[key]));
	}

	public sealed class GuildSpecific<TKey, TValue>
		where TValue : new()
	{
		private readonly ConcurrentDictionary<ulong, ConcurrentDictionary<TKey, TValue>> _Dict
			= new ConcurrentDictionary<ulong, ConcurrentDictionary<TKey, TValue>>();

		public TValue Get(IGuild guild, TKey key)
		{
			return _Dict
				.GetOrAdd(guild.Id, _ => new ConcurrentDictionary<TKey, TValue>())
				.GetOrAdd(key, _ => new TValue());
		}

		public void Reset(IGuild guild)
			=> _Dict.TryRemove(guild.Id, out _);
	}
}