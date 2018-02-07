using System;
using System.Diagnostics;
using System.Linq;
using Advobot.Core.Classes;
using Advobot.Core.Interfaces;
using Discord;
using Discord.Rest;
using Discord.WebSocket;

namespace Advobot.Core.Utilities.Formatting
{
	/// <summary>
	/// Formatting for information about Discord objects.
	/// </summary>
	public static class InfoFormatting
	{
		/// <summary>
		/// Returns a new <see cref="EmbedWrapper"/> containing information about a user on a guild.
		/// </summary>
		/// <param name="guild"></param>
		/// <param name="user"></param>
		/// <returns></returns>
		public static EmbedWrapper FormatUserInfo(SocketGuildUser user)
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

			var embed = new EmbedWrapper
			{
				Description = user.FormatInfo() +
					$"**Nickname:** `{user.Nickname?.EscapeBackTicks() ?? "No nickname"}`\n" +
					$"**Joined:** `{user.JoinedAt?.UtcDateTime.ToReadable()}` " +
						$"(`{guild.Users.OrderBy(x => x.JoinedAt?.Ticks ?? 0).Select(x => x.Id).ToList().IndexOf(user.Id) + 1}` to join the guild)\n\n" +
					$"{user.Activity.Format()}\n" +
					$"**Online status:** `{user.Status}`\n",
				Color = roles.LastOrDefault(x => x.Color.RawValue != 0)?.Color,
				ThumbnailUrl = user.GetAvatarUrl(),
			};
			embed.TryAddAuthor(user, out _);
			embed.TryAddFooter("User Info", null, out _);

