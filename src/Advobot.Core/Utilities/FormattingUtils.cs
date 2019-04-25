using System;
using System.Linq;
using System.Text;
using Advobot.Classes;
using Advobot.Interfaces;
using AdvorangesUtils;
using Discord;
using Discord.WebSocket;

namespace Advobot.Utilities
{
    /// <summary>
    /// Formatting for information about Discord objects.
    /// </summary>
    public static class FormattingUtils
    {
        /// <summary>
        /// Returns a string with the object's name and id.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string Format(this ISnowflakeEntity obj)
        {
            switch (obj)
            {
                case IUser user:
                    return user.Format();
                case IRole role:
                    return role.Format();
                case IChannel channel:
                    return channel.Format();
                case IGuild guild:
                    return guild.Format();
                case IMessage message:
                    return message.Format(true);
                case IActivity activity:
                    return activity.Format();
                case IWebhook webhook:
                    return webhook.Format();
                case IInviteMetadata invite:
                    return invite.Format();
                default:
                    return obj?.ToString();
            }
        }
        /// <summary>
        /// Returns a string with the user's name, discriminator and id.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static string Format(this IUser user)
            => user != null ? $"'{user.Username.EscapeBackTicks()}#{user.Discriminator}' ({user.Id})" : "Irretrievable User";
        /// <summary>
        /// Returns a string with the role's name and id.
        /// </summary>
        /// <param name="role"></param>
        /// <returns></returns>
        public static string Format(this IRole role)
            => role != null ? $"'{role.Name.EscapeBackTicks()}' ({role.Id})" : "Irretrievable Role";
        /// <summary>
        /// Returns a string with the channel's name and id.
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        public static string Format(this IChannel channel)
            => channel != null ? $"'{channel.Name.EscapeBackTicks()}' ({channel.GetChannelType()}) ({channel.Id})" : "Irretrievable Channel";
        /// <summary>
        /// Returns a string with the guild's name and id.
        /// </summary>
        /// <param name="guild"></param>
        /// <returns></returns>
        public static string Format(this IGuild guild)
            => guild != null ? $"'{guild.Name.EscapeBackTicks()}' ({guild.Id})" : "Irretrievable Guild";
        /// <summary>
        /// Returns a string with the messages content, embeds, and attachments listed.
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="withMentions"></param>
        /// <returns></returns>
        public static string Format(this IMessage msg, bool withMentions)
        {
            var text = string.IsNullOrEmpty(msg.Content) ? "Empty message content" : msg.Content;
            var time = msg.CreatedAt.ToString("HH:mm:ss");
            var header = withMentions
                ? $"`[{time}]` {((ITextChannel)msg.Channel).Mention} {msg.Author.Mention} `{msg.Id}`"
                : $"`[{time}]` `{msg.Channel.Format()}` `{msg.Author.Format()}` `{msg.Id}`";

            var sb = new StringBuilder($"{header}\n```\n{text.EscapeBackTicks()}");

            var currentEmbed = 1;
            foreach (var embed in msg.Embeds)
            {
                if (embed.Description == null && embed.Url == null && !embed.Image.HasValue)
                {
                    continue;
                }

                sb.Append($"Embed {currentEmbed}: {embed.Description ?? "No description"}".EscapeBackTicks());
                if (embed.Url != null)
                {
                    sb.Append($" URL: {embed.Url}");
                }
                if (embed.Image.HasValue)
                {
                    sb.Append($" IURL: {embed.Image.Value.Url}");
                }
                sb.AppendLineFeed();
                ++currentEmbed;
            }
            var attachments = msg.Attachments.Select(x => x.Filename).ToArray();
            if (attachments.Any())
            {
                sb.AppendLineFeed($" + {string.Join(" + ", attachments).EscapeBackTicks()}");
            }
            return sb.Append("```").ToString();
        }
        /// <summary>
        /// Returns a string with the game's name or stream name/url.
        /// </summary>
        /// <param name="activity"></param>
        /// <returns></returns>
        public static string Format(this IActivity activity)
        {
            switch (activity)
            {
                case SpotifyGame sp:
                    return $"Listening to {sp.TrackTitle}";
                case StreamingGame sg:
                    return $"Streaming {sg.Name.EscapeBackTicks()} at {sg.Url}";
                case RichGame rg:
                    return $"Playing {rg.Name.EscapeBackTicks()} ({rg.State.EscapeBackTicks()})";
                case Game g:
                    return $"Playing {g.Name.EscapeBackTicks()}";
                default:
                    return "N/A";
            }
        }
        /// <summary>
        /// Returns a string with the webhook's name and id.
        /// </summary>
        /// <param name="webhook"></param>
        /// <returns></returns>
        public static string Format(this IWebhook webhook)
            => webhook != null ? $"'{webhook.Name.EscapeBackTicks()}' ({webhook.Id})" : "Irretrievable Webhook";
        /// <summary>
        /// Formats information about an invite.
        /// </summary>
        /// <param name="invite"></param>
        /// <returns></returns>
        public static string Format(this IInviteMetadata invite)
        {
            const string INF = "\u221E"; //∞
            if (invite == null)
            {
                return "Irretrievable Invite";
            }

            var uses = invite.MaxUses.HasValue ? invite.MaxUses.Value.ToString() : INF;
            var time = invite.MaxAge.HasValue ? (invite.MaxAge.Value / 60).ToString() : INF;
            var temp = invite.IsTemporary ? ", temp" : "";
            return $"'{invite.Code}' ({uses} uses, {time} minutes{temp})";
        }
		/// <summary>
		/// Invokes <see cref="string.Format(IFormatProvider, string, object)"/>
		/// </summary>
		/// <param name="provider"></param>
		/// <param name="format"></param>
		/// <param name="arg0"></param>
		/// <returns></returns>
		public static string Format(this IFormatProvider provider, string format, object arg0)
			=> string.Format(provider, format, arg0);
		/// <summary>
		/// Invokes <see cref="string.Format(IFormatProvider, string, object, object)"/>.
		/// </summary>
		/// <param name="provider"></param>
		/// <param name="format"></param>
		/// <param name="arg0"></param>
		/// <param name="arg1"></param>
		/// <returns></returns>
		public static string Format(this IFormatProvider provider, string format, object arg0, object arg1)
			=> string.Format(provider, format, arg0, arg1);
        /// <summary>
        /// Invokes <see cref="string.Format(IFormatProvider, string, object[])"/>.
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="format"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static string Format(this IFormatProvider provider, string format, object[] args)
            => string.Format(provider, format, args);
		/// <summary>
		/// Formats the interpolated string with the specified format provider.
		/// </summary>
		/// <param name="provider"></param>
		/// <param name="formattable"></param>
		/// <returns></returns>
		public static string FormatInterpolated(this IFormatProvider provider, IFormattable formattable)
			=> formattable.ToString(null, provider);

