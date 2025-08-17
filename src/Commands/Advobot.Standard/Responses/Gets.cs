using Advobot.Embeds;
using Advobot.Info;
using Advobot.Modules;
using Advobot.Utilities;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

using System.Text;

using static Advobot.Resources.Responses;
using static Advobot.Utilities.FormattingUtils;

namespace Advobot.Standard.Responses;

public sealed class Gets : AdvobotResult
{
	private static readonly IReadOnlyList<ActivityType> _Activities
		= Enum.GetValues<ActivityType>();

	private static readonly IReadOnlyList<GuildPermission> _Permissions
		= Enum.GetValues<GuildPermission>();

	private static readonly IReadOnlyList<UserStatus> _Statuses
		= Enum.GetValues<UserStatus>();

	private Gets() : base(null, "")
	{
	}

	public static async Task<RuntimeResult> AllGuildUsers(IGuild guild)
	{
		var users = await guild.GetUsersAsync().ConfigureAwait(false);
		var statuses = _Statuses.ToDictionary(x => x, _ => 0);
		var activities = _Activities.ToDictionary(x => x, _ => 0);
		int webhooks = 0, bots = 0, nickname = 0, voice = 0;
		foreach (var user in users)
		{
			++statuses[user.Status];
			foreach (var activity in user.Activities)
			{
				++activities[activity.Type];
			}
			if (user.IsWebhook)
			{
				++webhooks;
			}
			if (user.IsBot)
			{
				++bots;
			}
			if (user.Nickname != null)
			{
				++nickname;
			}
			if (user.VoiceChannel != null)
			{
				++voice;
			}
		}

		var info = new InfoMatrix();
		var meta = info.CreateCollection();
		meta.Add(GetsTitleUserCount, users.Count);
		meta.Add(GetsTitleBotCount, bots);
		meta.Add(GetsTitleWebhookCount, webhooks);
		meta.Add(GetsTitleInVoiceCount, voice);
		meta.Add(GetsTitleNicknameCount, nickname);

		var embed = new EmbedWrapper
		{
			Description = info.ToString(),
			Author = new()
			{
				Name = guild.Format(),
				IconUrl = guild.IconUrl,
			},
			Footer = new()
			{
				Text = GetsFooterGuildUsers,
				IconUrl = guild.IconUrl,
			},
		};
		{
			var statusInfo = new InfoCollection();
			foreach (var kvp in statuses)
			{
				statusInfo.Add(kvp.Key.ToString(), kvp.Value);
			}
			embed.TryAddField(GetsTitleStatuses, statusInfo.ToString(), false, out _);
		}
		{
			var activityInfo = new InfoCollection();
			foreach (var kvp in activities)
			{
				activityInfo.Add(kvp.Key.ToString(), kvp.Value);
			}
			embed.TryAddField(GetsTitleActivities, activityInfo.ToString(), false, out _);
		}
		return Success(embed);
	}

	public static AdvobotResult Bot(DiscordShardedClient client)
	{
		var startTime = Constants.START.ToReadable();
		var runDuration = DateTime.UtcNow - Constants.START;
		var embed = new EmbedWrapper
		{
			Description = $"**Online Since:** `{startTime}` (`{runDuration:g}`)\n" +
				$"**Latency:** `{client.Latency}`\n" +
				$"**Shard Count:** `{client.Shards.Count}`",
			Author = client.CurrentUser.CreateAuthor(),
			Footer = new()
			{
				Text = $"Versions [Bot: {Constants.BOT_VERSION}] [Discord.Net: {Constants.DISCORD_NET_VERSION}]",
			},
		};

		return Success(embed);
	}

