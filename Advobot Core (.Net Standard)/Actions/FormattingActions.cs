using Advobot.Attributes;
using Advobot.Enums;
using Advobot.Interfaces;
using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Advobot.Actions
{
	public static class FormattingActions
	{
		public static EmbedBuilder FormatUserInfo(IGuildSettings guildSettings, SocketGuild guild, SocketGuildUser user)
		{
			var guildUser = user as SocketGuildUser;

			var roles = guildUser.Roles.OrderBy(x => x.Position).Where(x => !x.IsEveryone);
			var channels = guild.TextChannels.Where(x => guildUser.GetPermissions(x).ReadMessages).OrderBy(x => x.Position).Select(x => x.Name).ToList();
			channels.AddRange(guild.VoiceChannels.Where(x => guildUser.GetPermissions(x).Connect).OrderBy(x => x.Position).Select(x => x.Name + " (Voice)"));
			var users = guild.Users.Where(x => x.JoinedAt != null).OrderBy(x => x.JoinedAt.Value.Ticks).ToList();

			var desc = String.Join("\n", new[]
			{
				$"**ID:** `{guildUser.Id}`",
				$"**Nickname:** `{(String.IsNullOrWhiteSpace(guildUser.Nickname) ? "No nickname" : guildUser.Nickname.EscapeBackTicks())}`",
				FormatDateTimeForCreatedAtMessage(guildUser.CreatedAt),
				$"**Joined:** `{FormatDateTime(guildUser.JoinedAt.Value.UtcDateTime)}` (`{users.IndexOf(guildUser) + 1}` to join the guild)\n",
				FormatGame(guildUser),
				$"**Online status:** `{guildUser.Status}`",
			});

			var color = roles.OrderBy(x => x.Position).LastOrDefault(x => x.Color.RawValue != 0)?.Color;
			var embed = EmbedActions.MakeNewEmbed(null, desc, color, thumbnailUrl: user.GetAvatarUrl());

			if (channels.Count() != 0)
			{
				EmbedActions.AddField(embed, "Channels", String.Join(", ", channels));
			}
			if (roles.Count() != 0)
			{
				EmbedActions.AddField(embed, "Roles", String.Join(", ", roles.Select(x => x.Name)));
			}
			if (user.VoiceChannel != null)
			{
				var value = $"Server mute: `{user.IsMuted}`\nServer deafen: `{user.IsDeafened}`\nSelf mute: `{user.IsSelfMuted}`\nSelf deafen: `{user.IsSelfDeafened}`";
				EmbedActions.AddField(embed, "Voice Channel: " + user.VoiceChannel.Name, value);
			}
			EmbedActions.AddAuthor(embed, guildUser);
			EmbedActions.AddFooter(embed, "User Info");
			return embed;
		}
		public static EmbedBuilder FormatUserInfo(IGuildSettings guildSettings, SocketGuild guild, SocketUser user)
		{
			var desc = String.Join("\n", new[]
			{
				FormatDateTimeForCreatedAtMessage(user.CreatedAt),
				FormatGame(user),
				$"**Online status:** `{user.Status}`",
			});

			var embed = EmbedActions.MakeNewEmbed(null, desc, null, thumbnailUrl: user.GetAvatarUrl());
			EmbedActions.AddAuthor(embed, user.FormatUser(), user.GetAvatarUrl(), user.GetAvatarUrl());
			EmbedActions.AddFooter(embed, "User Info");
			return embed;
		}
		public static EmbedBuilder FormatRoleInfo(IGuildSettings guildSettings, SocketGuild guild, SocketRole role)
		{
			var desc = String.Join("\n", new[]
			{
				FormatDateTimeForCreatedAtMessage(role.CreatedAt),
				$"**Position:** `{role.Position}`",
				$"**User Count:** `{guild.Users.Where(x => x.Roles.Any(y => y.Id == role.Id)).Count()}`",
			});

			var embed = EmbedActions.MakeNewEmbed(null, desc, role.Color);
			EmbedActions.AddAuthor(embed, role.FormatRole());
			EmbedActions.AddFooter(embed, "Role Info");
			return embed;
		}
		public static EmbedBuilder FormatChannelInfo(IGuildSettings guildSettings, SocketGuild guild, SocketChannel channel)
		{
			var ignoredFromLog = guildSettings.IgnoredLogChannels.Contains(channel.Id);
			var ignoredFromCmd = guildSettings.IgnoredCommandChannels.Contains(channel.Id);
			var imageOnly = guildSettings.ImageOnlyChannels.Contains(channel.Id);
			var serverLog = guildSettings.ServerLog?.Id == channel.Id;
			var modLog = guildSettings.ModLog?.Id == channel.Id;
			var imageLog = guildSettings.ImageLog?.Id == channel.Id;

			var desc = String.Join("\n", new[]
			{
				FormatDateTimeForCreatedAtMessage(channel.CreatedAt),
				$"**User Count:** `{channel.Users.Count}`",
				$"\n**Ignored From Log:** `{(ignoredFromLog ? "Yes" : "No")}`",
				$"**Ignored From Commands:** `{(ignoredFromCmd ? "Yes" : "No")}`",
				$"**Image Only:** `{(imageOnly ? "Yes" : "No")}`",
				$"\n**Serverlog:** `{(serverLog ? "Yes" : "No")}`",
				$"**Modlog:** `{(modLog ? "Yes" : "No")}`",
				$"**Imagelog:** `{(imageLog ? "Yes" : "No")}`",
			});

			var embed = EmbedActions.MakeNewEmbed(null, desc);
			EmbedActions.AddAuthor(embed, channel.FormatChannel());
			EmbedActions.AddFooter(embed, "Channel Info");
			return embed;
		}
		public static EmbedBuilder FormatGuildInfo(IGuildSettings guildSettings, SocketGuild guild)
		{
			var owner = guild.Owner;
			var onlineCount = guild.Users.Where(x => x.Status != UserStatus.Offline).Count();
			var nicknameCount = guild.Users.Where(x => x.Nickname != null).Count();
			var gameCount = guild.Users.Where(x => x.Game.HasValue).Count();
			var botCount = guild.Users.Where(x => x.IsBot).Count();
			var voiceCount = guild.Users.Where(x => x.VoiceChannel != null).Count();
			var localECount = guild.Emotes.Where(x => !x.IsManaged).Count();
			var globalECount = guild.Emotes.Where(x => x.IsManaged).Count();

			var desc = String.Join("\n", new[]
			{
				FormatDateTimeForCreatedAtMessage(guild.CreatedAt),
				$"**Owner:** `{owner.FormatUser()}`",
				$"**Region:** `{guild.VoiceRegionId}`",
				$"**Emotes:** `{localECount + globalECount}` (`{localECount}` local, `{globalECount}` global)\n",
				$"**User Count:** `{guild.MemberCount}` (`{onlineCount}` online, `{botCount}` bots)",
				$"**Users With Nickname:** `{nicknameCount}`",
				$"**Users Playing Games:** `{gameCount}`",
				$"**Users In Voice:** `{voiceCount}`\n",
				$"**Role Count:** `{guild.Roles.Count}`",
				$"**Channel Count:** `{guild.Channels.Count}` (`{guild.TextChannels.Count}` text, `{guild.VoiceChannels.Count}` voice)",
				$"**AFK Channel:** `{guild.AFKChannel.FormatChannel()}` (`{guild.AFKTimeout / 60}` minute{GetActions.GetPlural(guild.AFKTimeout / 60)})",
			});

			var color = owner.Roles.FirstOrDefault(x => x.Color.RawValue != 0)?.Color;
			var embed = EmbedActions.MakeNewEmbed(null, desc, color, thumbnailUrl: guild.IconUrl);
			EmbedActions.AddAuthor(embed, guild.FormatGuild());
			EmbedActions.AddFooter(embed, "Guild Info");
			return embed;
		}
		public static EmbedBuilder FormatEmoteInfo(IGuildSettings guildSettings, IEnumerable<IGuild> guilds, Emote emote)
		{
			//Try to find the emote if global
			var guildsWithEmote = guilds.Where(x => x.HasGlobalEmotes());

			var description = $"**ID:** `{emote.Id}`\n";
			if (guildsWithEmote.Any())
			{
				description += $"**From:** `{String.Join("`, `", guildsWithEmote.Select(x => x.FormatGuild()))}`";
			}

			var embed = EmbedActions.MakeNewEmbed(null, description, thumbnailUrl: emote.Url);
			EmbedActions.AddAuthor(embed, emote.Name);
			EmbedActions.AddFooter(embed, "Emoji Info");
			return embed;
		}
		public static EmbedBuilder FormatInviteInfo(IGuildSettings guildSettings, SocketGuild guild, IInviteMetadata invite)
		{
			var desc = String.Join("\n", new[]
			{
				$"**Inviter:** `{invite.Inviter.FormatUser()}`",
				$"**Channel:** `{guild.Channels.FirstOrDefault(x => x.Id == invite.ChannelId).FormatChannel()}`",
				$"**Uses:** `{invite.Uses}`",
				FormatDateTimeForCreatedAtMessage(invite.CreatedAt),
			});

			var embed = EmbedActions.MakeNewEmbed(null, desc);
			EmbedActions.AddAuthor(embed, invite.Code);
			EmbedActions.AddFooter(embed, "Emote Info");
			return embed;
		}
		public static EmbedBuilder FormatBotInfo(IBotSettings globalInfo, IDiscordClient client, ILogModule logModule, IGuild guild)
		{
			var desc = String.Join("\n", new[]
			{
				$"**Online Since:** `{FormatDateTime(Process.GetCurrentProcess().StartTime)}`",
				$"**Uptime:** `{FormatUptime()}`",
				$"**Guild Count:** `{logModule.TotalGuilds}`",
				$"**Cumulative Member Count:** `{logModule.TotalUsers}`",
				$"**Current Shard:** `{ClientActions.GetShardIdFor(client, guild)}`",
			});

			var embed = EmbedActions.MakeNewEmbed(null, desc);
			EmbedActions.AddAuthor(embed, client.CurrentUser);
			EmbedActions.AddFooter(embed, "Version " + Constants.BOT_VERSION);

			var firstField = logModule.FormatLoggedActions();
			EmbedActions.AddField(embed, "Logged Actions", firstField);

			var secondField = String.Join("\n", new[]
			{
				$"**Attempted:** `{logModule.AttemptedCommands}`",
				$"**Successful:** `{logModule.SuccessfulCommands}`",
				$"**Failed:** `{logModule.FailedCommands}`",
			});
			EmbedActions.AddField(embed, "Commands", secondField);

			var thirdField = String.Join("\n", new[]
			{
				$"**Latency:** `{ClientActions.GetLatency(client)}ms`",
				$"**Memory Usage:** `{GetActions.GetMemory().ToString("0.00")}MB`",
				$"**Thread Count:** `{Process.GetCurrentProcess().Threads.Count}`",
			});
			EmbedActions.AddField(embed, "Technical", thirdField);

			return embed;
		}

		public static List<string> FormatMessages(IEnumerable<IMessage> list)
		{
			return list.Select(x => FormatMessage(x)).ToList();
		}
		public static string FormatMessage(IMessage message)
		{
			var time = message.CreatedAt.ToString("HH:mm:ss");
			var author = message.Author.FormatUser();
			var channel = message.Channel.FormatChannel();
			var text = FormatMessageContent(message).RemoveAllMarkdown().RemoveDuplicateNewLines();
			return $"`[{time}]` `{author}` **IN** `{channel}`\n```\n{text}```";
		}
		public static string FormatMessageContent(IMessage message)
		{
			var content = String.IsNullOrEmpty(message.Content) ? "Empty message content" : message.Content;
			if (message.Embeds.Any())
			{
				var descriptions = message.Embeds.Where(x => x.Description != null || x.Url != null || x.Image.HasValue).Select(x =>
				{
					if (x.Url != null)
					{
						return $"{x.Description} URL: {x.Url}";
					}
					if (x.Image.HasValue)
					{
						return $"{x.Description} IURL: {x.Image.Value.Url}";
					}
					else
					{
						return x.Description;
					}
				}).ToArray();

				var formattedDescriptions = "";
				for (int i = 0; i < descriptions.Length; ++i)
				{
					formattedDescriptions += $"Embed {i + 1}: {descriptions[i]}";
				}

				content += "\n" + formattedDescriptions;
			}
			if (message.Attachments.Any())
			{
				content += " + " + String.Join(" + ", message.Attachments.Select(x => x.Filename));
			}

			return content;
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
		public static string FormatDateTime(DateTime? dt)
		{
			if (!dt.HasValue)
			{
				return "N/A";
			}

			var ndt = dt.Value.ToUniversalTime();
			var monthName = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(ndt.Month);
			return $"{monthName} {ndt.Day}, {ndt.Year} at {ndt.ToLongTimeString()}";
		}
		public static string FormatDateTime(DateTimeOffset? dt)
		{
			return FormatDateTime(dt?.UtcDateTime);
		}
		/// <summary>
		/// Returns the current time in a year, month, day, hour, minute, second format. E.G: 20170815_053645
		/// </summary>
		/// <returns></returns>
		public static string FormatDateTimeForSaving()
		{
			return DateTime.UtcNow.ToString("yyyyMMdd_hhmmss");
		}
		public static string FormatDateTimeForCreatedAtMessage(DateTimeOffset? dt)
		{
			return $"**Created:** `{FormatDateTime(dt)}` (`{DateTime.UtcNow.Subtract(dt.HasValue ? dt.Value.UtcDateTime : DateTime.UtcNow).Days}` days ago)";
		}

		public static string FormatUserStayLength(IGuildUser user)
		{
			var timeStayedStr = "";
			if (user.JoinedAt.HasValue)
			{
				var timeStayed = (DateTime.UtcNow - user.JoinedAt.Value.ToUniversalTime());
				timeStayedStr = $"\n**Stayed for:** {timeStayed.Days}:{timeStayed.Hours:00}:{timeStayed.Minutes:00}:{timeStayed.Seconds:00}";
			}
			return timeStayedStr;
		}
		public static async Task<string> FormatUserInviteJoin(IGuildSettings guildSettings, IGuild guild)
		{
			var curInv = await InviteActions.GetInviteUserJoinedOn(guildSettings, guild);
			var inviteStr = "";
			if (curInv != null)
			{
				inviteStr = $"\n**Invite:** {curInv.Code}";
			}
			return inviteStr;
		}
		public static string FormatUserAccountAgeWarning(IUser user)
		{
			var userAccAge = (DateTime.UtcNow - user.CreatedAt.ToUniversalTime());
			var ageWarningStr = "";
			if (userAccAge.TotalHours < 24)
			{
				ageWarningStr = $"\n**New Account:** {(int)userAccAge.TotalHours} hours, {userAccAge.Minutes} minutes old.";
			}
			return ageWarningStr;
		}

		public static string FormatGame(IUser user)
		{
			var game = user.Game;
			switch (game?.StreamType)
			{
				case StreamType.NotStreaming:
				{
					return $"**Current Game:** `{game?.Name.EscapeBackTicks()}`";
				}
				case StreamType.Twitch:
				{
					return $"**Current Stream:** [{game?.Name.EscapeBackTicks()}]({game?.StreamUrl})";
				}
				default:
				{
					return "**Current Game:** `N/A`";
				}
			}
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

		public static string FormatAttribute(PermissionRequirementAttribute attr)
		{
			return attr != null ? $"[{JoinNonNullStrings(" | ", attr.AllText, attr.AnyText)}]" : "N/A";
		}
		public static string FormatAttribute(OtherRequirementAttribute attr)
		{
			var basePerm = "N/A";
			if (attr != null)
			{
				var text = new List<string>();
				if ((attr.Requirements & Precondition.UserHasAPerm) != 0)
				{
					text.Add("Administrator | Any perm ending with 'Members' | Any perm starting with 'Manage'");
				}
				if ((attr.Requirements & Precondition.GuildOwner) != 0)
				{
					text.Add("Guild Owner");
				}
				if ((attr.Requirements & Precondition.TrustedUser) != 0)
				{
					text.Add("Trusted User");
				}
				if ((attr.Requirements & Precondition.BotOwner) != 0)
				{
					text.Add("Bot Owner");
				}
				basePerm = $"[{String.Join(" | ", text)}]";
			}
			return basePerm;
		}

		public static async Task<string> FormatAllBotSettings(IDiscordClient client, IBotSettings botSettings)
		{
			var str = "";
			foreach (var property in botSettings.GetType().GetProperties())
			{
				//Only get public editable properties
				if (property.GetGetMethod() != null && property.GetSetMethod() != null)
				{
					var formatted = await FormatBotSettingInfo(client, botSettings, property);
					if (!String.IsNullOrWhiteSpace(formatted))
					{
						str += $"**{property.Name}**:\n{formatted}\n\n";
					}
				}
			}
			return str;
		}
		public static async Task<string> FormatBotSettingInfo(IDiscordClient client, IBotSettings botSettings, PropertyInfo property)
		{
			var value = property.GetValue(botSettings);
			return value != null ? await FormatBotSettingInfo(client, value) : null;
		}
		public static async Task<string> FormatBotSettingInfo(IDiscordClient client, object value)
		{
			if (value is ulong)
			{
				var user = await UserActions.GetGlobalUser(client, (ulong)value);
				if (user != null)
				{
					return $"`{user.FormatUser()}`";
				}

				var guild = await client.GetGuildAsync((ulong)value);
				if (guild != null)
				{
					return $"`{guild.FormatGuild()}`";
				}

				return ((ulong)value).ToString();
			}
			//Because strings are char[] this pointless else if has to be here so it doesn't go into the else if directly below
			else if (value is string)
			{
				return String.IsNullOrWhiteSpace(value.ToString()) ? "`Nothing`" : $"`{value.ToString()}`";
			}
			else if (value is System.Collections.IEnumerable)
			{
				var temp = new List<string>();
				foreach (var tempSetting in ((System.Collections.IEnumerable)value).Cast<object>())
				{
					temp.Add(await FormatBotSettingInfo(client, tempSetting));
				}
				return String.Join("\n", temp);
			}
			else
			{
				return $"`{value.ToString()}`";
			}
		}

		public static string FormatAllGuildSettings(IGuild guild, IGuildSettings guildSettings)
		{
			var str = "";
			foreach (var property in guildSettings.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
			{
				//Only get public editable properties
				if (property.GetGetMethod() != null && property.GetSetMethod() != null)
				{
					var formatted = FormatGuildSettingInfo(guild as SocketGuild, guildSettings, property);
					if (!String.IsNullOrWhiteSpace(formatted))
					{
						str += $"**{property.Name}**:\n{formatted}\n\n";
					}
				}
			}
			return str;
		}
		public static string FormatGuildSettingInfo(SocketGuild guild, IGuildSettings guildSettings, PropertyInfo property)
		{
			var value = property.GetValue(guildSettings);
			if (value != null)
			{
				return FormatGuildSettingInfo(guild, value);
			}
			else
			{
				return null;
			}
		}
		public static string FormatGuildSettingInfo(SocketGuild guild, object value)
		{
			if (value is ISetting)
			{
				return ((ISetting)value).ToString();
			}
			else if (value is ulong)
			{
				var chan = guild.GetChannel((ulong)value);
				if (chan != null)
				{
					return $"`{chan.FormatChannel()}`";
				}

				var role = guild.GetRole((ulong)value);
				if (role != null)
				{
					return $"`{role.FormatRole()}`";
				}

				var user = guild.GetUser((ulong)value);
				if (user != null)
				{
					return $"`{user.FormatUser()}`";
				}

				return ((ulong)value).ToString();
			}
			//Because strings are char[] this has to be here so it doesn't go into IEnumerable
			else if (value is string)
			{
				return String.IsNullOrWhiteSpace(value.ToString()) ? "`Nothing`" : $"`{value.ToString()}`";
			}
			//Has to be above IEnumerable too
			else if (value is System.Collections.IDictionary)
			{
				var dict = value as System.Collections.IDictionary;
				//I can't tell if I'm retarded or working with the dictionary interface is just annoying as fuck
				var settings = dict.Keys.Cast<object>().Where(x => dict[x] != null).Select(x =>
				{
					return $"{FormatGuildSettingInfo(guild, x)}: {FormatGuildSettingInfo(guild, dict[x])}";
				});
				return String.Join("\n", settings);
			}
			else if (value is System.Collections.IEnumerable)
			{
				return String.Join("\n", ((System.Collections.IEnumerable)value).Cast<object>().Select(x => FormatGuildSettingInfo(guild, x)));
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
		public static string FormatUser(this IUser user, ulong? userId = 0)
		{
			if (user != null)
			{
				var userName = user.Username.EscapeBackTicks().CaseInsReplace("discord.gg", Constants.FAKE_DISCORD_LINK);
				return $"'{userName}#{user.Discriminator}' ({user.Id})";
			}
			else
			{
				return $"Irretrievable User ({userId})";
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
		public static string FormatGuild(this IGuild guild, ulong? guildId = 0)
		{
			if (guild != null)
			{
				return $"'{guild.Name.EscapeBackTicks()}' ({guild.Id})";
			}
			else
			{
				return $"Irretrievable Guild ({guildId})";
			}
		}
	}
}