        /// <summary>
        /// Returns a new <see cref="EmbedWrapper"/> containing information about a user on a guild.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static EmbedWrapper FormatGuildUserInfo(SocketGuildUser user)
        {
            var guild = user.Guild;
            var textChannels = guild.TextChannels.Where(x => user.GetPermissions(x).ViewChannel).OrderBy(x => x.Position).Select(x => x.Name);
            var voiceChannels = guild.VoiceChannels.Where(x =>
            {
                var perms = user.GetPermissions(x);
                return perms.ViewChannel && perms.Connect;
            }).OrderBy(x => x.Position).Select(x => x.Name + " (Voice)");
            var channels = textChannels.Concat(voiceChannels).ToList();
            var roles = user.Roles.OrderBy(x => x.Position).Where(x => !x.IsEveryone).ToList();
            var join = guild.GetUsersByJoinDate().Select((Val, Index) => new { Val.Id, Index }).First(x => x.Id == user.Id).Index + 1;

            var embed = new EmbedWrapper
            {
                Description = user.FormatInfo() +
                    $"**Nickname:** `{user.Nickname?.EscapeBackTicks() ?? "No nickname"}`\n" +
                    $"**Joined:** `{user.JoinedAt?.UtcDateTime.ToReadable()}` (`#{join}`)\n\n" +
                    $"**Activity:** `{user.Activity.Format()}`\n" +
                    $"**Online status:** `{user.Status}`\n",
                Color = roles.LastOrDefault(x => x.Color.RawValue != 0)?.Color,
                ThumbnailUrl = user.GetAvatarUrl(),
                Author = user.CreateAuthor(),
                Footer = new EmbedFooterBuilder { Text = "Guild User Info", },
            };

            if (channels.Any())
            {
                embed.TryAddField("Channels", $"`{string.Join("`, `", channels)}`", false, out _);
            }
            if (roles.Any())
            {
                embed.TryAddField("Roles", $"`{roles.Join("`, `", x => x.Name)}`", false, out _);
            }
            if (user.VoiceChannel != null)
            {
                var value = $"**Server Mute:** `{user.IsMuted}`\n" +
                    $"**Server Deafen:** `{user.IsDeafened}`\n" +
                    $"**Self Mute:** `{user.IsSelfMuted}`\n" +
                    $"**Self Deafen:** `{user.IsSelfDeafened}`";
                embed.TryAddField($"Voice Channel: {user.VoiceChannel.Name}", value, false, out _);
            }
            return embed;
        }
        /// <summary>
        /// Returns a new <see cref="EmbedWrapper"/> containing information about a user not on a guild.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static EmbedWrapper FormatUserInfo(SocketUser user)
        {
            return new EmbedWrapper
            {
                Description = user.FormatInfo() +
                    $"**Activity:** `{user.Activity.Format()}`\n" +
                    $"**Online status:** `{user.Status}`",
                ThumbnailUrl = user.GetAvatarUrl(),
                Author = user.CreateAuthor(),
                Footer = new EmbedFooterBuilder { Text = "Global User Info", },
            };
        }
        /// <summary>
        /// Returns a new <see cref="EmbedWrapper"/> containing information about a role.
        /// </summary>
        /// <param name="role"></param>
        /// <returns></returns>
        public static EmbedWrapper FormatRoleInfo(SocketRole role)
        {
            return new EmbedWrapper
            {
                Description = role.FormatInfo() +
                    $"**Position:** `{role.Position}`\n" +
                    $"**Color:** `#{role.Color.RawValue.ToString("X6")}`\n\n" +
                    $"**Is Hoisted:** `{role.IsHoisted}`\n" +
                    $"**Is Managed:** `{role.IsManaged}`\n" +
                    $"**Is Mentionable:** `{role.IsMentionable}`\n\n" +
                    $"**User Count:** `{role.Guild.Users.Count(u => u.Roles.Any(r => r.Id == role.Id))}`\n" +
                    $"**Permissions:** `{string.Join("`, `", Enum.GetValues(typeof(GuildPermission)).Cast<GuildPermission>().Where(x => role.Permissions.Has(x)))}`",
                Color = role.Color,
                Author = new EmbedAuthorBuilder { Name = role.Format(), },
                Footer = new EmbedFooterBuilder { Text = "Role Info", },
            };
        }
        /// <summary>
        /// Returns a new <see cref="EmbedWrapper"/> containing information about a channel.
        /// </summary>
        /// <param name="guildSettings"></param>
        /// <param name="channel"></param>
        /// <returns></returns>
        public static EmbedWrapper FormatChannelInfo(SocketGuildChannel channel, IGuildSettings guildSettings)
        {
            var overwriteNames = channel.PermissionOverwrites.Select(o =>
            {
                switch (o.TargetType)
                {
                    case PermissionTarget.Role:
                        return channel.Guild.GetRole(o.TargetId).Name;
                    case PermissionTarget.User:
                        return channel.Guild.GetUser(o.TargetId).Username;
                    default:
                        throw new InvalidOperationException("Invalid overwrite target type.");
                }
            });
            return new EmbedWrapper
            {
                Description = channel.FormatInfo() +
                    $"**Position:** `{channel.Position}`\n" +
                    $"**Is Ignored From Log:** `{guildSettings.IgnoredLogChannels.Contains(channel.Id)}`\n" +
                    $"**Is Ignored From Commands:** `{guildSettings.IgnoredCommandChannels.Contains(channel.Id)}`\n" +
                    $"**Is Image Only:** `{guildSettings.ImageOnlyChannels.Contains(channel.Id)}`\n" +
                    $"**Is Serverlog:** `{guildSettings.ServerLogId == channel.Id}`\n" +
                    $"**Is Modlog:** `{guildSettings.ModLogId == channel.Id}`\n" +
                    $"**Is Imagelog:** `{guildSettings.ImageLogId == channel.Id}`\n\n" +
                    $"**User Count:** `{channel.Users.Count}`\n" +
                    $"**Overwrites:** `{string.Join("`, `", overwriteNames)}`",
                Author = new EmbedAuthorBuilder { Name = channel.Format(), },
                Footer = new EmbedFooterBuilder { Text = "Channel Info", },
            };
        }
        /// <summary>
        /// Returns a new <see cref="EmbedWrapper"/> containing information about a guild.
        /// </summary>
        /// <param name="guild"></param>
        /// <returns></returns>
        public static EmbedWrapper FormatGuildInfo(SocketGuild guild)
        {
            int local = 0, animated = 0, managed = 0;
            foreach (var emote in guild.Emotes)
            {
                if (emote.IsManaged) { ++managed; }
                if (emote.Animated) { ++animated; }
                else { ++local; }
            }

            return new EmbedWrapper
            {
                Description = guild.FormatInfo() +
                    $"**Owner:** `{guild.Owner.Format()}`\n" +
                    $"**Default Message Notifs:** `{guild.DefaultMessageNotifications}`\n" +
                    $"**Verification Level:** `{guild.VerificationLevel}`\n" +
                    $"**Region:** `{guild.VoiceRegionId}`\n\n" +
                    $"**Emotes:** `{guild.Emotes.Count}` (`{local}` local, `{animated}` animated, `{managed}` managed)\n" +
                    $"**User Count:** `{guild.MemberCount}`\n" +
                    $"**Role Count:** `{guild.Roles.Count}`\n" +
                    $"**Channel Count:** `{guild.Channels.Count}` " +
                        $"(`{guild.TextChannels.Count}` text, " +
                        $"`{guild.VoiceChannels.Count}` voice, " +
                        $"`{guild.CategoryChannels.Count}` categories)\n\n" +
                    $"**Default Channel:** `{guild.DefaultChannel?.Format() ?? "None"}`\n" +
                    $"**AFK Channel:** `{guild.AFKChannel?.Format() ?? "None"}` (Time: `{guild.AFKTimeout / 60}`)\n" +
                    $"**System Channel:** `{guild.SystemChannel?.Format() ?? "None"}`\n" +
                    $"**Embed Channel:** `{guild.EmbedChannel?.Format() ?? "None"}`\n" +
                    (guild.Features.Any() ? $"**Features:** `{string.Join("`, `", guild.Features)}`" : ""),
                Color = guild.Owner.Roles.OrderBy(x => x.Position).Where(x => !x.IsEveryone).LastOrDefault(x => x.Color.RawValue != 0)?.Color,
                ThumbnailUrl = guild.IconUrl,
                Author = new EmbedAuthorBuilder { Name = guild.Format(), },
                Footer = new EmbedFooterBuilder { Text = "Guild Info", },
            };
        }
        /// <summary>
        /// Returns a new <see cref="EmbedWrapper"/> containing information about every member in the guild.
        /// </summary>
        /// <param name="guild"></param>
        /// <returns></returns>
        public static EmbedWrapper FormatAllGuildUsersInfo(SocketGuild guild)
        {
            int offline = 0, online = 0, idle = 0, afk = 0, donotdisturb = 0,
                playing = 0, listening = 0, watching = 0, streaming = 0,
                webhooks = 0, bots = 0, nickname = 0, voice = 0;
            foreach (var user in guild.Users)
            {
                switch (user.Status)
                {
                    case UserStatus.Offline:
                        ++offline;
                        break;
                    case UserStatus.Online:
                        ++online;
                        break;
                    case UserStatus.Idle:
                        ++idle;
                        break;
                    case UserStatus.AFK:
                        ++afk;
                        break;
                    case UserStatus.DoNotDisturb:
                        ++donotdisturb;
                        break;
                }
                switch (user.Activity?.Type)
                {
                    case ActivityType.Playing:
                        ++playing;
                        break;
                    case ActivityType.Listening:
                        ++listening;
                        break;
                    case ActivityType.Streaming:
                        ++streaming;
                        break;
                    case ActivityType.Watching:
                        ++watching;
                        break;
                }
                if (user.IsWebhook) { ++webhooks; }
                if (user.IsBot) { ++bots; }
                if (user.Nickname != null) { ++nickname; }
                if (user.VoiceChannel != null) { ++voice; }
            }

            var embed = new EmbedWrapper
            {
                Description = $"**Count:** `{guild.Users.Count}`\n" +
                    $"**Bots:** `{bots}`\n" +
                    $"**Webhooks:** `{webhooks}`\n" +
                    $"**In Voice:** `{voice}`\n" +
                    $"**Has Nickname:** `{nickname}`\n",
                Author = new EmbedAuthorBuilder { Name = "Guild Users", },
                Footer = new EmbedFooterBuilder { Text = "Guild Users Info", },
            };
            var statuses = $"**Offline:** `{offline}`\n" +
                $"**Online:** `{online}`\n" +
                $"**Idle:** `{idle}`\n" +
                $"**AFK:** `{afk}`\n" +
                $"**Do Not Disturb:** `{donotdisturb}`";
            embed.TryAddField("Statuses", statuses, false, out _);
            var activities = $"**Playing Games:** `{playing}`\n" +
                $"**Streaming:** `{streaming}`\n" +
                $"**Listening:** `{listening}`\n" +
                $"**Watching:** `{watching}`";
            embed.TryAddField("Activities", activities, false, out _);
            return embed;
        }
        /// <summary>
        /// Returns a new <see cref="EmbedWrapper"/> containing information about an emote.
        /// </summary>
        /// <param name="emote"></param>
        /// <returns></returns>
        public static EmbedWrapper FormatEmoteInfo(Emote emote)
        {
            return new EmbedWrapper
            {
                Description = emote.FormatInfo(),
                ThumbnailUrl = emote.Url,
                Author = new EmbedAuthorBuilder { Name = emote.FormatInfo(), },
                Footer = new EmbedFooterBuilder { Text = "Emote Info", },
            };
        }
        /// <summary>
        /// Returns a new <see cref="EmbedWrapper"/> containing information about a guild emote.
        /// </summary>
        /// <param name="guild"></param>
        /// <param name="emote"></param>
        /// <returns></returns>
        public static EmbedWrapper FormatGuildEmoteInfo(SocketGuild guild, GuildEmote emote)
        {
            return new EmbedWrapper
            {
                Description = emote.FormatInfo() +
                    $"**Is Managed:** `{emote.IsManaged}`\n" +
                    $"**Requires Colons:** `{emote.RequireColons}`\n\n" +
                    $"**Roles:** `{emote.RoleIds.Select(x => guild.GetRole(x)).OrderBy(x => x.Position).Join("`, `", x => x.Name)}`",
                ThumbnailUrl = emote.Url,
                Author = new EmbedAuthorBuilder { Name = emote.FormatInfo(), },
                Footer = new EmbedFooterBuilder { Text = "Guild Emote Info", },
            };
        }
        /// <summary>
        /// Returns a new <see cref="EmbedWrapper"/> containing information about an invite.
        /// </summary>
        /// <param name="invite"></param>
        /// <returns></returns>
        public static EmbedWrapper FormatInviteInfo(IInviteMetadata invite)
        {
            return new EmbedWrapper
            {
                Description = $"{invite.CreatedAt.GetValueOrDefault().UtcDateTime.ToCreatedAt()}\n" +
                    $"**Inviter:** `{invite.Inviter.Format()}`\n" +
                    $"**Channel:** `{invite.Channel.Format()}`\n" +
                    $"**Uses:** `{invite.Uses}`",
                Author = new EmbedAuthorBuilder { Name = invite.Code, },
                Footer = new EmbedFooterBuilder { Text = "Invite Info", },
            };
        }
        /// <summary>
        /// Returns a new <see cref="EmbedWrapper"/> containing information about a webhook.
        /// </summary>
        /// <param name="guild"></param>
        /// <param name="webhook"></param>
        /// <returns></returns>
        public static EmbedWrapper FormatWebhookInfo(SocketGuild guild, IWebhook webhook)
        {
            return new EmbedWrapper
            {
                Description = webhook.FormatInfo() +
                    $"**Creator:** `{webhook.Creator.Format()}`\n" +
                    $"**Channel:** `{guild.GetChannel(webhook.ChannelId).Format()}`\n",
                ThumbnailUrl = webhook.GetAvatarUrl(),
                Author = new EmbedAuthorBuilder { Name = webhook.Name, IconUrl = webhook.GetAvatarUrl(), Url = webhook.GetAvatarUrl(), },
                Footer = new EmbedFooterBuilder { Text = "Webhook Info", },
            };
        }
        /// <summary>
        /// Returns a new <see cref="EmbedWrapper"/> containing information about the bot.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="logging"></param>
        /// <returns></returns>
        public static EmbedWrapper FormatBotInfo(DiscordShardedClient client, ILogService logging)
        {
            string FormatLogCounters(ILogCounter[] counters)
            {
                var titlesAndCount = new (string Title, string Count)[counters.Length];
                var rightSpacing = 0;
                var leftSpacing = 0;
                for (int i = 0; i < counters.Length; ++i)
                {
                    var counter = counters[i];
                    var temp = (Title: $"**{counter.Name}**:", Count: $"`{counter.Count}`");
                    titlesAndCount[i] = temp;
                    rightSpacing = Math.Max(rightSpacing, temp.Title.Length);
                    leftSpacing = Math.Max(leftSpacing, temp.Count.Length);
                }

                var sb = new StringBuilder();
                foreach (var (Title, Count) in titlesAndCount)
                {
                    sb.AppendLineFeed($"{Title.PadRight(Math.Max(rightSpacing + 1, 0))}{Count.PadLeft(Math.Max(leftSpacing, 0))}");
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
                Author = CreateAuthor(client.CurrentUser),
                Footer = new EmbedFooterBuilder { Text = $"Versions [Bot: {Version.VERSION_NUMBER}] [API: {Constants.API_VERSION}]", },
            };
            var userCounters = new[] { logging.UserJoins, logging.UserLeaves, logging.UserChanges };
            embed.TryAddField("Users", FormatLogCounters(userCounters), true, out _);
            var msgCounters = new[] { logging.MessageEdits, logging.MessageDeletes, logging.Images, logging.Animated, logging.Files };
            embed.TryAddField("Messages", FormatLogCounters(msgCounters), true, out _);
            var cmdCounters = new[] { logging.AttemptedCommands, logging.SuccessfulCommands, logging.FailedCommands };
            embed.TryAddField("Commands", FormatLogCounters(cmdCounters), true, out _);
            return embed;
        }
        /// <summary>
        /// Returns a new <see cref="EmbedWrapper"/> containing information about the bot's shards.
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        public static EmbedWrapper FormatShardsInfo(DiscordShardedClient client)
        {
            var shardInfo = "";
            foreach (var shard in client.Shards)
            {
                var statusEmoji = "";
                switch (shard.ConnectionState)
                {
                    case ConnectionState.Disconnected:
                    case ConnectionState.Disconnecting:
                        statusEmoji = "\u274C"; //❌
                        break;
                    case ConnectionState.Connected:
                    case ConnectionState.Connecting:
                        statusEmoji = "\u2705"; //✅
                        break;
                }
                shardInfo += $"Shard `{shard.ShardId}`: `{statusEmoji} ({shard.Latency}ms)`\n";
            }
            return new EmbedWrapper
            {
                Description = shardInfo,
                Author = client.CurrentUser.CreateAuthor(),
            };
        }
        /// <summary>
        /// Returns a new <see cref="EmbedAuthorBuilder"/> containing the user's info.
        /// </summary>
        /// <param name="author"></param>
        /// <returns></returns>
        public static EmbedAuthorBuilder CreateAuthor(this IUser author)
            => new EmbedAuthorBuilder { IconUrl = author?.GetAvatarUrl(), Name = author?.Format(), Url = author?.GetAvatarUrl(), };

        private static string FormatInfo(this ISnowflakeEntity obj)
            => $"**Id:** `{obj.Id}`\n{obj.CreatedAt.UtcDateTime.ToCreatedAt()}\n\n";
        private static string GetChannelType(this IChannel channel)
        {
            switch (channel)
            {
                case IMessageChannel _:
                    return "text";
                case IVoiceChannel _:
                    return "voice";
                case ICategoryChannel _:
                    return "category";
                default:
                    return "unknown";
            }
        }
    }
}
