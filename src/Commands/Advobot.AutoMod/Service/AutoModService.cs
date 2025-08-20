using Advobot.AutoMod.Database;
using Advobot.AutoMod.Database.Models;
using Advobot.AutoMod.Utilities;
using Advobot.Punishments;
using Advobot.Services;
using Advobot.Services.Punishments;
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
	AutoModDatabase db,
	ITimeService time,
	IPunishmentService punishmentService
) : StartableService
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

	private readonly ConcurrentDictionary<UserKey, ConcurrentDictionary<PunishmentType, int>> _Infractions = new();

	private ConcurrentDictionary<ulong, ConcurrentBag<ulong>> _ToDelete = new();

	protected override Task StartAsyncImpl()
	{
		client.MessageReceived += OnMessageReceived;
		client.MessageUpdated += OnMessageUpdated;
		client.UserJoined += OnUserJoined;

		_ = Task.Run(async () =>
		{
			while (IsRunning)
			{
				foreach (var (channelId, messageIds) in Interlocked.Exchange(ref _ToDelete, []))
				{
					try
					{
						if (client.GetChannel(channelId) is not ITextChannel channel)
						{
							logger.LogWarning(
								message: "Channel was null while deleting messages. {@Info}",
								args: new
								{
									Channel = channelId
								}
							);
							continue;
						}

						await channel.DeleteMessagesAsync(messageIds, _AutoMod).ConfigureAwait(false);
					}
					catch (Exception e)
					{
						logger.LogWarning(
							exception: e,
							message: "Exception occurred while deleting messages. {@Info}",
							args: new
							{
								Channel = channelId,
								Messages = messageIds,
							}
						);
					}
				}

				await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);
			}
		});
		return Task.CompletedTask;
	}

	protected override Task StopAsyncImpl()
	{
		client.MessageReceived -= OnMessageReceived;
		client.MessageUpdated -= OnMessageUpdated;
		client.UserJoined -= OnUserJoined;

		return Task.CompletedTask;
	}

	private async Task<bool> HandleBannedNamesAsync(AutoModContext context)
	{
		var names = await db.GetBannedNamesAsync(context.Guild.Id).ConfigureAwait(false);
		foreach (var name in names)
		{
			if (!name.IsMatch(context.User.Username))
			{
				continue;
			}

			logger.LogInformation(
				message: "{User} had a banned name. {@Info}",
				args: [context.User.Id, new
				{
					Guild = context.Guild.Id,
					Username = context.User.Username,
					BannedName = name.Phrase,
				}]
			);

			await PunishAsync(context.User, _Ban, _BannedName).ConfigureAwait(false);
			return true;
		}
		return false;
	}

	private async Task<bool> HandleBannedPhrasesAsync(AutoModMessageContext context)
	{
		var phrases = await db.GetBannedPhrasesAsync(context.Guild.Id).ConfigureAwait(false);
		var infractions = _Infractions.GetOrAdd(
			key: new(context.Guild.Id, context.User.Id),
			valueFactory: _ => new(Enum.GetValues<PunishmentType>().ToDictionary(x => x, _ => 0))
		);

		var isDirty = false;
		foreach (var phrase in phrases)
		{
			if (!phrase.IsMatch(context.Message.Content))
			{
				continue;
			}

			logger.LogInformation(
				message: "{User} sent a banned phrase. {@Info}",
				args: [context.User.Id, new
				{
					Guild = context.Guild.Id,
					BannedPhrase = phrase.Phrase,
				}]
			);

			++infractions[phrase.PunishmentType];
			isDirty = true;
		}
		if (!isDirty)
		{
			return false;
		}

		var punishments = await db.GetPunishmentsAsync(context.Guild.Id).ConfigureAwait(false);
		foreach (var punishment in punishments)
		{
			foreach (var infraction in infractions)
			{
				if (punishment.PunishmentType == infraction.Key &&
					punishment.Instances == infraction.Value)
				{
					logger.LogInformation(
						message: "{User} received enough infractions to recieve a punishment. {@Info}",
						args: [context.User.Id, new
						{
							Guild = context.Guild.Id,
							PunishmentType = punishment.PunishmentType,
							InfractionCount = punishment.Instances,
						}]
					);

					await PunishAsync(context.User, punishment, _BannedPhrase).ConfigureAwait(false);
				}
			}
		}
		return true;
	}

	private async Task HandlePersistentRolesAsync(AutoModContext context)
	{
		var persistentRoles = await db.GetPersistentRolesAsync(context.Guild.Id, context.User.Id).ConfigureAwait(false);
		if (persistentRoles.Count == 0)
		{
			return;
		}

		var roles = persistentRoles
			.Select(x => context.Guild.GetRole(x.RoleId))
			.Where(x => x != null);

		try
		{
			await context.User.ModifyRolesAsync(
				rolesToAdd: roles,
				rolesToRemove: [],
				_PersistentRoles
			).ConfigureAwait(false);
		}
		catch (Exception e)
		{
			logger.LogWarning(
				exception: e,
				message: "Exception occurred while giving persistent roles. {@Info}",
				args: new
				{
					Guild = context.Guild.Id,
					User = context.User.Id,
					Roles = roles.Select(x => x.Id).ToArray(),
				}
			);
		}
	}

	private async Task OnMessageReceived(IMessage message)
	{
		if (message is not IUserMessage userMessage
			|| userMessage.Author is not IGuildUser user
			|| userMessage.Channel is not ITextChannel channel)
		{
			return;
		}

		var context = new AutoModMessageContext(user, channel, userMessage);
		var settings = await db.GetAutoModSettingsAsync(context.Guild.Id).ConfigureAwait(false);
		var ts = time.UtcNow - message.CreatedAt.UtcDateTime;
		if (!await settings.ShouldScanMessageAsync(message, ts).ConfigureAwait(false))
		{
			return;
		}

		if (await HandleBannedPhrasesAsync(context).ConfigureAwait(false)
			|| !await ValidateMessageAsync(context).ConfigureAwait(false))
		{
			_ToDelete.GetOrAdd(message.Channel.Id, _ => []).Add(message.Id);
		}
	}

	private Task OnMessageUpdated(Cacheable<IMessage, ulong> _, IMessage message, ISocketMessageChannel __)
		=> OnMessageReceived(message);

	private async Task OnUserJoined(IGuildUser user)
	{
		var context = new AutoModContext(user);
		if (await HandleBannedNamesAsync(context).ConfigureAwait(false))
		{
			return;
		}

		await HandlePersistentRolesAsync(context).ConfigureAwait(false);
	}

	private async Task PunishAsync(
		IGuildUser user,
		Punishment punishment,
		RequestOptions options)
	{
		var context = new DynamicPunishment(user.Guild, user.Id, true, punishment.PunishmentType)
		{
			RoleId = punishment.RoleId,
		};

		try
		{
			await punishmentService.PunishAsync(context, options).ConfigureAwait(false);
		}
		catch (Exception e)
		{
			logger.LogWarning(
				exception: e,
				message: "Exception occurred while auto mod punishing user. {@Info}",
				args: new
				{
					Guild = context.Guild.Id,
					User = context.UserId,
					Role = context.RoleId,
					PunishmentType = context.Type,
					IsGive = context.IsGive,
				}
			);
		}
	}

	private async Task<bool> ValidateMessageAsync(AutoModMessageContext context)
	{
		var settings = await db.GetChannelSettingsAsync(context.Channel.Id).ConfigureAwait(false);
		if (settings is null)
		{
			return true;
		}

		static int GetImageCount(IMessage message)
		{
			var attachments = message.Attachments
				.Count(x => x.Height != null || x.Width != null);
			var embeds = message.Embeds
				.Count(x => x.Image != null || x.Video != null);
			return attachments + embeds;
		}

		return !settings.IsImageOnly || GetImageCount(context.Message) > 0;
	}

	private readonly record struct UserKey(ulong GuildId, ulong UserId);

	private record AutoModContext(IGuildUser User)
	{
		public IGuild Guild => User.Guild;
	}

	private record AutoModMessageContext(
		IGuildUser User,
		ITextChannel Channel,
		IUserMessage Message
	) : AutoModContext(User);
}