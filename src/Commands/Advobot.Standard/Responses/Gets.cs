using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Advobot.Classes;
using Advobot.Formatting;
using Advobot.Modules;
using Advobot.Services.GuildSettings;
using Advobot.Services.Logging;
using Advobot.Utilities;

using AdvorangesUtils;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

using static Advobot.Standard.Resources.Responses;
using static Advobot.Utilities.FormattingUtils;

namespace Advobot.Standard.Responses
{
	public sealed class Gets : CommandResponses
	{
		private static readonly IReadOnlyList<ActivityType> _Activities
			= Enum.GetValues(typeof(ActivityType)).Cast<ActivityType>().ToArray();

		private static readonly IReadOnlyList<GuildPermission> _Permissions
			= Enum.GetValues(typeof(GuildPermission)).Cast<GuildPermission>().ToArray();

		private static readonly IReadOnlyList<UserStatus> _Statuses
							= Enum.GetValues(typeof(UserStatus)).Cast<UserStatus>().ToArray();

		private Gets()
		{
		}

		public static async Task<RuntimeResult> AllGuildUsers(IGuild guild)
		{
			var users = await guild.GetUsersAsync().CAF();
			var statuses = _Statuses.ToDictionary(x => x, x => 0);
			var activities = _Activities.ToDictionary(x => x, x => 0);
			int webhooks = 0, bots = 0, nickname = 0, voice = 0;
			foreach (var user in users)
			{
				++statuses[user.Status];
				if (user.Activity != null) { ++activities[user.Activity.Type]; }
				if (user.IsWebhook) { ++webhooks; }
				if (user.IsBot) { ++bots; }
				if (user.Nickname != null) { ++nickname; }
				if (user.VoiceChannel != null) { ++voice; }
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
				Author = new EmbedAuthorBuilder
				{
					Name = guild.Format(),
					IconUrl = guild.IconUrl,
				},
				Footer = new EmbedFooterBuilder
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

		public static AdvobotResult Bot(DiscordShardedClient client, ILogService logging)
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
				Footer = new EmbedFooterBuilder { Text = $"Versions [Bot: {Constants.BOT_VERSION}] [API: {Constants.API_VERSION}]", },
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

		public static async Task<RuntimeResult> Channel(
			IGuildChannel channel,
			IGuildSettings settings)
		{
			var userCount = (await channel.GetUsersAsync().FlattenAsync().CAF()).Count();
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
			var logs = info.CreateCollection();
			logs.Add(GetsTitleIgnoredLog, settings.IgnoredLogChannels.Contains(channel.Id));
			logs.Add(GetsTitleIgnoredCommands, settings.IgnoredCommandChannels.Contains(channel.Id));
			logs.Add(GetsTitleImageOnly, settings.ImageOnlyChannels.Contains(channel.Id));
			logs.Add(GetsTitleServerLog, settings.ServerLogId == channel.Id);
			logs.Add(GetsTitleModLog, settings.ModLogId == channel.Id);
			logs.Add(GetsTitleImageLog, settings.ImageLogId == channel.Id);

			var embed = new EmbedWrapper
			{
				Description = info.ToString(),
				Author = new EmbedAuthorBuilder
				{
					Name = channel.Format(),
					IconUrl = channel.Guild.IconUrl,
				},
				Footer = new EmbedFooterBuilder
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
				Author = new EmbedAuthorBuilder
				{
					Name = ((IEmote)emote).Format(),
					IconUrl = emote.Url,
					Url = emote.Url,
				},
				Footer = new EmbedFooterBuilder
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
				if (channel is ICategoryChannel) { ++categories; }
				if (channel is IVoiceChannel) { ++voice; }
				if (channel is ITextChannel) { ++text; }
			}
			int emotes = 0, local = 0, animated = 0, managed = 0;
			foreach (var emote in guild.Emotes)
			{
				++emotes;
				if (emote.IsManaged) { ++managed; }
				if (emote.Animated) { ++animated; }
				else { ++local; }
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
				Author = new EmbedAuthorBuilder
				{
					Name = guild.Format(),
					IconUrl = guild.IconUrl,
				},
				Footer = new EmbedFooterBuilder
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
				special.Add(GetsTitleEmbedChannel, (await guild.GetEmbedChannelAsync().CAF()).Format());
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
			if (guild.Features.Count > 0)
			{
				var fieldValue = guild.Features.Join().WithBlock().Value;
				embed.TryAddField(GetsTitleFeatures, fieldValue, false, out _);
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
				Author = new EmbedAuthorBuilder
				{
					Name = invite.Format(),
					IconUrl = invite.Guild.IconUrl,
					Url = invite.Url,
				},
				Footer = new EmbedFooterBuilder
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
			meta.Add(GetsTitleColor, $"#{role.Color.RawValue.ToString("X6")}");
			var other = info.CreateCollection();
			other.Add(GetsTitleHoisted, role.IsHoisted);
			other.Add(GetsTitleManaged, role.IsManaged);
			other.Add(GetsTitleMentionable, role.IsMentionable);

			var embed = new EmbedWrapper
			{
				Description = info.ToString(),
				Color = role.Color,
				Author = new EmbedAuthorBuilder { Name = role.Format(), },
				Footer = new EmbedFooterBuilder { Text = GetsFooterRole, },
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
					_ => throw new ArgumentOutOfRangeException(nameof(shard.ConnectionState)),
				};
				return $"Shard `{shard.ShardId}`: `{statusEmoji} ({shard.Latency}ms)`";
			}, "\n");
			return Success(new EmbedWrapper
			{
				Description = description,
				Author = client.CurrentUser.CreateAuthor(),
				Footer = new EmbedFooterBuilder
				{
					Text = GetsFooterShards,
					IconUrl = client.CurrentUser.GetAvatarUrl(),
				},
			});
		}

		public static AdvobotResult ShowAllEnums(IEnumerable<Type> enums)
		{
			var description = enums
				.Join(x => x.Name)
				.WithBlock()
				.Value;
			return Success(new EmbedWrapper
			{
				Title = GetsTitleEnumNames,
				Description = description,
			});
		}

		public static AdvobotResult ShowEnumNames<T>(ulong value) where T : struct, Enum
		{
			return Success(GetsShowEnumNames.Format(
				value.ToString().WithBlock(),
				EnumUtils.GetFlagNames((T)(object)value).Join().WithBlock()
			));
		}

		public static AdvobotResult ShowEnumValues(Type enumType)
		{
			var description = Enum.GetNames(enumType)
				.Join()
				.WithBlock()
				.Value;
			return Success(new EmbedWrapper
			{
				Title = enumType.Name, //TODO: Localize enum name
				Description = description,
			});
		}

		public static async Task<RuntimeResult> User(IUser user)
		{
			var info = new InformationMatrix();
			info.AddTimeCreatedCollection(user);
			var status = info.CreateCollection();
			status.Add(GetsTitleActivity, user.Activity.Format());
			status.Add(GetsTitleStatus, user.Status.ToString());

			var embed = new EmbedWrapper
			{
				Description = info.ToString(),
				ThumbnailUrl = user.GetAvatarUrl(),
				Author = user.CreateAuthor(),
				Footer = new EmbedFooterBuilder
				{
					Text = GetsFooterUser,
					IconUrl = user.GetAvatarUrl(),
				},
			};

			//User is not from a guild so we can't get any more information about them
			if (!(user is IGuildUser guildUser))
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
				var join = (await guildUser.Guild.GetUsersAsync().CAF())
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
				var fieldValue = roles.Join(x => x.Name).WithBlock().Value;
				embed.TryAddField(GetsTitleRoles, fieldValue, false, out _);
				embed.Color = roles.LastOrDefault(x => x.Color.RawValue != 0)?.Color;
			}
			if (textChannels.Count > 0)
			{
				var fieldValue = textChannels.Join(x => x.Name).WithBlock().Value;
				embed.TryAddField(GetsTitleTextChannels, fieldValue, false, out _);
			}
			if (voiceChannels.Count > 0)
			{
				var fieldValue = voiceChannels.Join(x => x.Name).WithBlock().Value;
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
			var text = users.OrderBy(x => x.JoinedAt).FormatNumberedList(x =>
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
				Author = new EmbedAuthorBuilder
				{
					Name = webhook.Format(),
					IconUrl = webhook.GetAvatarUrl(),
					Url = webhook.GetAvatarUrl(),
				},
				Footer = new EmbedFooterBuilder
				{
					Text = GetsFooterWebhook,
					IconUrl = webhook.GetAvatarUrl(),
				},
			});
		}
	}
}