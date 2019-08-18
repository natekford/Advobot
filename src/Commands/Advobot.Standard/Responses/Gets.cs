using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Advobot.Classes;
using Advobot.Modules;
using Advobot.Services.GuildSettings;
using Advobot.Services.Logging;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Advobot.Standard.Responses
{
	public sealed class Gets : CommandResponses
	{
		private Gets() { }

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
#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
				foreach (var (Title, Count) in titlesAndCount)
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.
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
			var userCounters = new[] { logging.UserJoins, logging.UserLeaves, logging.UserChanges };
			embed.TryAddField("Users", FormatLogCounters(userCounters), true, out _);
			var msgCounters = new[] { logging.MessageEdits, logging.MessageDeletes, logging.Images, logging.Animated, logging.Files };
			embed.TryAddField("Messages", FormatLogCounters(msgCounters), true, out _);
			var cmdCounters = new[] { logging.AttemptedCommands, logging.SuccessfulCommands, logging.FailedCommands };
			embed.TryAddField("Commands", FormatLogCounters(cmdCounters), true, out _);
			return Success(embed);
		}
		public static AdvobotResult Shards(DiscordShardedClient client)
		{
			var description = client.Shards.Join("\n", shard =>
			{
				var statusEmoji = shard.ConnectionState switch
				{
					ConnectionState.Disconnected => '\u274C', //❌
					ConnectionState.Disconnecting => '\u274C', //❌
					ConnectionState.Connected => '\u2705', //❌
					ConnectionState.Connecting => '\u2705', //❌
					_ => throw new ArgumentOutOfRangeException(nameof(shard.ConnectionState)),
				};
				return $"Shard `{shard.ShardId}`: `{statusEmoji} ({shard.Latency}ms)`";
			});
			return Success(new EmbedWrapper
			{
				Description = description,
				Author = client.CurrentUser.CreateAuthor(),
			});
		}
		public static async Task<RuntimeResult> Guild(IGuild guild)
		{
			int local = 0, animated = 0, managed = 0;
			foreach (var emote in guild.Emotes)
			{
				if (emote.IsManaged) { ++managed; }
				if (emote.Animated) { ++animated; }
				else { ++local; }
			}

			var owner = await guild.GetOwnerAsync().CAF();
			var memberCount = (await guild.GetUsersAsync().CAF()).Count;
			var channelCount = (await guild.GetChannelsAsync().CAF()).Count;
			var textChannelCount = (await guild.GetTextChannelsAsync().CAF()).Count;
			var voiceChannelCount = (await guild.GetVoiceChannelsAsync().CAF()).Count;
			var categoryChannelCount = (await guild.GetCategoriesAsync().CAF()).Count;
			var defaultChannel = await guild.GetDefaultChannelAsync().CAF();
			var afkChannel = await guild.GetAFKChannelAsync().CAF();
			var systemChannel = await guild.GetSystemChannelAsync().CAF();
			var embedChannel = await guild.GetEmbedChannelAsync().CAF();

			return Success(new EmbedWrapper
			{
				Description = FormatIdAndCreatedAt(guild) +
					$"**Owner:** `{owner.Format()}`\n" +
					$"**Default Message Notifs:** `{guild.DefaultMessageNotifications}`\n" +
					$"**Verification Level:** `{guild.VerificationLevel}`\n" +
					$"**Region:** `{guild.VoiceRegionId}`\n\n" +
					$"**Emotes:** `{guild.Emotes.Count}` (`{local}` local, `{animated}` animated, `{managed}` managed)\n" +
					$"**User Count:** `{memberCount}`\n" +
					$"**Role Count:** `{guild.Roles.Count}`\n" +
					$"**Channel Count:** `{channelCount}` " +
						$"(`{textChannelCount}` text, " +
						$"`{voiceChannelCount}` voice, " +
						$"`{categoryChannelCount}` categories)\n\n" +
					$"**Default Channel:** `{defaultChannel?.Format() ?? "None"}`\n" +
					$"**AFK Channel:** `{afkChannel?.Format() ?? "None"}` (Time: `{guild.AFKTimeout / 60}`)\n" +
					$"**System Channel:** `{systemChannel?.Format() ?? "None"}`\n" +
					$"**Embed Channel:** `{embedChannel?.Format() ?? "None"}`\n" +
					(guild.Features.Any() ? $"**Features:** `{string.Join("`, `", guild.Features)}`" : ""),
				Color = owner.GetRoles().LastOrDefault(x => x.Color.RawValue != 0)?.Color,
				ThumbnailUrl = guild.IconUrl,
				Author = new EmbedAuthorBuilder { Name = guild.Format(), },
				Footer = new EmbedFooterBuilder { Text = "Guild Info", },
			});
		}
		public static async Task<RuntimeResult> User(IUser user)
		{
			var embed = new EmbedWrapper
			{
				Description = FormatIdAndCreatedAt(user) +
					$"**Activity:** `{user.Activity.Format()}`\n" +
					$"**Online status:** `{user.Status}`",
				ThumbnailUrl = user.GetAvatarUrl(),
				Author = user.CreateAuthor(),
				Footer = new EmbedFooterBuilder { Text = "User Info", },
			};

			if (!(user is IGuildUser guildUser))
			{
				return Success(embed);
			}

			var channels = new List<string>();
			foreach (var t in (await guildUser.Guild.GetTextChannelsAsync().CAF()).OrderBy(x => x.Position))
			{
				var perms = guildUser.GetPermissions(t);
				if (perms.ViewChannel)
				{
					channels.Add(t.Name);
				}
			}
			foreach (var v in (await guildUser.Guild.GetVoiceChannelsAsync().CAF()).OrderBy(x => x.Position))
			{
				var perms = guildUser.GetPermissions(v);
				if (perms.ViewChannel && perms.Connect)
				{
					channels.Add($"{v.Name} (Voice)");
				}
			}

			var join = 1;
			foreach (var u in (await guildUser.Guild.GetUsersAsync().CAF()).OrderByJoinDate())
			{
				if (u.Id == user.Id)
				{
					break;
				}
				++join;
			}

			var roles = guildUser.GetRoles();

			embed.Description += "\n\n" +
				$"**Nickname:** `{guildUser.Nickname?.EscapeBackTicks() ?? "No nickname"}`\n" +
				$"**Joined:** `{guildUser.JoinedAt?.UtcDateTime.ToReadable()}` (`#{join}`)";
			embed.Color = roles.LastOrDefault(x => x.Color.RawValue != 0)?.Color;

			if (channels.Any())
			{
				embed.TryAddField("Channels", $"`{channels.Join("`, `")}`", false, out _);
			}
			if (roles.Any())
			{
				embed.TryAddField("Roles", $"`{roles.Join("`, `", x => x.Name)}`", false, out _);
			}
			if (guildUser.VoiceChannel != null)
			{
				var value = $"**Server Mute:** `{guildUser.IsMuted}`\n" +
					$"**Server Deafen:** `{guildUser.IsDeafened}`\n" +
					$"**Self Mute:** `{guildUser.IsSelfMuted}`\n" +
					$"**Self Deafen:** `{guildUser.IsSelfDeafened}`";
				embed.TryAddField($"Voice Channel: {guildUser.VoiceChannel.Name}", value, false, out _);
			}
			return Success(embed);
		}
		public static async Task<RuntimeResult> Role(IRole role)
		{
			var users = await role.Guild.GetUsersAsync().CAF();
			return Success(new EmbedWrapper
			{
				Description = FormatIdAndCreatedAt(role) +
					$"**Position:** `{role.Position}`\n" +
					$"**Color:** `#{role.Color.RawValue.ToString("X6")}`\n\n" +
					$"**Is Hoisted:** `{role.IsHoisted}`\n" +
					$"**Is Managed:** `{role.IsManaged}`\n" +
					$"**Is Mentionable:** `{role.IsMentionable}`\n\n" +
					$"**User Count:** `{users.Count(u => u.RoleIds.Any(r => r == role.Id))}`\n" +
					$"**Permissions:** `{string.Join("`, `", Enum.GetValues(typeof(GuildPermission)).Cast<GuildPermission>().Where(x => role.Permissions.Has(x)))}`",
				Color = role.Color,
				Author = new EmbedAuthorBuilder { Name = role.Format(), },
				Footer = new EmbedFooterBuilder { Text = "Role Info", },
			});
		}
		public static async Task<RuntimeResult> Channel(IGuildChannel channel, IGuildSettings guildSettings)
		{
			var overwriteNames = new List<string>();
			foreach (var o in channel.PermissionOverwrites)
			{
				if (o.TargetType == PermissionTarget.Role)
				{
					overwriteNames.Add(channel.Guild.GetRole(o.TargetId).Name);
				}
				else if (o.TargetType == PermissionTarget.User)
				{
					var user = await channel.Guild.GetUserAsync(o.TargetId).CAF();
					overwriteNames.Add(user.Username);
				}
			}
			var userCount = (await channel.GetUsersAsync().FlattenAsync().CAF()).Count();
			return Success(new EmbedWrapper
			{
				Description = FormatIdAndCreatedAt(channel) +
					$"**Position:** `{channel.Position}`\n" +
					$"**Is Ignored From Log:** `{guildSettings.IgnoredLogChannels.Contains(channel.Id)}`\n" +
					$"**Is Ignored From Commands:** `{guildSettings.IgnoredCommandChannels.Contains(channel.Id)}`\n" +
					$"**Is Image Only:** `{guildSettings.ImageOnlyChannels.Contains(channel.Id)}`\n" +
					$"**Is Serverlog:** `{guildSettings.ServerLogId == channel.Id}`\n" +
					$"**Is Modlog:** `{guildSettings.ModLogId == channel.Id}`\n" +
					$"**Is Imagelog:** `{guildSettings.ImageLogId == channel.Id}`\n\n" +
					$"**User Count:** `{userCount}`\n" +
					$"**Overwrites:** `{overwriteNames.Join("`, `")}`",
				Author = new EmbedAuthorBuilder { Name = channel.Format(), },
				Footer = new EmbedFooterBuilder { Text = "Channel Info", },
			});
		}
		public static async Task<RuntimeResult> AllGuildUsers(IGuild guild)
		{
			var users = await guild.GetUsersAsync().CAF();
			var statuses = Enum.GetValues(typeof(UserStatus)).Cast<UserStatus>().ToDictionary(x => x, x => 0);
			var activities = Enum.GetValues(typeof(ActivityType)).Cast<ActivityType>().ToDictionary(x => x, x => 0);
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

			var embed = new EmbedWrapper
			{
				Description = $"**Count:** `{users.Count}`\n" +
					$"**Bots:** `{bots}`\n" +
					$"**Webhooks:** `{webhooks}`\n" +
					$"**In Voice:** `{voice}`\n" +
					$"**Has Nickname:** `{nickname}`\n",
				Author = new EmbedAuthorBuilder { Name = "Guild Users", },
				Footer = new EmbedFooterBuilder { Text = "Guild Users Info", },
			};
			embed.TryAddField("Statuses", statuses.Join("\n", kvp => $"**{kvp.Key.ToString().FormatTitle()}:** `{kvp.Value}`"), false, out _);
			embed.TryAddField("Activities", activities.Join("\n", kvp => $"**{kvp.Key.ToString().FormatTitle()}:** `{kvp.Value}`"), false, out _);
			return Success(embed);
		}
		public static AdvobotResult Emote(Emote emote)
		{
			var embed = new EmbedWrapper
			{
				Description = FormatIdAndCreatedAt(emote),
				ThumbnailUrl = emote.Url,
				Author = new EmbedAuthorBuilder { Name = FormatIdAndCreatedAt(emote), },
				Footer = new EmbedFooterBuilder { Text = "Emote Info", },
			};

			if (!(emote is GuildEmote guildEmote))
			{
				return Success(embed);
			}

			embed.Description += $"**Is Managed:** `{guildEmote.IsManaged}`\n" +
				$"**Requires Colons:** `{guildEmote.RequireColons}`\n\n" +
				$"**Roles:** `{guildEmote.RoleIds.Join("`, `", x => x.ToString())}`";
			return Success(embed);
		}
		public static AdvobotResult Invite(IInviteMetadata invite)
		{
			return Success(new EmbedWrapper
			{
				Description = $"{invite.CreatedAt.GetValueOrDefault().UtcDateTime.ToCreatedAt()}\n" +
					$"**Inviter:** `{invite.Inviter.Format()}`\n" +
					$"**Channel:** `{invite.Channel.Format()}`\n" +
					$"**Uses:** `{invite.Uses}`",
				Author = new EmbedAuthorBuilder { Name = invite.Code, },
				Footer = new EmbedFooterBuilder { Text = "Invite Info", },
			});
		}
		public static AdvobotResult Webhook(IWebhook webhook)
		{
			return Success(new EmbedWrapper
			{
				Description = FormatIdAndCreatedAt(webhook) +
					$"**Creator:** `{webhook.Creator.Format()}`\n" +
					$"**Channel:** `{webhook.Channel.Format()}`\n",
				ThumbnailUrl = webhook.GetAvatarUrl(),
				Author = new EmbedAuthorBuilder { Name = webhook.Name, IconUrl = webhook.GetAvatarUrl(), Url = webhook.GetAvatarUrl(), },
				Footer = new EmbedFooterBuilder { Text = "Webhook Info", },
			});
		}
		public static AdvobotResult UsersWithReason(string title, IEnumerable<IGuildUser> users)
		{
			return Success(new EmbedWrapper
			{
				Title = title,
				Description = users.FormatNumberedList(x => x.Format()),
			});
		}
		public static AdvobotResult UserJoinPosition(IGuildUser user, int position)
			=> Success(Default.FormatInterpolated($"{user} is #{position} to join the guild on {user.JoinedAt?.UtcDateTime.ToReadable()}."));
		public static AdvobotResult Guilds(IReadOnlyCollection<IGuild> guilds)
		{
			return Success(new EmbedWrapper
			{
				Title = "Guilds",
				Description = BigBlock.FormatInterpolated($"{guilds.FormatNumberedList(x => Default.FormatInterpolated($"{x} Owner: {x.OwnerId}"))}"),
			});
		}
		public static AdvobotResult UserJoin(IReadOnlyCollection<IGuildUser> users)
		{
			return Success(new TextFileInfo
			{
				Name = "User_Joins",
				Text = users.FormatNumberedList(x => Default.FormatInterpolated($"{x} joined on {x.JoinedAt?.UtcDateTime.ToReadable()}")),
			});
		}
		public static AdvobotResult Messages(IChannel channel, IMessage[] messages, int maxSize)
		{
			var formattedMessagesBuilder = new StringBuilder();
			for (var count = 0; count < messages.Length; ++count)
			{
				var text = messages[count].Format(withMentions: false).RemoveAllMarkdown().RemoveDuplicateNewLines();
				if (formattedMessagesBuilder.Length + text.Length >= maxSize)
				{
					break;
				}
				formattedMessagesBuilder.AppendLineFeed(text);
			}

			return Success(new TextFileInfo
			{
				Name = $"{channel.Name}_Messages",
				Text = formattedMessagesBuilder.ToString(),
			});
		}
		public static AdvobotResult ShowEnumNames<T>(ulong value) where T : struct, Enum
			=> Success(Default.FormatInterpolated($"{value} has the following permissions: {EnumUtils.GetFlagNames((T)(object)value)}"));
		public static AdvobotResult ShowAllEnums(IEnumerable<Type> enums)
		{
			return Success(new EmbedWrapper
			{
				Title = "Enum Names",
				Description = Default.FormatInterpolated($"{enums.Select(x => x.Name)}"),
			});
		}
		public static AdvobotResult ShowEnumValues(Type enumType)
		{
			return Success(new EmbedWrapper
			{
				Title = enumType.Name,
				Description = Default.FormatInterpolated($"{Enum.GetNames(enumType)}"),
			});
		}

		private static string FormatIdAndCreatedAt(ISnowflakeEntity obj)
			=> $"**Id:** `{obj.Id}`\n{obj.CreatedAt.UtcDateTime.ToCreatedAt()}\n\n";
	}
}