	public static async Task<RuntimeResult> Channel(IGuildChannel channel)
	{
		var userCount = channel is SocketGuildChannel sgc ? sgc.Users.Count : 0;
		var roles = new List<string>();
		var users = new List<string>();
		foreach (var o in channel.PermissionOverwrites)
		{
			if (o.TargetType == PermissionTarget.Role)
			{
				roles.Add(channel.Guild.GetRole(o.TargetId).Name);
			}
			else if (o.TargetType == PermissionTarget.User)
			{
				var user = await channel.Guild.GetUserAsync(o.TargetId).ConfigureAwait(false);
				users.Add(user.Username);
			}
		}

		var info = new InfoMatrix();
		info.AddTimeCreatedCollection(channel);
		var meta = info.CreateCollection();
		meta.Add(GetsTitlePosition, channel.Position);
		meta.Add(GetsTitleUserCount, userCount);

		var embed = new EmbedWrapper
		{
			Description = info.ToString(),
			Author = new()
			{
				Name = channel.Format(),
				IconUrl = channel.Guild.IconUrl,
			},
			Footer = new()
			{
				Text = GetsFooterChannel,
				IconUrl = channel.Guild.IconUrl,
			},
		};
		if (roles.Count > 0)
		{
			var fieldValue = roles.Join().WithBlock().Current;
			embed.TryAddField(GetsTitleRoles, fieldValue, false, out _);
		}
		if (users.Count > 0)
		{
			var fieldValue = users.Join().WithBlock().Current;
			embed.TryAddField(GetsTitleUsers, fieldValue, false, out _);
		}
		return Success(embed);
	}

	public static AdvobotResult Emote(Emote emote)
	{
		var info = new InfoMatrix();
		info.AddTimeCreatedCollection(emote);
		//Emote is GuildEmote meaning we can get extra informatino about it
		if (emote is GuildEmote guildEmote)
		{
			var meta = info.CreateCollection();
			meta.Add(GetsTitleManaged, guildEmote.IsManaged);
			meta.Add(GetsTitleColons, guildEmote.RequireColons);

			var roles = info.CreateCollection();
			roles.Add(GetsTitleRoles, guildEmote.RoleIds.Select(x => x.ToString()).Join());
		}

		return Success(new EmbedWrapper
		{
			Description = info.ToString(),
			ThumbnailUrl = emote.Url,
			Author = new()
			{
				Name = ((IEmote)emote).Format(),
				IconUrl = emote.Url,
				Url = emote.Url,
			},
			Footer = new()
			{
				Text = GetsFooterEmote,
				IconUrl = emote.Url,
			},
		});
	}

	public static async Task<RuntimeResult> Guild(IGuild guild)
	{
		var userCount = (await guild.GetUsersAsync().ConfigureAwait(false)).Count;
		var owner = await guild.GetOwnerAsync().ConfigureAwait(false);

		int channels = 0, categories = 0, voice = 0, text = 0;
		foreach (var channel in await guild.GetChannelsAsync().ConfigureAwait(false))
		{
			++channels;
			if (channel is ICategoryChannel)
			{
				++categories;
			}
			if (channel is IVoiceChannel)
			{
				++voice;
			}
			if (channel is ITextChannel)
			{
				++text;
			}
		}
		int emotes = 0, local = 0, animated = 0, managed = 0;
		foreach (var emote in guild.Emotes)
		{
			++emotes;
			if (emote.IsManaged)
			{
				++managed;
			}
			if (emote.Animated)
			{
				++animated;
			}
			else
			{
				++local;
			}
		}

		var info = new InfoMatrix();
		info.AddTimeCreatedCollection(guild);
		var meta = info.CreateCollection();
		meta.Add(GetsTitleOwner, owner.Format());
		meta.Add(GetsTitleUserCount, userCount);
		meta.Add(GetsTitleRoleCount, guild.Roles.Count);
		meta.Add(GetsTitleNotifications, guild.DefaultMessageNotifications.ToString());
		meta.Add(GetsTitleVerification, guild.VerificationLevel.ToString());
		meta.Add(GetsTitleVoiceRegion, guild.VoiceRegionId);

		var embed = new EmbedWrapper
		{
			Description = info.ToString(),
			Color = owner.GetRoles().LastOrDefault(x => x.Color.RawValue != 0)?.Color,
			ThumbnailUrl = guild.IconUrl,
			Author = new()
			{
				Name = guild.Format(),
				IconUrl = guild.IconUrl,
			},
			Footer = new()
			{
				Text = GetsFooterGuild,
				IconUrl = guild.IconUrl,
			},
		};
		{
			var channelInfo = new InfoMatrix();
			var counts = channelInfo.CreateCollection();
			counts.Add(GetsTitleChannelCount, channels);
			counts.Add(GetsTitleTextChannelCount, text);
			counts.Add(GetsTitleVoiceChannelCount, voice);
			counts.Add(GetsTitleCategoryChannelCount, categories);
			var special = channelInfo.CreateCollection();
			special.Add(GetsTitleDefaultChannel, (await guild.GetDefaultChannelAsync().ConfigureAwait(false)).Format());
			special.Add(GetsTitleAfkChannel, (await guild.GetAFKChannelAsync().ConfigureAwait(false)).Format());
			special.Add(GetsTitleSystemChannel, (await guild.GetSystemChannelAsync().ConfigureAwait(false)).Format());
			special.Add(GetsTitleEmbedChannel, (await guild.GetWidgetChannelAsync().ConfigureAwait(false)).Format());
			embed.TryAddField(GetsTitleChannelInfo, channelInfo.ToString(), false, out _);
		}
		{
			var emoteInfo = new InfoCollection();
			emoteInfo.Add(GetsTitleEmoteCount, emotes);
			emoteInfo.Add(GetsTitleAnimatedEmoteCount, animated);
			emoteInfo.Add(GetsTitleLocalEmoteCount, local);
			emoteInfo.Add(GetsTitleManagedEmoteCount, managed);
			embed.TryAddField(GetsTitleEmoteInfo, emoteInfo.ToString(), false, out _);
		}
		{
			var fieldValue = "";
			if (guild.Features.Value != 0)
			{
				fieldValue += guild.Features.Value.ToString();
			}
			if (guild.Features.Experimental.Count > 0)
			{
				fieldValue += guild.Features.Experimental.Join();
			}

			if (!string.IsNullOrWhiteSpace(fieldValue))
			{
				fieldValue = fieldValue.WithBlock().Current;
				embed.TryAddField(GetsTitleFeatures, fieldValue, false, out _);
			}
		}
		return Success(embed);
	}