			if (channels.Count() != 0)
			{
				embed.TryAddField("Channels", $"`{String.Join("`, `", channels)}`", false, out _);
			}
			if (roles.Count() != 0)
			{
				embed.TryAddField("Roles", $"`{String.Join(", ", roles.Select(x => x.Name))}`", false, out _);
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
		/// <param name="guild"></param>
		/// <param name="user"></param>
		/// <returns></returns>
		public static EmbedWrapper FormatUserInfo(SocketUser user)
		{
			var embed = new EmbedWrapper
			{
				Description = user.FormatInfo() +
					$"{user.Activity.Format()}\n" +
					$"**Online status:** `{user.Status}`",
				ThumbnailUrl = user.GetAvatarUrl()
			};
			embed.TryAddAuthor(user, out _);
			embed.TryAddFooter("User Info", null, out _);
			return embed;
		}
		/// <summary>
		/// Returns a new <see cref="EmbedWrapper"/> containing information about a role.
		/// </summary>
		/// <param name="guild"></param>
		/// <param name="role"></param>
		/// <returns></returns>
		public static EmbedWrapper FormatRoleInfo(SocketRole role)
		{
			var embed = new EmbedWrapper
			{
				Description = role.FormatInfo() +
					$"**Position:** `{role.Position}`\n" +
					$"**Color:** `#{role.Color.RawValue.ToString("X6")}`\n\n" +
					$"**Is Hoisted:** `{role.IsHoisted}`\n" +
					$"**Is Managed:** `{role.IsManaged}`\n" +
					$"**Is Mentionable:** `{role.IsMentionable}`\n\n" +
					$"**User Count:** `{role.Guild.Users.Count(u => u.Roles.Any(r => r.Id == role.Id))}`\n" +
					$"**Permissions:** `{String.Join("`, `", Enum.GetValues(typeof(GuildPermission)).Cast<GuildPermission>().Where(x => role.Permissions.Has(x)))}`",
				Color = role.Color
			};
			embed.TryAddAuthor(role.Format(), null, null, out _);
			embed.TryAddFooter("Role Info", null, out _);
			return embed;
		}
		/// <summary>
		/// Returns a new <see cref="EmbedWrapper"/> containing information about a channel.
		/// </summary>
		/// <param name="guildSettings"></param>
		/// <param name="channel"></param>
		/// <returns></returns>
		public static EmbedWrapper FormatChannelInfo(IGuildSettings guildSettings, SocketGuildChannel channel)
		{
			var overwriteNames = channel.PermissionOverwrites.Select(overwrite =>
			{
				switch (overwrite.TargetType)
				{
					case PermissionTarget.Role:
						return channel.Guild.GetRole(overwrite.TargetId).Name;
					case PermissionTarget.User:
						return channel.Guild.GetUser(overwrite.TargetId).Username;
					default:
						throw new InvalidOperationException("Invalid overwrite target type.");
				}
			});
			var embed = new EmbedWrapper
			{
				Description = channel.FormatInfo() +
					$"**Is Ignored From Log:** `{guildSettings.IgnoredLogChannels.Contains(channel.Id)}`\n" +
					$"**Is Ignored From Commands:** `{guildSettings.IgnoredCommandChannels.Contains(channel.Id)}`\n" +
					$"**Is Image Only:** `{guildSettings.ImageOnlyChannels.Contains(channel.Id)}`\n" +
					$"**Is Serverlog:** `{guildSettings.ServerLog?.Id == channel.Id}`\n" +
					$"**Is Modlog:** `{guildSettings.ModLog?.Id == channel.Id}`\n" +
					$"**Is Imagelog:** `{guildSettings.ImageLog?.Id == channel.Id}`\n\n" +
					$"**User Count:** `{channel.Users.Count}`\n" +
					$"**Overwrites:** `{String.Join("`, `", overwriteNames)}`",
			};
			embed.TryAddAuthor(channel.Format(), null, null, out _);
			embed.TryAddFooter("Channel Info", null, out _);
			return embed;
		}
		/// <summary>
		/// Returns a new <see cref="EmbedWrapper"/> containing information about a guild.
		/// </summary>
		/// <param name="guild"></param>
		/// <returns></returns>
		public static EmbedWrapper FormatGuildInfo(SocketGuild guild)
		{
			int online = 0, bots = 0, nickname = 0, voice = 0, playing = 0, listening = 0, watching = 0, streaming = 0;
			foreach (var user in guild.Users)
			{
				if (user.Status != UserStatus.Offline) { ++online; }
				if (user.IsBot) { ++bots; }
				if (user.Nickname != null) { ++nickname; }
				if (user.VoiceChannel != null) { ++voice; }
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
			}

			var embed = new EmbedWrapper
			{
				Description = guild.FormatInfo() +
					$"**Owner:** `{guild.Owner.Format()}`\n" +
					$"**Region:** `{guild.VoiceRegionId}`\n" +
					$"**Emotes:** `{guild.Emotes.Count}` " +
						$"(`{guild.Emotes.Count(x => !x.IsManaged)}` local, " +
						$"`{guild.Emotes.Count(x => x.IsManaged)}` global)\n\n" +
					$"**User Count:** `{guild.MemberCount}` (`{online}` online, `{bots}` bots)\n" +
					$"**Users With Nickname:** `{nickname}`\n" +
					$"**Users In Voice:** `{voice}`\n" +
					$"**Users Playing:** `{playing}`\n" +
					$"**Users Streaming:** `{streaming}`\n" +
					$"**Users Listening:** `{listening}`\n" +
					$"**Users Watching:** `{watching}`\n\n" +
					$"**Role Count:** `{guild.Roles.Count}`\n" +
					$"**Channel Count:** `{guild.Channels.Count}` " +
						$"(`{guild.TextChannels.Count}` text, " +
						$"`{guild.VoiceChannels.Count}` voice, " +
						$"`{guild.CategoryChannels.Count}` categories)\n" +
					$"**AFK Channel:** `{guild.AFKChannel.Format()}` " +
						$"(`{guild.AFKTimeout / 60}` minute{GeneralFormatting.FormatPlural(guild.AFKTimeout / 60)})",
				Color = guild.Owner.Roles.OrderBy(x => x.Position).Where(x => !x.IsEveryone).LastOrDefault(x => x.Color.RawValue != 0)?.Color,
				ThumbnailUrl = guild.IconUrl
			};
			embed.TryAddAuthor(guild.Format(), null, null, out _);
			embed.TryAddFooter("Guild Info", null, out _);
			return embed;
		}
		/// <summary>
		/// Returns a new <see cref="EmbedWrapper"/> containing information about an emote.
		/// </summary>
		/// <param name="emote"></param>
		/// <returns></returns>
		public static EmbedWrapper FormatEmoteInfo(Emote emote)
		{
			var embed = new EmbedWrapper
			{
				Description = emote.FormatInfo(),
				ThumbnailUrl = emote.Url
			};
			embed.TryAddAuthor(emote.Name, null, null, out _);
			embed.TryAddFooter("Emote Info", null, out _);
			return embed;
		}
		/// <summary>
		/// Returns a new <see cref="EmbedWrapper"/> containing information about an invite.
		/// </summary>
		/// <param name="invite"></param>
		/// <returns></returns>
		public static EmbedWrapper FormatInviteInfo(IInviteMetadata invite)
		{
			var embed = new EmbedWrapper
			{
				Description = $"{invite.CreatedAt.UtcDateTime.ToCreatedAt()}\n" +
					$"**Inviter:** `{invite.Inviter.Format()}`\n" +
					$"**Channel:** `{invite.Channel.Format()}`\n" +
					$"**Uses:** `{invite.Uses}`",
			};
			embed.TryAddAuthor(invite.Code, null, null, out _);
			embed.TryAddFooter("Invite Info", null, out _);
			return embed;
		}
		/// <summary>
		/// Returns a new <see cref="EmbedWrapper"/> containing information about the bot.
		/// </summary>
		/// <param name="globalInfo"></param>
		/// <param name="client"></param>
		/// <param name="logModule"></param>
		/// <param name="guild"></param>
		/// <returns></returns>
		public static EmbedWrapper FormatBotInfo(IDiscordClient client, ILogService logModule, IGuild guild)
		{
			var embed = new EmbedWrapper
			{
				Description = $"**Online Since:** `{Process.GetCurrentProcess().StartTime.ToReadable()}` (`{TimeFormatting.GetUptime()}`)\n" +
					$"**Guild/User Count:** `{logModule.TotalGuilds.Count}`/`{logModule.TotalUsers.Count}`\n" +
					$"**Current Shard:** `{ClientUtils.GetShardIdFor(client, guild)}`\n" +
					$"**Latency:** `{ClientUtils.GetLatency(client)}ms`\n" +
					$"**Memory Usage:** `{IOUtils.GetMemory():0.00}MB`\n" +
					$"**Thread Count:** `{Process.GetCurrentProcess().Threads.Count}`",
			};
			embed.TryAddAuthor(client.CurrentUser, out _);
			embed.TryAddField("Users", logModule.FormatLoggedUserActions(true, false).Trim('\n', '\r'), false, out _);
			embed.TryAddField("Messages", logModule.FormatLoggedMessageActions(true, false).Trim('\n', '\r'), false, out _);
			embed.TryAddField("Commands", logModule.FormatLoggedCommands(true, false).Trim('\n', '\r'), false, out _);
			embed.TryAddFooter($"Versions [Bot: {Version.VERSION_NUMBER}] [API: {Constants.API_VERSION}]", null, out _);
			return embed;
		}
		private static string FormatInfo(this ISnowflakeEntity obj)
		{
			return $"**Id:** `{obj.Id}`\n{obj.CreatedAt.UtcDateTime.ToCreatedAt()}\n\n";
		}
	}
}
