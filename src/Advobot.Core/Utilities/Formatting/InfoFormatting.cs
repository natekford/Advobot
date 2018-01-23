using System;
using System.Diagnostics;
using System.Linq;
using Advobot.Core.Classes;
using Advobot.Core.Interfaces;
using Discord;
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
		public static EmbedWrapper FormatUserInfo(SocketGuild guild, SocketGuildUser user)
		{
			var textChannels = guild.TextChannels.Where(x => user.GetPermissions(x).ViewChannel).OrderBy(x => x.Position).Select(x => x.Name);
			var voiceChannels = guild.VoiceChannels.Where(x =>
			{
				var perms = user.GetPermissions(x);
				return perms.ViewChannel && perms.Connect;
			}).OrderBy(x => x.Position).Select(x => x.Name + " (Voice)");
			var channels = textChannels.Concat(voiceChannels).ToList();
			var users = guild.Users.Where(x => x.JoinedAt != null).OrderBy(x => x.JoinedAt?.Ticks ?? 0).ToList();
			var roles = user.Roles.OrderBy(x => x.Position).Where(x => !x.IsEveryone).ToList();

			var desc = $"**Id:** `{user.Id}`\n" +
				$"**Nickname:** `{(String.IsNullOrWhiteSpace(user.Nickname) ? "No nickname" : user.Nickname.EscapeBackTicks())}`\n" +
				$"{user.CreatedAt.UtcDateTime.CreatedAt()}\n" +
				$"**Joined:** `{user.JoinedAt?.UtcDateTime.Readable()}` (`{users.IndexOf(user) + 1}` to join the guild)\n\n" +
				$"{user.Activity.Format()}\n" +
				$"**Online status:** `{user.Status}`\n";

			var color = roles.OrderBy(x => x.Position).LastOrDefault(x => x.Color.RawValue != 0)?.Color;
			var embed = new EmbedWrapper
			{
				Description = desc,
				Color = color,
				ThumbnailUrl = user.GetAvatarUrl()
			};
			embed.TryAddAuthor(user, out _);
			embed.TryAddFooter("User Info", null, out _);

			if (channels.Count() != 0)
			{
				embed.TryAddField("Channels", String.Join(", ", channels), false, out _);
			}
			if (roles.Count() != 0)
			{
				embed.TryAddField("Roles", String.Join(", ", roles.Select(x => x.Name)), false, out _);
			}
			if (user.VoiceChannel != null)
			{
				var value = $"Server mute: `{user.IsMuted}`\n" +
					$"Server deafen: `{user.IsDeafened}`\n" +
					$"Self mute: `{user.IsSelfMuted}`\n" +
					$"Self deafen: `{user.IsSelfDeafened}`";
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
		public static EmbedWrapper FormatUserInfo(IUser user)
		{
			var desc = $"{user.CreatedAt.UtcDateTime.CreatedAt()}\n" +
				$"{user.Activity.Format()}\n" +
				$"**Online status:** `{user.Status}`";

			var embed = new EmbedWrapper
			{
				Description = desc,
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
		public static EmbedWrapper FormatRoleInfo(SocketGuild guild, IRole role)
		{
			var desc = $"{role.CreatedAt.UtcDateTime.CreatedAt()}\n" +
				$"**Position:** `{role.Position}`" +
				$"**User Count:** `{guild.Users.Count(x => x.Roles.Any(y => y.Id == role.Id))}`";

			var embed = new EmbedWrapper
			{
				Description = desc,
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
		public static EmbedWrapper FormatChannelInfo(IGuildSettings guildSettings, SocketChannel channel)
		{
			var ignoredFromLog = guildSettings.IgnoredLogChannels.Contains(channel.Id);
			var ignoredFromCmd = guildSettings.IgnoredCommandChannels.Contains(channel.Id);
			var imageOnly = guildSettings.ImageOnlyChannels.Contains(channel.Id);
			var serverLog = guildSettings.ServerLog?.Id == channel.Id;
			var modLog = guildSettings.ModLog?.Id == channel.Id;
			var imageLog = guildSettings.ImageLog?.Id == channel.Id;

			var desc = $"{channel.CreatedAt.UtcDateTime.CreatedAt()}\n" +
				$"**User Count:** `{channel.Users.Count}`\n\n" +
				$"**Ignored From Log:** `{(ignoredFromLog ? "Yes" : "No")}`" +
				$"**Ignored From Commands:** `{(ignoredFromCmd ? "Yes" : "No")}`" +
				$"**Image Only:** `{(imageOnly ? "Yes" : "No")}`\n\n" +
				$"**Serverlog:** `{(serverLog ? "Yes" : "No")}`" +
				$"**Modlog:** `{(modLog ? "Yes" : "No")}`" +
				$"**Imagelog:** `{(imageLog ? "Yes" : "No")}`";

			var embed = new EmbedWrapper
			{
				Description = desc
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
			var owner = guild.Owner;
			var onlineCount = guild.Users.Count(x => x.Status != UserStatus.Offline);
			var nicknameCount = guild.Users.Count(x => x.Nickname != null);
			var gameCount = guild.Users.Count(x => x.Activity is Game);
			var botCount = guild.Users.Count(x => x.IsBot);
			var voiceCount = guild.Users.Count(x => x.VoiceChannel != null);
			var localECount = guild.Emotes.Count(x => !x.IsManaged);
			var globalECount = guild.Emotes.Count(x => x.IsManaged);

			var desc = $"{guild.CreatedAt.UtcDateTime.CreatedAt()}\n" +
				$"**Owner:** `{owner.Format()}`\n" +
				$"**Region:** `{guild.VoiceRegionId}`\n" +
				$"**Emotes:** `{localECount + globalECount}` (`{localECount}` local, `{globalECount}` global)\n\n" +
				$"**User Count:** `{guild.MemberCount}` (`{onlineCount}` online, `{botCount}` bots)\n" +
				$"**Users With Nickname:** `{nicknameCount}`\n" +
				$"**Users Playing Games:** `{gameCount}`\n" +
				$"**Users In Voice:** `{voiceCount}`\n\n" +
				$"**Role Count:** `{guild.Roles.Count}`\n" +
				$"**Channel Count:** `{guild.Channels.Count}` (`{guild.TextChannels.Count}` text, `{guild.VoiceChannels.Count}` voice)\n" +
				$"**AFK Channel:** `{guild.AFKChannel.Format()}` (`{guild.AFKTimeout / 60}` minute{GeneralFormatting.FormatPlural(guild.AFKTimeout / 60)})";

			var color = owner.Roles.FirstOrDefault(x => x.Color.RawValue != 0)?.Color;
			var embed = new EmbedWrapper
			{
				Description = desc,
				Color = color,
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
			var desc = $"**ID:** `{emote.Id}`";

			var embed = new EmbedWrapper
			{
				Description = desc,
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
			var desc = $"{invite.CreatedAt.UtcDateTime.CreatedAt()}\n" +
				$"**Inviter:** `{invite.Inviter.Format()}`\n" +
				$"**Channel:** `{invite.Channel.Format()}`\n" +
				$"**Uses:** `{invite.Uses}`";

			var embed = new EmbedWrapper
			{
				Description = desc
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
			var desc = $"**Online Since:** `{Process.GetCurrentProcess().StartTime.Readable()}` (`{TimeFormatting.Uptime()}`)\n" +
				$"**Guild/User Count:** `{logModule.TotalGuilds.Count}`/`{logModule.TotalUsers.Count}`\n" +
				$"**Current Shard:** `{ClientUtils.GetShardIdFor(client, guild)}`\n" +
				$"**Latency:** `{ClientUtils.GetLatency(client)}ms`\n" +
				$"**Memory Usage:** `{IOUtils.GetMemory():0.00)}MB`\n" +
				$"**Thread Count:** `{Process.GetCurrentProcess().Threads.Count}`\n";

			var firstField = logModule.FormatLoggedUserActions(true, false).Trim('\n', '\r');
			var secondField = logModule.FormatLoggedMessageActions(true, false).Trim('\n', '\r');
			var thirdField = logModule.FormatLoggedCommands(true, false).Trim('\n', '\r');

			var embed = new EmbedWrapper
			{
				Description = desc
			};
			embed.TryAddAuthor(client.CurrentUser, out _);
			embed.TryAddField("Users", firstField, false, out _);
			embed.TryAddField("Messages", secondField, false, out _);
			embed.TryAddField("Commands", thirdField, false, out _);
			embed.TryAddFooter($"Versions [Bot: {Version.VERSION_NUMBER}] [API: {Constants.ApiVersion}]", null, out _);
			return embed;
		}
	}
}