	public static AdvobotResult Guilds(IReadOnlyCollection<IGuild> guilds)
	{
		var text = guilds.Select(x => GetsUserJoins.Format(
			x.Format().WithNoMarkdown(),
			x.OwnerId.ToString().WithNoMarkdown()
		)).FormatNumberedList();
		return Success(MessageUtils.CreateTextFile(GetsTitleGuilds, text));
	}

	public static AdvobotResult Invite(IInviteMetadata invite)
	{
		var info = new InfoMatrix();
		info.AddTimeCreatedCollection(invite.Id, invite.CreatedAt.GetValueOrDefault().UtcDateTime);
		var meta = info.CreateCollection();
		meta.Add(GetsTitleCreator, invite.Inviter.Format());
		meta.Add(GetsTitleChannel, invite.Channel.Format());
		meta.Add(GetsTitleUses, invite.Uses ?? 0);

		return Success(new EmbedWrapper
		{
			Description = info.ToString(),
			Author = new()
			{
				Name = invite.Format(),
				IconUrl = invite.Guild.IconUrl,
				Url = invite.Url,
			},
			Footer = new()
			{
				Text = GetsFooterInvite,
				IconUrl = invite.Guild.IconUrl,
			},
		});
	}

	public static AdvobotResult Messages(
		IMessageChannel channel,
		IReadOnlyCollection<IMessage> messages,
		int maxSize)
	{
		var formattedMessagesBuilder = new StringBuilder();
		foreach (var message in messages)
		{
			var text = message.Format(withMentions: false).Sanitize(keepMarkdown: false);
			if (formattedMessagesBuilder.Length + text.Length >= maxSize)
			{
				break;
			}
			formattedMessagesBuilder.AppendLine(text);
		}

		var fileName = GetsFileMessages.Format(channel.Name.WithNoMarkdown());
		var content = formattedMessagesBuilder.ToString();
		return Success(MessageUtils.CreateTextFile(fileName, content));
	}

