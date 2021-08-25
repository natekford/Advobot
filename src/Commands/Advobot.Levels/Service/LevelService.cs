﻿using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;

using Advobot.Levels.Database;
using Advobot.Levels.Utilities;
using Advobot.Services.Time;
using Advobot.Utilities;

using AdvorangesUtils;

using Discord;
using Discord.WebSocket;

namespace Advobot.Levels.Service
{
	public sealed class LevelService : ILevelService
	{
		private readonly BaseSocketClient _Client;
		private readonly LevelServiceConfig _Config;
		private readonly ILevelDatabase _Db;
		private readonly ConcurrentDictionary<Key, RuntimeInfo> _RuntimeInfo = new();
		private readonly ITime _Time;

		public LevelService(
			LevelServiceConfig config,
			ILevelDatabase db,
			BaseSocketClient client,
			ITime time)
		{
			_Config = config;
			_Client = client;
			_Db = db;
			_Time = time;

			_Client.MessageReceived += AddExperienceAsync;
			_Client.MessageDeleted += RemoveExperienceAsync;
		}

		public int CalculateLevel(int experience)
		{
			var logged = Math.Log(experience, _Config.Log);
			var powed = (int)Math.Pow(logged, _Config.Pow);
			return Math.Max(powed, 0); //No negative levels
		}

		private async Task AddExperienceAsync(IMessage message)
		{
			var context = await XpContext.CreateAsync(_Db, message).CAF();
			if (context == null)
			{
				return;
			}

			var runtimeInfo = _RuntimeInfo.GetOrAdd(context.Key, _ => new RuntimeInfo(_Time, _Config));
			var xp = CalculateExperience(context, runtimeInfo, _Config.BaseXp);
			if (!runtimeInfo.TryAdd(context.Message, context.Channel, xp))
			{
				return;
			}

			var user = await _Db.GetUserAsync(context.CreateArgs()).CAF();
			var added = user.AddXp(xp);
			await _Db.UpsertUserAsync(added).CAF();

			ConsoleUtils.DebugWrite($"Successfully gave {xp} xp to {context.User.Format()}.");
		}

		private int CalculateExperience(XpContext context, RuntimeInfo info, int experience)
		{
			var rng = new Random();
			//Be within 20% of the base value
			var xp = (double)experience * rng.Next(80, 120) / 100.0;
			//Message length adds up to 10% increase capping out at 50 characters (any higher = same)
			//Reason: Some people just spam short messages for xp and this incentivizes longer messages which indicates better convos
			var length = context.Message.Content.Length;
			var lengthFactor = 1 + (Math.Min(length, 50) / 50.0 * .1);
			//Attachments/embeds remove up to 5% of the xp capping out at 5 attachments/embeds
			//Reason: Marginally disincentivizes lots of images which discourage conversation
			var attachments = context.Message.Attachments.Count;
			var embeds = context.Message.Embeds.Count;
			var attachmentFactor = 1 - Math.Min((attachments + embeds) * .01, .05);
			//Any messages with the same hash are considered spam and remove up to 60% of the xp
			//Reason: Disincentivizes spamming which greatly discourages conversation
			//The first punishes for spam that was said before and during the last message. This only takes off up to 30% of the xp.
			//The second punishes for spam that is the same as the last message sent. This takes off up to 60% of the xp.
			var hashes = info.Messages;
			var spamFactor = rng.Next(0, 2) != 0
				? 1 - Math.Min((hashes.Count - hashes.Select(x => x.Hash).Distinct().Count()) * .1, .3)
				: 1 - Math.Min((hashes.Count(x => x.Hash == hashes[hashes.Count - 1].Hash) - 1) * .1, .6);
			return (int)Math.Round(xp * lengthFactor * attachmentFactor * spamFactor);
		}

