using Advobot.AutoMod.Database;
using Advobot.AutoMod.Models;
using Advobot.AutoMod.Utils;
using Advobot.Punishments;
using Advobot.Services;
using Advobot.Services.Time;
using Advobot.Utilities;

using Discord;
using Discord.WebSocket;

using Microsoft.Extensions.Logging;

using System.Collections.Concurrent;

namespace Advobot.AutoMod.Service;

public sealed class AutoModService(
	ILogger<AutoModService> logger,
	BaseSocketClient client,
	IAutoModDatabase db,
	ITimeService time,
	IPunishmentService punishmentService
) : IStartableService, IConfigurableService
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

	private readonly GuildSpecific<ulong, EnumMapped<PunishmentType, int>> _Phrases = new();

	private bool _IsRunning;
	private ConcurrentDictionary<ulong, (ConcurrentBag<ulong>, ITextChannel)> _Messages = new();

	public void Start()
	{
		if (_IsRunning)
		{
			return;
		}
		_IsRunning = true;

		client.MessageReceived += OnMessageReceived;
		client.MessageUpdated += OnMessageUpdated;
		client.UserJoined += OnUserJoined;

		_ = Task.Run(async () =>
		{
			while (_IsRunning)
			{
				var messageGroups = Interlocked.Exchange(ref _Messages, []);
				foreach (var (_, (messageIds, channel)) in messageGroups)
				{
					try
					{
						await channel.DeleteMessagesAsync(messageIds, _AutoMod).ConfigureAwait(false);
					}
					catch (Exception e)
					{
						logger.LogWarning(
							exception: e,
							message: "Exception occurred while deleting messages. {@Info}",
							args: new
							{
								Guild = channel.GuildId,
								Channel = channel.Id,
								Messages = messageIds,
							}
						);
					}
				}

				await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);
			}
		});
	}

	public void Stop()
	{
		client.MessageReceived -= OnMessageReceived;
		client.MessageUpdated -= OnMessageUpdated;
		client.UserJoined -= OnUserJoined;

		_IsRunning = false;
	}

	Task IConfigurableService.ConfigureAsync()
	{
		Start();
		return Task.CompletedTask;
	}

	private async Task OnMessageReceived(IMessage message)
	{
		var context = AutoModMessageContext.FromMessage(message);
		if (context is null)
		{
			return;
		}

		var settings = await db.GetAutoModSettingsAsync(context.Guild.Id).ConfigureAwait(false);
		var ts = time.UtcNow - message.CreatedAt.UtcDateTime;
		if (!await settings.ShouldScanMessageAsync(message, ts).ConfigureAwait(false))
		{
			return;
		}

		var isBannedPhrase = await ProcessBannedPhrasesAsync(context).ConfigureAwait(false);
		var isAllowed = await ProcessChannelSettings(context).ConfigureAwait(false);
		if (isBannedPhrase || !isAllowed)
		{
			_Messages
				.GetOrAdd(message.Channel.Id, _ => ([], context.Channel))
				.Item1.Add(message.Id);
		}
	}

	private Task OnMessageUpdated(Cacheable<IMessage, ulong> cached, IMessage message, ISocketMessageChannel channel)
		=> OnMessageReceived(message);

	private async Task OnUserJoined(IGuildUser user)
	{
		var context = new AutoModContext(user);
		if (context is null)
		{
			return;
		}

		var isBannedName = await ProcessBannedNamesAsync(context).ConfigureAwait(false);
		if (isBannedName)
		{
			return;
		}

		await ProcessPersistentRolesAsync(context).ConfigureAwait(false);
	}

	private async Task<bool> ProcessBannedNamesAsync(AutoModContext context)
	{
		var names = await db.GetBannedNamesAsync(context.Guild.Id).ConfigureAwait(false);
		foreach (var name in names)
		{
			if (name.IsMatch(context.User.Username))
			{
				await PunishAsync(context.Guild, context.User.Id, _Ban, _BannedName).ConfigureAwait(false);
				return true;
			}
		}
		return false;
	}

	private async Task<bool> ProcessBannedPhrasesAsync(AutoModMessageContext context)
	{
		var phrases = await db.GetBannedPhrasesAsync(context.Guild.Id).ConfigureAwait(false);
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

		var punishments = await db.GetPunishmentsAsync(context.Guild.Id).ConfigureAwait(false);
		foreach (var punishment in punishments)
		{
			foreach (var instance in instances)
			{
				if (punishment.PunishmentType == instance.Key &&
					punishment.Instances == instance.Value)
				{
					await PunishAsync(context.Guild, context.User.Id, punishment, _BannedPhrase).ConfigureAwait(false);
				}
			}
		}
		return true;
	}

	private async Task<bool> ProcessChannelSettings(AutoModMessageContext context)
	{
		var settings = await db.GetChannelSettingsAsync(context.Channel.Id).ConfigureAwait(false);
		if (settings is null)
		{
			return true;
		}

		return !settings.IsImageOnly || context.Message.GetImageCount() > 0;
	}

	private async Task<bool> ProcessPersistentRolesAsync(AutoModContext context)
	{
		var persistent = await db.GetPersistentRolesAsync(context.Guild.Id, context.User.Id).ConfigureAwait(false);
		if (persistent.Count == 0)
		{
			return false;
		}

		var roles = persistent
			.Select(x => context.Guild.GetRole(x.RoleId))
			.Where(x => x != null);
		await context.User.ModifyRolesAsync(
			rolesToAdd: roles,
			rolesToRemove: [],
			_PersistentRoles
		).ConfigureAwait(false);
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
		return punishmentService.HandleAsync(context);
	}

	private record AutoModContext(IGuildUser User)
	{
		public IGuild Guild => User.Guild;
	}

	private record AutoModMessageContext(
		IGuildUser User,
		ITextChannel Channel,
		IUserMessage Message
	) : AutoModContext(User)
	{
		public static AutoModMessageContext? FromMessage(IMessage message)
		{
			if (message is not IUserMessage userMessage
				|| userMessage.Author is not IGuildUser user
				|| userMessage.Channel is not ITextChannel channel)
			{
				return null;
			}
			return new(user, channel, userMessage);
		}
	}
}