	public static async Task<RuntimeResult> Role(IRole role)
	{
		var userCount = (await role.Guild.GetUsersAsync().ConfigureAwait(false)).Count(x => x.RoleIds.Contains(role.Id));
		var permissions = _Permissions.Where(x => role.Permissions.Has(x)).Select(x => x.ToString()).ToArray();

		var info = new InfoMatrix();
		info.AddTimeCreatedCollection(role);
		var meta = info.CreateCollection();
		meta.Add(GetsTitlePosition, role.Position);
		meta.Add(GetsTitleUserCount, userCount);
		meta.Add(GetsTitleColor, $"#{role.Color.RawValue:X6}");
		var other = info.CreateCollection();
		other.Add(GetsTitleHoisted, role.IsHoisted);
		other.Add(GetsTitleManaged, role.IsManaged);
		other.Add(GetsTitleMentionable, role.IsMentionable);

		var embed = new EmbedWrapper
		{
			Description = info.ToString(),
			Color = role.Color,
			Author = new() { Name = role.Format(), },
			Footer = new() { Text = GetsFooterRole, },
		};
		if (permissions.Length > 0)
		{
			var fieldValue = permissions.Join().WithBlock().Current;
			embed.TryAddField(GetsTitlePermissions, fieldValue, false, out _);
		}
		return Success(embed);
	}

	public static AdvobotResult Shards(DiscordShardedClient client)
	{
		var description = client.Shards.Select(shard =>
		{
			var statusEmoji = shard.ConnectionState switch
			{
				ConnectionState.Disconnected => Constants.DENIED,
				ConnectionState.Disconnecting => Constants.DENIED,
				ConnectionState.Connected => Constants.ALLOWED,
				ConnectionState.Connecting => Constants.ALLOWED,
				_ => Constants.UNKNOWN,
			};
			return $"Shard `{shard.ShardId}`: `{statusEmoji} ({shard.Latency}ms)`";
		}).Join("\n");
		return Success(new EmbedWrapper
		{
			Description = description,
			Author = client.CurrentUser.CreateAuthor(),
			Footer = new()
			{
				Text = GetsFooterShards,
				IconUrl = client.CurrentUser.GetAvatarUrl(),
			},
		});
	}

	public static AdvobotResult ShowEnumNames<T>(ulong value) where T : struct, Enum
	{
		return Success(GetsShowEnumNames.Format(
			value.ToString().WithBlock(),
			((T)(object)value).ToString("F").WithBlock()
		));
	}

