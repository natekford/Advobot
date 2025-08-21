using Advobot.Levels.Database;
using Advobot.Levels.Utilities;
using Advobot.Services;
using Advobot.Services.Events;
using Advobot.Services.Time;

using Discord;

using Microsoft.Extensions.Logging;

using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;

namespace Advobot.Levels.Service;

public sealed class LevelService(
	ILogger<LevelService> logger,
	LevelServiceConfig config,
	LevelDatabase db,
	EventProvider eventProvider,
	ITimeService time
) : StartableService
{
	private readonly ConcurrentDictionary<UserKey, ConcurrentQueue<MessageHash>> _Hashes = new();
	private readonly Random _Rng = new();
	private readonly ConcurrentDictionary<UserKey, DateTimeOffset> _Time = new();

	public int CalculateLevel(int experience)
	{
		var logged = Math.Log(experience, config.Log);
		var powed = (int)Math.Pow(logged, config.Pow);
		return Math.Max(powed, 0); //No negative levels
	}

	protected override Task StartAsyncImpl()
	{
		eventProvider.MessageReceived.Add(AddExperienceAsync);
		eventProvider.MessageDeleted.Add(RemoveExperienceAsync);

		return Task.CompletedTask;
	}

	protected override Task StopAsyncImpl()
	{
		eventProvider.MessageReceived.Remove(AddExperienceAsync);
		eventProvider.MessageDeleted.Remove(RemoveExperienceAsync);

		return base.StopAsyncImpl();
	}

	private async Task AddExperienceAsync(IMessage message)
	{
		var context = await CreateXpContextAsync(message).ConfigureAwait(false);
		if (context is null)
		{
			return;
		}

		var now = time.UtcNow;
		if (_Time.TryGetValue(context.Key, out var xpLastAdded)
			&& xpLastAdded + config.WaitDuration > now)
		{
			return;
		}

		var hashes = _Hashes.GetOrAdd(context.Key, _ => []);
		var xp = CalculateExperience(context, hashes, config.BaseXp);
		var bytes = Encoding.UTF8.GetBytes(message.Content);
		var hash = BitConverter.ToString(MD5.HashData(bytes));

		hashes.Enqueue(new(xp, hash, message.Id));
		if (hashes.Count > config.CacheSize)
		{
			hashes.TryDequeue(out _);
		}
		_Time[context.Key] = now;

		var user = await db.GetUserAsync(context.SearchArgs).ConfigureAwait(false);
		var added = user.AddXp(xp);
		await db.UpsertUserAsync(added).ConfigureAwait(false);

		logger.LogDebug(
			message: "Successfully gave {Xp} xp to {User} for {Message}.",
			args: [xp, context.User.Id, context.Message.Id]
		);
	}

	private int CalculateExperience(XpContext context, IReadOnlyCollection<MessageHash> hashes, int experience)
	{
		// Be within 20% of the base value
		var xp = (double)experience * _Rng.Next(80, 120) / 100.0;
		var modifier = 1.00;

		// Message length adds up to 10% capping out at 50 characters
		var length = context.Message.Content.Length;
		modifier += Math.Min(length, 50) / 50.0 * .1;

		// Attachments/embeds remove up to 5% capping out at 5 attachments/embeds
		var attachments = context.Message.Attachments.Count;
		var embeds = context.Message.Embeds.Count;
		modifier -= Math.Min((attachments + embeds) * .01, .05);

		// Messages with the same hash are considered spam and remove up to 30% xp
		// This punishes for spam before the last message. This removes up to 30% xp.
		if (hashes.Count != 0)
		{
			if (_Rng.Next(0, 2) == 0)
			{
				var total = 0;
				var set = new HashSet<string>(hashes.Count);
				foreach (var hash in hashes)
				{
					++total;
					set.Add(hash.Hash);
				}
				modifier -= Math.Min((total - set.Count) * .1, .3);
			}
			// This punishes for spam that is the same as the last message. This removes up to 60% xp.
			else
			{
				var lastHash = "";
				var dict = new Dictionary<string, int>(hashes.Count);
				foreach (var hash in hashes)
				{
					if (!dict.TryAdd(hash.Hash, 1))
					{
						++dict[hash.Hash];
					}
					lastHash = hash.Hash;
				}
				modifier -= Math.Min((dict[lastHash] - 1) * .1, .6);
			}
		}

		return (int)Math.Round(xp * modifier);
	}

	private async Task<XpContext?> CreateXpContextAsync(IMessage? message)
	{
		if (message is not IUserMessage msg
			|| msg.Author.IsBot || msg.Author.IsWebhook
			|| msg.Channel is not ITextChannel channel
			|| channel.Guild is not IGuild guild
			|| msg.Author is not IGuildUser user)
		{
			return null;
		}

		var ignored = await db.GetIgnoredChannelsAsync(guild.Id).ConfigureAwait(false);
		if (ignored.Contains(channel.Id))
		{
			return null;
		}

		return new(channel, guild, msg, user);
	}

	private async Task RemoveExperienceAsync((IMessage? Message, ulong Id) message)
	{
		var context = await CreateXpContextAsync(message.Message).ConfigureAwait(false);
		if (context is null || !_Hashes.TryGetValue(context.Key, out var hashes))
		{
			return;
		}

		var xp = default(int?);
		var hashesArray = hashes.ToArray();
		for (var i = 0; i < hashesArray.Length; ++i)
		{
			var hash = hashesArray[i];
			if (hash.MessageId != context.Message.Id)
			{
				continue;
			}
			else if (i == hashesArray.Length - 1)
			{
				// If the most recent message is deleted, then reset the next time
				// they can get xp in addition to resetting the xp from that message
				// (Makes up for bots deleting commands automatically, etc)
				_Time.TryRemove(context.Key, out var _);
				logger.LogDebug(
					message: "Cleared xp cooldown from {User}.",
					args: [context.User.Id]
				);
			}

			xp = hash.Experience;
		}
		if (xp is null)
		{
			return;
		}

		var user = await db.GetUserAsync(context.SearchArgs).ConfigureAwait(false);
		var added = user.RemoveXp(xp.Value);
		await db.UpsertUserAsync(added).ConfigureAwait(false);

		logger.LogDebug(
			message: "Successfully removed {Xp} xp from {User} for {Message}.",
			args: [xp, context.User.Id, context.Message.Id]
		);
	}

	private readonly record struct UserKey(ulong GuildId, ulong UserId);

	private sealed record MessageHash(
		int Experience,
		string Hash,
		ulong MessageId
	);

	private sealed record XpContext(
		ITextChannel Channel,
		IGuild Guild,
		IUserMessage Message,
		IGuildUser User
	)
	{
		public UserKey Key { get; } = new(Guild.Id, User.Id);
		public SearchArgs SearchArgs => new(User.Id, Guild.Id, Channel.Id);
	}
}