		private async Task RemoveExperienceAsync(
			Cacheable<IMessage, ulong> cached,
			Cacheable<IMessageChannel, ulong> _)
		{
			var context = await XpContext.CreateAsync(_Db, cached.Value).CAF();
			if (context == null
				|| !_RuntimeInfo.TryGetValue(context.Key, out var info)
				|| !info.TryGet(cached.Id, out var hash))
			{
				return;
			}

			var user = await _Db.GetUserAsync(context.CreateArgs()).CAF();
			var xp = hash.Experience;
			var added = user.RemoveXp(xp);
			await _Db.UpsertUserAsync(added).CAF();

			ConsoleUtils.DebugWrite($"Successfully removed {xp} xp from {context.User.Format()}.");
		}

		private readonly struct Key
		{
			public ulong GuildId { get; }
			public ulong UserId { get; }

			public Key(IGuildUser user)
			{
				GuildId = user.Guild.Id;
				UserId = user.Id;
			}
		}

		private sealed class MessageHash
		{
			public ulong ChannelId { get; }
			public int Experience { get; }
			public ulong GuildId { get; }
			public string Hash { get; }
			public ulong MessageId { get; }

			public MessageHash(IMessage message, ITextChannel channel, int xp)
			{
				GuildId = channel.Guild.Id;
				ChannelId = channel.Id;
				MessageId = message.Id;
				using (var md5 = MD5.Create())
				{
					var bytes = Encoding.UTF8.GetBytes(message.Content);
					var hashed = md5.ComputeHash(bytes);
					Hash = BitConverter.ToString(hashed).Replace("-", "").ToLower();
				}
				Experience = xp;
			}
		}

		private sealed class RuntimeInfo
		{
			private readonly LevelServiceConfig _Config;
			private readonly ConcurrentQueue<MessageHash> _Messages = new();
			private readonly ITime _Time;

			public IReadOnlyList<MessageHash> Messages => _Messages.ToArray();
			public DateTimeOffset Time { get; private set; } = DateTimeOffset.MinValue;

			public RuntimeInfo(ITime time, LevelServiceConfig config)
			{
				_Time = time;
				_Config = config;
			}

			public bool TryAdd(IUserMessage message, ITextChannel channel, int xp)
			{
				if (Time + _Config.WaitTime > _Time.UtcNow)
				{
					return false;
				}

				_Messages.Enqueue(new MessageHash(message, channel, xp));
				if (_Messages.Count > _Config.CacheSize)
				{
					_Messages.TryDequeue(out _);
				}
				Time = _Time.UtcNow;
				return true;
			}

			public bool TryGet(ulong id, out MessageHash hash)
			{
				var stored = Messages;
				for (var i = 0; i < stored.Count; ++i)
				{
					var message = stored[i];
					if (message.MessageId != id)
					{
						continue;
					}
					else if (i == 0)
					{
						//If the most recent message is deleted, then reset the next time
						//they can get xp in addition to resetting the xp from that message
						//(Makes up for bots deleting commands automatically, etc)
						Time = _Time.UtcNow - _Config.WaitTime;
					}

					hash = message;
					return true;
				}
				hash = default!;
				return false;
			}
		}

		private sealed class XpContext
		{
			public ITextChannel Channel { get; }
			public IGuild Guild { get; }
			public Key Key => new(User);
			public IUserMessage Message { get; }
			public IGuildUser User { get; }

			private XpContext(
				IGuild guild,
				ITextChannel channel,
				IGuildUser user,
				IUserMessage message)
			{
				Guild = guild;
				Channel = channel;
				User = user;
				Message = message;
			}

			public static async Task<XpContext?> CreateAsync(
				ILevelDatabase db,
				IMessage message)
			{
				if (message is not IUserMessage msg
					|| msg.Author.IsBot || msg.Author.IsWebhook
					|| msg.Channel is not ITextChannel channel
					|| channel.Guild is not IGuild guild
					|| msg.Author is not IGuildUser user)
				{
					return null;
				}

				var ignored = await db.GetIgnoredChannelsAsync(guild.Id).CAF();
				if (ignored.Contains(channel.Id))
				{
					return null;
				}

				return new(guild, channel, user, msg);
			}

			public SearchArgs CreateArgs()
				=> new(User.Id, Guild.Id, Channel.Id);
		}
	}
}