	public static async Task<RuntimeResult> User(IUser user)
	{
		var info = new InfoMatrix();
		info.AddTimeCreatedCollection(user);
		var status = info.CreateCollection();
		status.Add(GetsTitleActivity, user.Activities.Select(x => x.Format()).Join("\n"));
		status.Add(GetsTitleStatus, user.Status.ToString());

		var embed = new EmbedWrapper
		{
			Description = info.ToString(),
			ThumbnailUrl = user.GetAvatarUrl(),
			Author = user.CreateAuthor(),
			Footer = new()
			{
				Text = GetsFooterUser,
				IconUrl = user.GetAvatarUrl(),
			},
		};

		//User is not from a guild so we can't get any more information about them
		if (user is not IGuildUser guildUser)
		{
			return Success(embed);
		}

		var guildInfo = info.CreateCollection();
		if (guildUser.Nickname is string nickname)
		{
			guildInfo.Add(GetsTitleNickname, nickname.EscapeBackTicks());
		}
		if (guildUser.JoinedAt is DateTimeOffset dto)
		{
			//If cachemode is allow download this can take ages
#if DEBUG
			var a = guildUser.Guild as SocketGuild;
			var b = a?.Users;
#endif
			var join = (await guildUser.Guild.GetUsersAsync(CacheMode.CacheOnly).ConfigureAwait(false))
				.Count(x => x.JoinedAt < guildUser.JoinedAt);
			guildInfo.Add(GetsTitleJoined, GetsJoinedAt.Format(
				dto.UtcDateTime.ToReadable().WithNoMarkdown(),
				join.ToString().WithNoMarkdown()
			));
		}
		embed.Description = info.ToString(); //Reupdate the description in case any changes

		async Task<IReadOnlyCollection<T>> GetChannelsAsync<T>(
			Func<IGuild, Task<IReadOnlyCollection<T>>> getter,
			Func<ChannelPermissions, bool> permCheck)
			where T : IGuildChannel
		{
			var channels = await getter(guildUser.Guild).ConfigureAwait(false);
			var ordered = channels.OrderBy(x => x.Position);
			var valid = ordered.Where(x => permCheck(guildUser.GetPermissions(x)));
			return valid.ToArray();
		}

		var roles = guildUser.GetRoles();
		var textChannels = await GetChannelsAsync(x => x.GetTextChannelsAsync(),
			x => x.ViewChannel).ConfigureAwait(false);
		var voiceChannels = await GetChannelsAsync(x => x.GetVoiceChannelsAsync(),
			x => x.ViewChannel && x.Connect).ConfigureAwait(false);

		if (roles.Count > 0)
		{
			var fieldValue = roles.Select(x => x.Name).Join().WithBigBlock().Current;
			embed.TryAddField(GetsTitleRoles, fieldValue, false, out _);
			embed.Color = roles.LastOrDefault(x => x.Color.RawValue != 0)?.Color;
		}
		if (textChannels.Count > 0)
		{
			var fieldValue = textChannels.Select(x => x.Name).Join().WithBigBlock().Current;
			embed.TryAddField(GetsTitleTextChannels, fieldValue, false, out _);
		}
		if (voiceChannels.Count > 0)
		{
			var fieldValue = voiceChannels.Select(x => x.Name).Join().WithBigBlock().Current;
			embed.TryAddField(GetsTitleVoiceChannels, fieldValue, false, out _);
		}
		if (guildUser.VoiceChannel is IVoiceChannel vc)
		{
			var voiceInfo = new InfoMatrix();
			var voiceChannel = voiceInfo.CreateCollection();
			voiceChannel.Add(GetsTitleVoiceChannel, vc.Format());
			var voiceMeta = voiceInfo.CreateCollection();
			voiceMeta.Add(GetsTitleServerMute, guildUser.IsMuted);
			voiceMeta.Add(GetsTitleServerDeafen, guildUser.IsDeafened);
			voiceMeta.Add(GetsTitleMute, guildUser.IsSelfMuted);
			voiceMeta.Add(GetsTitleDeafen, guildUser.IsSelfDeafened);
			embed.TryAddField(GetsTitleVoiceInfo, voiceInfo.ToString(), false, out _);
		}
		return Success(embed);
	}

	public static AdvobotResult UserJoin(IReadOnlyCollection<IGuildUser> users)
	{
		var text = users.Select(x =>
		{
			var joined = x.JoinedAt ?? DateTimeOffset.UtcNow;
			return GetsUserJoins.Format(
				x.Format().WithNoMarkdown(),
				joined.UtcDateTime.ToReadable().WithNoMarkdown()
			);
		}).FormatNumberedList();
		return Success(MessageUtils.CreateTextFile(GetsFileUserJoins, text));
	}

	public static AdvobotResult UserJoinPosition(IGuildUser user, int position)
	{
		var joined = user.JoinedAt ?? DateTimeOffset.UtcNow;
		return Success(GetsUserJoinPosition.Format(
			user.Format().WithBlock(),
			position.ToString().WithBlock(),
			joined.UtcDateTime.ToReadable().WithBlock()
		));
	}

	public static AdvobotResult UsersWithReason(IEnumerable<IGuildUser> users)
	{
		var text = users.Select(x => x.Format()).FormatNumberedList();
		return Success(MessageUtils.CreateTextFile(GetsFileUsersWithReason, text));
	}

	public static AdvobotResult Webhook(IWebhook webhook)
	{
		var info = new InfoMatrix();
		info.AddTimeCreatedCollection(webhook);
		var meta = info.CreateCollection();
		meta.Add(GetsTitleCreator, webhook.Creator.Format());
		meta.Add(GetsTitleChannel, webhook.Channel.Format());

		return Success(new EmbedWrapper
		{
			Description = info.ToString(),
			ThumbnailUrl = webhook.GetAvatarUrl(),
			Author = new()
			{
				Name = webhook.Format(),
				IconUrl = webhook.GetAvatarUrl(),
				Url = webhook.GetAvatarUrl(),
			},
			Footer = new()
			{
				Text = GetsFooterWebhook,
				IconUrl = webhook.GetAvatarUrl(),
			},
		});
	}
}