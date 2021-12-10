using Advobot.Classes;
using Advobot.Formatting;
using Advobot.Modules;
using Advobot.Services.LogCounters;
using Advobot.Utilities;

using AdvorangesUtils;

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
		= AdvobotUtils.GetValues<ActivityType>();

	private static readonly IReadOnlyList<GuildPermission> _Permissions
		= AdvobotUtils.GetValues<GuildPermission>();

	private static readonly IReadOnlyList<UserStatus> _Statuses
		= AdvobotUtils.GetValues<UserStatus>();

	private Gets() : base(null, "")
	{
	}

	public static async Task<RuntimeResult> AllGuildUsers(IGuild guild)
	{
		var users = await guild.GetUsersAsync().CAF();
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

		var info = new InformationMatrix();
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
			var statusInfo = new InformationCollection();
			foreach (var kvp in statuses)
			{
				statusInfo.Add(kvp.Key.ToString(), kvp.Value);
			}
			embed.TryAddField(GetsTitleStatuses, statusInfo.ToString(), false, out _);
		}
		{
			var activityInfo = new InformationCollection();
			foreach (var kvp in activities)
			{
				activityInfo.Add(kvp.Key.ToString(), kvp.Value);
			}
			embed.TryAddField(GetsTitleActivities, activityInfo.ToString(), false, out _);
		}
		return Success(embed);
	}

	public static AdvobotResult Bot(DiscordShardedClient client, ILogCounterService logging)
	{
		static string FormatLogCounters(ILogCounter[] counters)
		{
			var titlesAndCount = new (string Title, string Count)[counters.Length];
			var right = 0;
			var left = 0;
			for (var i = 0; i < counters.Length; ++i)
			{
				var counter = counters[i];
				var temp = (Title: $"**{counter.Name}**:", Count: $"`{counter.Count}`");
				titlesAndCount[i] = temp;
				right = Math.Max(right, temp.Title.Length);
				left = Math.Max(left, temp.Count.Length);
			}

			var sb = new StringBuilder();
			foreach (var (Title, Count) in titlesAndCount)
			{
				sb.AppendLineFeed($"{Title.PadRight(Math.Max(right + 1, 0))}{Count.PadLeft(Math.Max(left, 0))}");
			}
			return sb.ToString().Trim('\n', '\r');
		}

		var embed = new EmbedWrapper
		{
			Description = $"**Online Since:** `{ProcessInfoUtils.GetStartTime().ToReadable()}` (`{AdvorangesUtils.FormattingUtils.GetUptime()}`)\n" +
				$"**Guild/User Count:** `{logging.TotalGuilds.Count}`/`{logging.TotalUsers.Count}`\n" +
				$"**Latency:** `{client.Latency}`\n" +
				$"**Memory Usage:** `{ProcessInfoUtils.GetMemoryMB():0.00}MB`\n" +
				$"**Thread Count:** `{ProcessInfoUtils.GetThreadCount()}`\n" +
				$"**Shard Count:** `{client.Shards.Count}`",
			Author = client.CurrentUser.CreateAuthor(),
			Footer = new()
			{
				Text = $"Versions [Bot: {Constants.BOT_VERSION}] [Discord.Net: {Constants.DISCORD_NET_VERSION}]",
			},
		};

		embed.TryAddField("Users", FormatLogCounters(new[]
		{
				logging.UserJoins,
				logging.UserLeaves,
				logging.UserChanges
			}), true, out _);
		embed.TryAddField("Messages", FormatLogCounters(new[]
		{
				logging.MessageEdits,
				logging.MessageDeletes,
				logging.Images,
				logging.Animated,
				logging.Files
			}), true, out _);
		embed.TryAddField("Commands", FormatLogCounters(new[]
		{
				logging.AttemptedCommands,
				logging.SuccessfulCommands,
				logging.FailedCommands
			}), true, out _);
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
				var user = await channel.Guild.GetUserAsync(o.TargetId).CAF();
				users.Add(user.Username);
			}
		}

		var info = new InformationMatrix();
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
			var fieldValue = roles.Join().WithBlock().Value;
			embed.TryAddField(GetsTitleRoles, fieldValue, false, out _);
		}
		if (users.Count > 0)
		{
			var fieldValue = users.Join().WithBlock().Value;
			embed.TryAddField(GetsTitleUsers, fieldValue, false, out _);
		}
		return Success(embed);
	}

	public static AdvobotResult Emote(Emote emote)
	{
		var info = new InformationMatrix();
		info.AddTimeCreatedCollection(emote);
		//Emote is GuildEmote meaning we can get extra informatino about it
		if (emote is GuildEmote guildEmote)
		{
			var meta = info.CreateCollection();
			meta.Add(GetsTitleManaged, guildEmote.IsManaged);
			meta.Add(GetsTitleColons, guildEmote.RequireColons);

			var roles = info.CreateCollection();
			roles.Add(GetsTitleRoles, guildEmote.RoleIds.Join(x => x.ToString()));
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
		var userCount = (await guild.GetUsersAsync().CAF()).Count;
		var owner = await guild.GetOwnerAsync().CAF();

		int channels = 0, categories = 0, voice = 0, text = 0;
		foreach (var channel in await guild.GetChannelsAsync().CAF())
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

		var info = new InformationMatrix();
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
			var channelInfo = new InformationMatrix();
			var counts = channelInfo.CreateCollection();
			counts.Add(GetsTitleChannelCount, channels);
			counts.Add(GetsTitleTextChannelCount, text);
			counts.Add(GetsTitleVoiceChannelCount, voice);
			counts.Add(GetsTitleCategoryChannelCount, categories);
			var special = channelInfo.CreateCollection();
			special.Add(GetsTitleDefaultChannel, (await guild.GetDefaultChannelAsync().CAF()).Format());
			special.Add(GetsTitleAfkChannel, (await guild.GetAFKChannelAsync().CAF()).Format());
			special.Add(GetsTitleSystemChannel, (await guild.GetSystemChannelAsync().CAF()).Format());
			special.Add(GetsTitleEmbedChannel, (await guild.GetWidgetChannelAsync().CAF()).Format());
			embed.TryAddField(GetsTitleChannelInfo, channelInfo.ToString(), false, out _);
		}
		{
			var emoteInfo = new InformationCollection();
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
				fieldValue = fieldValue.WithBlock().Value;
				embed.TryAddField(GetsTitleFeatures, fieldValue, false, out _);
			}
		}
		return Success(embed);
	}

	public static AdvobotResult Guilds(IReadOnlyCollection<IGuild> guilds)
	{
		var text = guilds.FormatNumberedList(x => GetsUserJoins.Format(
			x.Format().WithNoMarkdown(),
			x.OwnerId.ToString().WithNoMarkdown()
		));
		return Success(new TextFileInfo
		{
			Name = GetsTitleGuilds,
			Text = text,
		});
	}

	public static AdvobotResult Invite(IInviteMetadata invite)
	{
		var info = new InformationMatrix();
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
			var text = message
				.Format(withMentions: false)
				.RemoveAllMarkdown()
				.RemoveDuplicateNewLines();
			if (formattedMessagesBuilder.Length + text.Length >= maxSize)
			{
				break;
			}
			formattedMessagesBuilder.AppendLineFeed(text);
		}

		return Success(new TextFileInfo
		{
			Name = GetsFileMessages.Format(channel.Name.WithNoMarkdown()),
			Text = formattedMessagesBuilder.ToString(),
		});
	}

	public static async Task<RuntimeResult> Role(IRole role)
	{
		var userCount = (await role.Guild.GetUsersAsync().CAF()).Count(x => x.RoleIds.Contains(role.Id));
		var permissions = _Permissions.Where(x => role.Permissions.Has(x)).Select(x => x.ToString()).ToArray();

		var info = new InformationMatrix();
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
			var fieldValue = permissions.Join().WithBlock().Value;
			embed.TryAddField(GetsTitlePermissions, fieldValue, false, out _);
		}
		return Success(embed);
	}

	public static AdvobotResult Shards(DiscordShardedClient client)
	{
		var description = client.Shards.Join(shard =>
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
		}, "\n");
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
			EnumUtils.GetFlagNames((T)(object)value).Join().WithBlock()
		));
	}

	public static async Task<RuntimeResult> User(IUser user)
	{
		var info = new InformationMatrix();
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
			var join = (await guildUser.Guild.GetUsersAsync(CacheMode.CacheOnly).CAF())
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
			var channels = await getter(guildUser.Guild).CAF();
			var ordered = channels.OrderBy(x => x.Position);
			var valid = ordered.Where(x => permCheck(guildUser.GetPermissions(x)));
			return valid.ToArray();
		}

		var roles = guildUser.GetRoles();
		var textChannels = await GetChannelsAsync(x => x.GetTextChannelsAsync(),
			x => x.ViewChannel).CAF();
		var voiceChannels = await GetChannelsAsync(x => x.GetVoiceChannelsAsync(),
			x => x.ViewChannel && x.Connect).CAF();

		if (roles.Count > 0)
		{
			var fieldValue = roles.Join(x => x.Name).WithBigBlock().Value;
			embed.TryAddField(GetsTitleRoles, fieldValue, false, out _);
			embed.Color = roles.LastOrDefault(x => x.Color.RawValue != 0)?.Color;
		}
		if (textChannels.Count > 0)
		{
			var fieldValue = textChannels.Join(x => x.Name).WithBigBlock().Value;
			embed.TryAddField(GetsTitleTextChannels, fieldValue, false, out _);
		}
		if (voiceChannels.Count > 0)
		{
			var fieldValue = voiceChannels.Join(x => x.Name).WithBigBlock().Value;
			embed.TryAddField(GetsTitleVoiceChannels, fieldValue, false, out _);
		}
		if (guildUser.VoiceChannel is IVoiceChannel vc)
		{
			var voiceInfo = new InformationMatrix();
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
		var text = users.FormatNumberedList(x =>
		{
			var joined = x.JoinedAt ?? DateTimeOffset.UtcNow;
			return GetsUserJoins.Format(
				x.Format().WithNoMarkdown(),
				joined.UtcDateTime.ToReadable().WithNoMarkdown()
			);
		});
		return Success(new TextFileInfo
		{
			Name = GetsFileUserJoins,
			Text = text,
		});
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
		var text = users.FormatNumberedList(x => x.Format());
		return Success(new TextFileInfo
		{
			Name = GetsFileUsersWithReason,
			Text = text,
		});
	}

	public static AdvobotResult Webhook(IWebhook webhook)
	{
		var info = new InformationMatrix();
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