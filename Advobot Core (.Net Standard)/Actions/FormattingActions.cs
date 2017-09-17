using Advobot.Enums;
using Advobot.Interfaces;
using Discord;
using Discord.WebSocket;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Advobot.Actions
{
	public static class FormattingActions
	{
		private static readonly Regex _RemoveDuplicateSpaces = new Regex(@"[\r\n]+", RegexOptions.Compiled);

		/// <summary>
		/// Returns a new <see cref="EmbedBuilder"/> containing information about a user on a guild.
		/// </summary>
		/// <param name="guildSettings"></param>
		/// <param name="guild"></param>
		/// <param name="user"></param>
		/// <returns></returns>
		public static EmbedBuilder FormatUserInfo(IGuildSettings guildSettings, SocketGuild guild, SocketGuildUser user)
		{
			var textChannels = guild.TextChannels.Where(x => user.GetPermissions(x).ReadMessages).OrderBy(x => x.Position).Select(x => x.Name);
			var voiceChannels = guild.VoiceChannels.Where(x => user.GetPermissions(x).Connect).OrderBy(x => x.Position).Select(x => x.Name + " (Voice)");
			var channels = textChannels.Concat(voiceChannels);
			var users = guild.Users.Where(x => x.JoinedAt != null).OrderBy(x => x.JoinedAt.Value.Ticks).ToList();
			var roles = user.Roles.OrderBy(x => x.Position).Where(x => !x.IsEveryone);

			var desc = new StringBuilder()
				.AppendLineFeed($"**ID:** `{user.Id}`")
				.AppendLineFeed($"**Nickname:** `{(String.IsNullOrWhiteSpace(user.Nickname) ? "No nickname" : user.Nickname.EscapeBackTicks())}`")
				.AppendLineFeed(FormatDateTimeForCreatedAtMessage(user.CreatedAt.UtcDateTime))
				.AppendLineFeed($"**Joined:** `{FormatDateTime(user.JoinedAt.Value.UtcDateTime)}` (`{users.IndexOf(user) + 1}` to join the guild)\n")
				.AppendLineFeed(FormatGame(user))
				.AppendLineFeed($"**Online status:** `{user.Status}`");

			var color = roles.OrderBy(x => x.Position).LastOrDefault(x => x.Color.RawValue != 0)?.Color;
			var embed = EmbedActions.MakeNewEmbed(null, desc.ToString(), color, thumbnailUrl: user.GetAvatarUrl())
				.MyAddAuthor(user)
				.MyAddFooter("User Info");

			if (channels.Count() != 0)
			{
				embed.AddField("Channels", String.Join(", ", channels));
			}
			if (roles.Count() != 0)
			{
				embed.AddField("Roles", String.Join(", ", roles.Select(x => x.Name)));
			}
			if (user.VoiceChannel != null)
			{
				var value = new StringBuilder()
					.AppendLineFeed($"Server mute: `{user.IsMuted}`")
					.AppendLineFeed($"Server deafen: `{user.IsDeafened}`")
					.AppendLineFeed($"Self mute: `{user.IsSelfMuted}`")
					.AppendLineFeed($"Self deafen: `{user.IsSelfDeafened}`");
				embed.MyAddField("Voice Channel: " + user.VoiceChannel.Name, value.ToString());
			}
			return embed;
		}
		/// <summary>
		/// Returns a new <see cref="EmbedBuilder"/> containing information about a user not on a guild.
		/// </summary>
		/// <param name="guildSettings"></param>
		/// <param name="guild"></param>
		/// <param name="user"></param>
		/// <returns></returns>
		public static EmbedBuilder FormatUserInfo(IGuildSettings guildSettings, SocketGuild guild, SocketUser user)
		{
			var desc = new StringBuilder()
				.AppendLineFeed(FormatDateTimeForCreatedAtMessage(user.CreatedAt.UtcDateTime))
				.AppendLineFeed(FormatGame(user))
				.AppendLineFeed($"**Online status:** `{user.Status}`");

			return EmbedActions.MakeNewEmbed(null, desc.ToString(), null, thumbnailUrl: user.GetAvatarUrl())
				.MyAddAuthor(user)
				.MyAddFooter("User Info");
		}
		/// <summary>
		/// Returns a new <see cref="EmbedBuilder"/> containing information about a role.
		/// </summary>
		/// <param name="guildSettings"></param>
		/// <param name="guild"></param>
		/// <param name="role"></param>
		/// <returns></returns>
		public static EmbedBuilder FormatRoleInfo(IGuildSettings guildSettings, SocketGuild guild, SocketRole role)
		{
			var desc = new StringBuilder()
				.AppendLineFeed(FormatDateTimeForCreatedAtMessage(role.CreatedAt.UtcDateTime))
				.AppendLineFeed($"**Position:** `{role.Position}`")
				.AppendLineFeed($"**User Count:** `{guild.Users.Where(x => x.Roles.Any(y => y.Id == role.Id)).Count()}`");

			return EmbedActions.MakeNewEmbed(null, desc.ToString(), role.Color)
				.MyAddAuthor(role.FormatRole())
				.MyAddFooter("Role Info");
		}
		/// <summary>
		/// Returns a new <see cref="EmbedBuilder"/> containing information about a channel.
		/// </summary>
		/// <param name="guildSettings"></param>
		/// <param name="guild"></param>
		/// <param name="channel"></param>
		/// <returns></returns>
		public static EmbedBuilder FormatChannelInfo(IGuildSettings guildSettings, SocketGuild guild, SocketChannel channel)
		{
			var ignoredFromLog	= guildSettings.IgnoredLogChannels.Contains(channel.Id);
			var ignoredFromCmd	= guildSettings.IgnoredCommandChannels.Contains(channel.Id);
			var imageOnly		= guildSettings.ImageOnlyChannels.Contains(channel.Id);
			var serverLog		= guildSettings.ServerLog?.Id == channel.Id;
			var modLog			= guildSettings.ModLog?.Id == channel.Id;
			var imageLog		= guildSettings.ImageLog?.Id == channel.Id;

			var desc = new StringBuilder()
				.AppendLineFeed(FormatDateTimeForCreatedAtMessage(channel.CreatedAt.UtcDateTime))
				.AppendLineFeed($"**User Count:** `{channel.Users.Count}`\n")
				.AppendLineFeed($"\n**Ignored From Log:** `{(ignoredFromLog ? "Yes" : "No")}`")
				.AppendLineFeed($"**Ignored From Commands:** `{(ignoredFromCmd ? "Yes" : "No")}`")
				.AppendLineFeed($"**Image Only:** `{(imageOnly ? "Yes" : "No")}`")
				.AppendLineFeed($"\n**Serverlog:** `{(serverLog ? "Yes" : "No")}`")
				.AppendLineFeed($"**Modlog:** `{(modLog ? "Yes" : "No")}`")
				.AppendLineFeed($"**Imagelog:** `{(imageLog ? "Yes" : "No")}`");

			return EmbedActions.MakeNewEmbed(null, desc.ToString())
				.MyAddAuthor(channel.FormatChannel())
				.MyAddFooter("Channel Info");
		}
		/// <summary>
		/// Returns a new <see cref="EmbedBuilder"/> containing information about a guild.
		/// </summary>
		/// <param name="guildSettings"></param>
		/// <param name="guild"></param>
		/// <returns></returns>
		public static EmbedBuilder FormatGuildInfo(IGuildSettings guildSettings, SocketGuild guild)
		{
			var owner			= guild.Owner;
			var onlineCount		= guild.Users.Where(x => x.Status != UserStatus.Offline).Count();
			var nicknameCount	= guild.Users.Where(x => x.Nickname != null).Count();
			var gameCount		= guild.Users.Where(x => x.Game.HasValue).Count();
			var botCount		= guild.Users.Where(x => x.IsBot).Count();
			var voiceCount		= guild.Users.Where(x => x.VoiceChannel != null).Count();
			var localECount		= guild.Emotes.Where(x => !x.IsManaged).Count();
			var globalECount	= guild.Emotes.Where(x => x.IsManaged).Count();

			var desc = new StringBuilder()
				.AppendLineFeed(FormatDateTimeForCreatedAtMessage(guild.CreatedAt.UtcDateTime))
				.AppendLineFeed($"**Owner:** `{owner.FormatUser()}`")
				.AppendLineFeed($"**Region:** `{guild.VoiceRegionId}`")
				.AppendLineFeed($"**Emotes:** `{localECount + globalECount}` (`{localECount}` local, `{globalECount}` global)\n")
				.AppendLineFeed($"**User Count:** `{guild.MemberCount}` (`{onlineCount}` online, `{botCount}` bots)")
				.AppendLineFeed($"**Users With Nickname:** `{nicknameCount}`")
				.AppendLineFeed($"**Users Playing Games:** `{gameCount}`")
				.AppendLineFeed($"**Users In Voice:** `{voiceCount}`\n")
				.AppendLineFeed($"**Role Count:** `{guild.Roles.Count}`")
				.AppendLineFeed($"**Channel Count:** `{guild.Channels.Count}` (`{guild.TextChannels.Count}` text, `{guild.VoiceChannels.Count}` voice)")
				.AppendLineFeed($"**AFK Channel:** `{guild.AFKChannel.FormatChannel()}` (`{guild.AFKTimeout / 60}` minute{GetActions.GetPlural(guild.AFKTimeout / 60)})");

			var color = owner.Roles.FirstOrDefault(x => x.Color.RawValue != 0)?.Color;
			return EmbedActions.MakeNewEmbed(null, desc.ToString(), color, thumbnailUrl: guild.IconUrl)
				.MyAddAuthor(guild.FormatGuild())
				.MyAddFooter("Guild Info");
		}
		/// <summary>
		/// Returns a new <see cref="EmbedBuilder"/> containing information about an emote.
		/// </summary>
		/// <param name="guildSettings"></param>
		/// <param name="emote"></param>
		/// <returns></returns>
		public static EmbedBuilder FormatEmoteInfo(IGuildSettings guildSettings, Emote emote)
		{
			var desc = new StringBuilder()
				.AppendLineFeed($"**ID:** `{emote.Id}`");

			return EmbedActions.MakeNewEmbed(null, desc.ToString(), thumbnailUrl: emote.Url)
				.MyAddAuthor(emote.Name)
				.MyAddFooter("Emoji Info");
		}
		/// <summary>
		/// Returns a new <see cref="EmbedBuilder"/> containing information about an invite.
		/// </summary>
		/// <param name="guildSettings"></param>
		/// <param name="guild"></param>
		/// <param name="invite"></param>
		/// <returns></returns>
		public static EmbedBuilder FormatInviteInfo(IGuildSettings guildSettings, IInviteMetadata invite)
		{
			var desc = new StringBuilder()
				.AppendLineFeed($"**Inviter:** `{invite.Inviter.FormatUser()}`")
				.AppendLineFeed($"**Channel:** `{invite.Channel.FormatChannel()}`")
				.AppendLineFeed($"**Uses:** `{invite.Uses}`")
				.AppendLineFeed(FormatDateTimeForCreatedAtMessage(invite.CreatedAt.UtcDateTime));

			return EmbedActions.MakeNewEmbed(null, desc.ToString())
				.MyAddAuthor(invite.Code)
				.MyAddFooter("Emote Info");
		}
		/// <summary>
		/// Returns a new <see cref="EmbedBuilder"/> containing information about the bot.
		/// </summary>
		/// <param name="globalInfo"></param>
		/// <param name="client"></param>
		/// <param name="logModule"></param>
		/// <param name="guild"></param>
		/// <returns></returns>
		public static EmbedBuilder FormatBotInfo(IBotSettings globalInfo, IDiscordClient client, ILogModule logModule, IGuild guild)
		{
			var desc = new StringBuilder()
				.AppendLineFeed($"**Online Since:** `{FormatDateTime(Process.GetCurrentProcess().StartTime)}`")
				.AppendLineFeed($"**Uptime:** `{FormatUptime()}`")
				.AppendLineFeed($"**Guild Count:** `{logModule.TotalGuilds}`")
				.AppendLineFeed($"**Cumulative Member Count:** `{logModule.TotalUsers}`")
				.AppendLineFeed($"**Current Shard:** `{ClientActions.GetShardIdFor(client, guild)}`");

			var firstField = new StringBuilder()
				.AppendLineFeed(logModule.FormatLoggedActions());

			var secondField = new StringBuilder()
				.AppendLineFeed($"**Attempted:** `{logModule.AttemptedCommands}`")
				.AppendLineFeed($"**Successful:** `{logModule.SuccessfulCommands}`")
				.AppendLineFeed($"**Failed:** `{logModule.FailedCommands}`");

			var thirdField = new StringBuilder()
				.AppendLineFeed($"**Latency:** `{ClientActions.GetLatency(client)}ms`")
				.AppendLineFeed($"**Memory Usage:** `{GetActions.GetMemory().ToString("0.00")}MB`")
				.AppendLineFeed($"**Thread Count:** `{Process.GetCurrentProcess().Threads.Count}`");

			return EmbedActions.MakeNewEmbed(null, desc.ToString())
				.MyAddAuthor(client.CurrentUser)
				.MyAddField("Logged Actions", firstField.ToString())
				.MyAddField("Commands", secondField.ToString())
				.MyAddField("Technical", thirdField.ToString())
				.MyAddFooter("Version " + Constants.BOT_VERSION);
		}

		/// <summary>
		/// Returns a formatted string displaying the bot's current uptime.
		/// </summary>
		/// <param name="botSettings"></param>
		/// <returns></returns>
		public static string FormatUptime()
		{
			var span = DateTime.UtcNow.Subtract(Process.GetCurrentProcess().StartTime.ToUniversalTime());
			return $"{span.Days}:{span.Hours:00}:{span.Minutes:00}:{span.Seconds:00}";
		}
		public static string FormatDateTime(DateTime dt)
		{
			var ndt = dt.ToUniversalTime();
			var monthName = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(ndt.Month);
			return $"{monthName} {ndt.Day}, {ndt.Year} at {ndt.ToLongTimeString()}";
		}
		/// <summary>
		/// Returns the current time in a year, month, day, hour, minute, second format. E.G: 20170815_053645
		/// </summary>
		/// <returns></returns>
		public static string FormatDateTimeForSaving()
		{
			return DateTime.UtcNow.ToString("yyyyMMdd_hhmmss");
		}
		public static string FormatDateTimeForCreatedAtMessage(DateTime? dt)
		{
			return $"**Created:** `{FormatDateTime(dt ?? DateTime.UtcNow)}` (`{DateTime.UtcNow.Subtract(dt ?? DateTime.UtcNow).TotalDays}` days ago)";
		}

		public static string ERROR(string message)
		{
			return Constants.ZERO_LENGTH_CHAR + Constants.ERROR_MESSAGE + message;
		}
		public static string FormatErrorString(IGuild guild, FailureReason failureReason, object obj)
		{
			var objType = FormatObjectType(obj);
			switch (failureReason)
			{
				case FailureReason.TooFew:
				{
					return $"Unable to find the {objType}.";
				}
				case FailureReason.UserInability:
				{
					return $"You are unable to make the given changes to the {objType}: `{FormatObject(obj)}`.";
				}
				case FailureReason.BotInability:
				{
					return $"I am unable to make the given changes to the {objType}: `{FormatObject(obj)}`.";
				}
				case FailureReason.TooMany:
				{
					return $"There are too many {objType}s with the same name.";
				}
				case FailureReason.EveryoneRole:
				{
					return "The everyone role cannot be modified in that way.";
				}
				case FailureReason.ManagedRole:
				{
					return "Managed roles cannot be modified in that way.";
				}
				case FailureReason.InvalidEnum:
				{
					return $"The option `{(obj as Enum).EnumName()}` is not accepted in this instance.";
				}
				default:
				{
					return "This shouldn't be seen. - Advobot";
				}
			}
		}
		public static string FormatObjectType(object obj)
		{
			if (obj is IUser)
			{
				return Constants.BASIC_TYPE_USER;
			}
			else if (obj is IChannel)
			{
				return Constants.BASIC_TYPE_CHANNEL;
			}
			else if (obj is IRole)
			{
				return Constants.BASIC_TYPE_ROLE;
			}
			else if (obj is IGuild)
			{
				return Constants.BASIC_TYPE_GUILD;
			}
			else
			{
				return "Error fetching type";
			}
		}
		public static string FormatObject(object obj)
		{
			if (obj is IUser)
			{
				return (obj as IUser).FormatUser();
			}
			else if (obj is IChannel)
			{
				return (obj as IChannel).FormatChannel();
			}
			else if (obj is IRole)
			{
				return (obj as IRole).FormatRole();
			}
			else if (obj is IGuild)
			{
				return (obj as IGuild).FormatGuild();
			}
			else
			{
				return "Error formatting object";
			}
		}

		public static string FormatStringsWithLength(object obj1, object obj2, int len)
		{
			var str1 = obj1.ToString();
			var str2 = obj2.ToString();
			return $"{str1.PadRight(len - str2.Length)}{str2}";
		}
		public static string FormatStringsWithLength(object obj1, object obj2, int right, int left)
		{
			var str1 = obj1.ToString().PadRight(right);
			var str2 = obj2.ToString().PadLeft(left);
			return $"{str1}{str2}";
		}

		public static async Task<string> FormatAllBotSettings(IDiscordClient client, IBotSettings botSettings)
		{
			var sb = new StringBuilder();
			foreach (var property in botSettings.GetType().GetProperties())
			{
				//Only get public editable properties
				if (property.GetGetMethod() == null || property.GetSetMethod() == null)
				{
					continue;
				}

				var formatted = await FormatBotSetting(client, property.GetValue(botSettings));
				if (String.IsNullOrWhiteSpace(formatted))
				{
					continue;
				}

				sb.AppendLineFeed($"**{property.Name}**:");
				sb.AppendLineFeed($"{formatted}");
				sb.AppendLineFeed("");
			}
			return sb.ToString();
		}
		public static async Task<string> FormatBotSetting(IDiscordClient client, object value)
		{
			if (value == null)
			{
				return "`Nothing`";
			}
			else if (value is ulong tempUlong)
			{
				var user = await client.GetUserAsync(tempUlong);
				if (user != null)
				{
					return $"`{user.FormatUser()}`";
				}

				var guild = await client.GetGuildAsync(tempUlong);
				if (guild != null)
				{
					return $"`{guild.FormatGuild()}`";
				}

				return tempUlong.ToString();
			}
			//Because strings are char[] this pointless else if has to be here so it doesn't go into the else if directly below
			else if (value is string tempStr)
			{
				return String.IsNullOrWhiteSpace(tempStr) ? "`Nothing`" : $"`{tempStr}`";
			}
			else if (value is IEnumerable tempIEnumerable)
			{
				return String.Join("\n", await Task.WhenAll(tempIEnumerable.Cast<object>().Select(async x => await FormatBotSetting(client, x))));
			}
			else
			{
				return $"`{value.ToString()}`";
			}
		}
		public static string FormatAllGuildSettings(IGuild guild, IGuildSettings guildSettings)
		{
			var sb = new StringBuilder();
			foreach (var property in guildSettings.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
			{
				//Only get public editable properties
				if (property.GetGetMethod() == null || property.GetSetMethod() == null)
				{
					continue;
				}

				var formatted = FormatGuildSetting(guild as SocketGuild, property.GetValue(guildSettings));
				if (String.IsNullOrWhiteSpace(formatted))
				{
					continue;
				}

				sb.AppendLineFeed($"**{property.Name}**:");
				sb.AppendLineFeed($"{formatted}");
				sb.AppendLineFeed("");
			}
			return sb.ToString();
		}
		public static string FormatGuildSetting(SocketGuild guild, object value)
		{
			if (value == null)
			{
				return "`Nothing`";
			}
			else if (value is ISetting tempISetting)
			{
				return tempISetting.ToString();
			}
			else if (value is ulong tempUlong)
			{
				var chan = guild.GetChannel(tempUlong);
				if (chan != null)
				{
					return $"`{chan.FormatChannel()}`";
				}

				var role = guild.GetRole(tempUlong);
				if (role != null)
				{
					return $"`{role.FormatRole()}`";
				}

				var user = guild.GetUser(tempUlong);
				if (user != null)
				{
					return $"`{user.FormatUser()}`";
				}

				return tempUlong.ToString();
			}
			//Because strings are char[] this has to be here so it doesn't go into IEnumerable
			else if (value is string tempStr)
			{
				return String.IsNullOrWhiteSpace(tempStr) ? "`Nothing`" : $"`{tempStr}`";
			}
			//Has to be above IEnumerable too
			else if (value is IDictionary tempIDictionary)
			{
				return String.Join("\n", tempIDictionary.Keys.Cast<object>().Where(x => tempIDictionary[x] != null).Select(x =>
				{
					return $"{FormatGuildSetting(guild, x)}: {FormatGuildSetting(guild, tempIDictionary[x])}";
				}));
			}
			else if (value is IEnumerable tempIEnumerable)
			{
				return String.Join("\n", tempIEnumerable.Cast<object>().Select(x => FormatGuildSetting(guild, x)));
			}
			else
			{
				return $"`{value.ToString()}`";
			}
		}

		public static string FormatUserReason(IUser user, string reason = null)
		{
			var reasonStr = reason == null ? "" : $" Reason: {reason}.";
			return $"Action by {user.FormatUser()}.{reasonStr}";
		}
		public static string FormatBotReason(string reason)
		{
			if (!String.IsNullOrWhiteSpace(reason))
			{
				reason = $"Automated action. User triggered {reason.TrimEnd('.')}.";
				reason = reason.Substring(0, Math.Min(reason.Length, Constants.MAX_LENGTH_FOR_REASON));
			}
			else
			{
				reason = "Automated action. User triggered something.";
			}

			return reason;
		}

		public static string JoinNonNullStrings(string joining, params string[] toJoin)
		{
			return String.Join(joining, toJoin.Where(x => !String.IsNullOrWhiteSpace(x)));
		}
		public static string FormatNumberedList<T>(this IEnumerable<T> list, string format, params Func<T, object>[] args)
		{
			var count = 0;
			var maxLen = list.Count().ToString().Length;
			//.ToArray() must be used or else String.Format tries to use an overload accepting object as a parameter instead of object[] thus causing an exception
			return String.Join("\n", list.Select(x => $"`{(++count).ToString().PadLeft(maxLen, '0')}.` " + String.Format(@format, args.Select(y => y(x)).ToArray())));
		}

		public static string FormatGame(IUser user)
		{
			var game = user.Game.Value;
			switch (game.StreamType)
			{
				case StreamType.NotStreaming:
				{
					return $"**Current Game:** `{game.Name.EscapeBackTicks()}`";
				}
				case StreamType.Twitch:
				{
					return $"**Current Stream:** [{game.Name.EscapeBackTicks()}]({game.StreamUrl})";
				}
				default:
				{
					return "**Current Game:** `N/A`";
				}
			}
		}
		public static string FormatUserStayLength(IGuildUser user)
		{
			if (user.JoinedAt.HasValue)
			{
				var timeStayed = (DateTime.UtcNow - user.JoinedAt.Value.ToUniversalTime());
				return $"\n**Stayed for:** {timeStayed.Days}:{timeStayed.Hours:00}:{timeStayed.Minutes:00}:{timeStayed.Seconds:00}";
			}
			return "";
		}
		public static async Task<string> FormatUserInviteJoin(IGuildSettings guildSettings, IGuild guild)
		{
			var curInv = await InviteActions.GetInviteUserJoinedOn(guildSettings, guild);
			return curInv != null ? $"\n**Invite:** {curInv.Code}" : "";
		}
		public static string FormatUserAccountAgeWarning(IUser user)
		{
			var userAccAge = (DateTime.UtcNow - user.CreatedAt.ToUniversalTime());
			if (userAccAge.TotalHours < 24)
			{
				return $"\n**New Account:** {(int)userAccAge.TotalHours} hours, {userAccAge.Minutes} minutes old.";
			}
			return "";
		}

		public static string FormatUser(this IUser user)
		{
			if (user != null)
			{
				var userName = user.Username.EscapeBackTicks().CaseInsReplace("discord.gg", Constants.FAKE_DISCORD_LINK);
				return $"'{userName}#{user.Discriminator}' ({user.Id})";
			}
			else
			{
				return "Irretrievable User";
			}
		}
		public static string FormatRole(this IRole role)
		{
			if (role != null)
			{
				return $"'{role.Name.EscapeBackTicks()}' ({role.Id})";
			}
			else
			{
				return "Irretrievable Role";
			}
		}
		public static string FormatChannel(this IChannel channel)
		{
			if (channel != null)
			{
				return $"'{channel.Name.EscapeBackTicks()}' ({(channel is IMessageChannel ? "text" : "voice")}) ({channel.Id})";
			}
			else
			{
				return "Irretrievable Channel";
			}
		}
		public static string FormatGuild(this IGuild guild)
		{
			if (guild != null)
			{
				return $"'{guild.Name.EscapeBackTicks()}' ({guild.Id})";
			}
			else
			{
				return "Irretrievable Guild";
			}
		}
		public static string FormatMessage(this IMessage message)
		{
			var time = message.CreatedAt.ToString("HH:mm:ss");
			var author = message.Author.FormatUser();
			var channel = message.Channel.FormatChannel();
			var text = FormatMessageContent(message).RemoveAllMarkdown().RemoveDuplicateNewLines();
			return $"`[{time}]` `{author}` **IN** `{channel}`\n```\n{text}```";
		}
		public static string FormatMessageContent(IMessage message)
		{
			var sb = new StringBuilder((String.IsNullOrEmpty(message.Content) ? "Empty message content" : message.Content) + "\n");

			if (message.Embeds.Any())
			{
				var validEmbeds = message.Embeds.Where(x => x.Description != null || x.Url != null || x.Image.HasValue);
				var formattedDescriptions = validEmbeds.Select((x, index) =>
				{
					var tempSb = new StringBuilder($"Embed {index + 1}: {x.Description ?? "No description"}");
					if (x.Url != null)
					{
						tempSb.Append($" URL: {x.Url}");
					}
					if (x.Image.HasValue)
					{
						tempSb.Append($" IURL: {x.Image.Value.Url}");
					}
					return tempSb.ToString();
				});

				sb.AppendLineFeed(String.Join("\n", formattedDescriptions));
			}
			if (message.Attachments.Any())
			{
				sb.Append(" + " + String.Join(" + ", message.Attachments.Select(x => x.Filename)));
			}

			return sb.ToString();
		}

		/// <summary>
		/// Returns the input string with `, *, and _, escaped.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public static string EscapeAllMarkdown(this string input)
		{
			return input.Replace("`", "\\`").Replace("*", "\\*").Replace("_", "\\_");
		}
		/// <summary>
		/// Returns the input string with ` escaped.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public static string EscapeBackTicks(this string input)
		{
			return input.Replace("`", "\\`");
		}
		/// <summary>
		/// Returns the input string without `, *, and _.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public static string RemoveAllMarkdown(this string input)
		{
			return input.Replace("`", "").Replace("*", "").Replace("_", "");
		}
		/// <summary>
		/// Returns the input string with no duplicate new lines.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public static string RemoveDuplicateNewLines(this string input)
		{
			return _RemoveDuplicateSpaces.Replace(input, "\n");
		}
		/// <summary>
		/// Returns the input string with no new lines.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public static string RemoveAllNewLines(this string input)
		{
			return input.Replace("\r", "").Replace("\n", "");
		}

		/// <summary>
		/// Only appends a \n after the value. On Windows <see cref="StringBuilder.AppendLine(string)"/> appends \r\n (which isn't
		/// necessarily wanted).
		/// </summary>
		/// <param name="sb"></param>
		/// <param name="text"></param>
		/// <returns></returns>
		public static StringBuilder AppendLineFeed(this StringBuilder sb, string value)
		{
			return sb.Append(value + "\n");
		}
	}
}