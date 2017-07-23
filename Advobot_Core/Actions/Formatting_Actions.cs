using Advobot.Attributes;
using Advobot.Enums;
using Advobot.Interfaces;
using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Advobot
{
	namespace Actions
	{
		public static class FormattingActions
		{
			public static EmbedBuilder FormatUserInfo(IGuildSettings guildSettings, SocketGuild guild, SocketGuildUser user)
			{
				var guildUser = user as SocketGuildUser;
				var roles = guildUser.Roles.OrderBy(x => x.Position).Where(x => !x.IsEveryone);
				var channels = new List<string>();
				guild.TextChannels.OrderBy(x => x.Position).ToList().ForEach(x =>
				{
					if (guildUser.GetPermissions(x).ReadMessages)
					{
						channels.Add(x.Name);
					}
				});
				guild.VoiceChannels.OrderBy(x => x.Position).ToList().ForEach(x =>
				{
					if (guildUser.GetPermissions(x).Connect)
					{
						channels.Add(x.Name + " (Voice)");
					}
				});
				var users = guild.Users.Where(x => x.JoinedAt != null).OrderBy(x => x.JoinedAt.Value.Ticks).ToList();
				var created = guildUser.CreatedAt.UtcDateTime;
				var joined = guildUser.JoinedAt.Value.UtcDateTime;

				var IDstr = String.Format("**ID:** `{0}`", guildUser.Id);
				var nicknameStr = String.Format("**Nickname:** `{0}`", String.IsNullOrWhiteSpace(guildUser.Nickname) ? "NO NICKNAME" : EscapeMarkdown(guildUser.Nickname, true));
				var createdStr = String.Format("\n**Created:** `{0}`", FormatDateTime(guildUser.CreatedAt.UtcDateTime));
				var joinedStr = String.Format("**Joined:** `{0}` (`{1}` to join the guild)\n", FormatDateTime(guildUser.JoinedAt.Value.UtcDateTime), users.IndexOf(guildUser) + 1);
				var gameStr = FormatGame(guildUser);
				var statusStr = String.Format("**Online status:** `{0}`", guildUser.Status);
				var description = String.Join("\n", new[] { IDstr, nicknameStr, createdStr, joinedStr, gameStr, statusStr });

				var color = roles.OrderBy(x => x.Position).LastOrDefault(x => x.Color.RawValue != 0)?.Color;
				var embed = EmbedActions.MakeNewEmbed(null, description, color, thumbnailURL: user.GetAvatarUrl());
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
					var desc = String.Format("Server mute: `{0}`\nServer deafen: `{1}`\nSelf mute: `{2}`\nSelf deafen: `{3}`", user.IsMuted, user.IsDeafened, user.IsSelfMuted, user.IsSelfDeafened);
					EmbedActions.AddField(embed, "Voice Channel: " + user.VoiceChannel.Name, desc);
				}
				EmbedActions.AddAuthor(embed, guildUser);
				EmbedActions.AddFooter(embed, "User Info");
				return embed;
			}
			public static EmbedBuilder FormatUserInfo(IGuildSettings guildSettings, SocketGuild guild, SocketUser user)
			{
				var ageStr = String.Format("**Created:** `{0}`\n", FormatDateTime(user.CreatedAt.UtcDateTime));
				var gameStr = FormatGame(user);
				var statusStr = String.Format("**Online status:** `{0}`", user.Status);
				var description = String.Join("\n", new[] { ageStr, gameStr, statusStr });

				var embed = EmbedActions.MakeNewEmbed(null, description, null, thumbnailURL: user.GetAvatarUrl());
				EmbedActions.AddAuthor(embed, user.FormatUser(), user.GetAvatarUrl(), user.GetAvatarUrl());
				EmbedActions.AddFooter(embed, "User Info");
				return embed;
			}
			public static EmbedBuilder FormatRoleInfo(IGuildSettings guildSettings, SocketGuild guild, SocketRole role)
			{
				var ageStr = String.Format("**Created:** `{0}` (`{1}` days ago)", FormatDateTime(role.CreatedAt.UtcDateTime), DateTime.UtcNow.Subtract(role.CreatedAt.UtcDateTime).Days);
				var positionStr = String.Format("**Position:** `{0}`", role.Position);
				var usersStr = String.Format("**User Count:** `{0}`", guild.Users.Where(x => x.Roles.Any(y => y.Id == role.Id)).Count());
				var description = String.Join("\n", new[] { ageStr, positionStr, usersStr });

				var color = role.Color;
				var embed = EmbedActions.MakeNewEmbed(null, description, color);
				EmbedActions.AddAuthor(embed, role.FormatRole());
				EmbedActions.AddFooter(embed, "Role Info");
				return embed;
			}
			public static EmbedBuilder FormatChannelInfo(IGuildSettings guildSettings, SocketGuild guild, SocketChannel channel)
			{
				var ignoredFromLog = guildSettings.IgnoredLogChannels.Contains(channel.Id);
				var ignoredFromCmd = guildSettings.IgnoredCommandChannels.Contains(channel.Id);
				var imageOnly = guildSettings.ImageOnlyChannels.Contains(channel.Id);
				var sanitary = guildSettings.SanitaryChannels.Contains(channel.Id);
				var serverLog = guildSettings.ServerLog?.Id == channel.Id;
				var modLog = guildSettings.ModLog?.Id == channel.Id;
				var imageLog = guildSettings.ImageLog?.Id == channel.Id;

				var ageStr = String.Format("**Created:** `{0}` (`{1}` days ago)", FormatDateTime(channel.CreatedAt.UtcDateTime), DateTime.UtcNow.Subtract(channel.CreatedAt.UtcDateTime).Days);
				var userCountStr = String.Format("**User Count:** `{0}`", channel.Users.Count);
				var ignoredFromLogStr = String.Format("\n**Ignored From Log:** `{0}`", ignoredFromLog ? "Yes" : "No");
				var ignoredFromCmdStr = String.Format("**Ignored From Commands:** `{0}`", ignoredFromCmd ? "Yes" : "No");
				var imageOnlyStr = String.Format("**Image Only:** `{0}`", imageOnly ? "Yes" : "No");
				var sanitaryStr = String.Format("**Sanitary:** `{0}`", sanitary ? "Yes" : "No");
				var serverLogStr = String.Format("\n**Serverlog:** `{0}`", serverLog ? "Yes" : "No");
				var modLogStr = String.Format("**Modlog:** `{0}`", modLog ? "Yes" : "No");
				var imageLogStr = String.Format("**Imagelog:** `{0}`", imageLog ? "Yes" : "No");
				var description = String.Join("\n", new[] { ageStr, userCountStr, ignoredFromLogStr, ignoredFromCmdStr, imageOnlyStr, sanitaryStr, serverLogStr, modLogStr, imageLogStr });

				var embed = EmbedActions.MakeNewEmbed(null, description);
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

				var ageStr = String.Format("**Created:** `{0}` (`{1}` days ago)", FormatDateTime(guild.CreatedAt.UtcDateTime), DateTime.UtcNow.Subtract(guild.CreatedAt.UtcDateTime).Days);
				var ownerStr = String.Format("**Owner:** `{0}`", owner.FormatUser());
				var regionStr = String.Format("**Region:** `{0}`", guild.VoiceRegionId);
				var emoteStr = String.Format("**Emotes:** `{0}` (`{1}` local, `{2}` global)\n", localECount + globalECount, localECount, globalECount);
				var userStr = String.Format("**User Count:** `{0}` (`{1}` online, `{2}` bots)", guild.MemberCount, onlineCount, botCount);
				var nickStr = String.Format("**Users With Nickname:** `{0}`", nicknameCount);
				var gameStr = String.Format("**Users Playing Games:** `{0}`", gameCount);
				var voiceStr = String.Format("**Users In Voice:** `{0}`\n", voiceCount);
				var roleStr = String.Format("**Role Count:** `{0}`", guild.Roles.Count);
				var channelStr = String.Format("**Channel Count:** `{0}` (`{1}` text, `{2}` voice)", guild.Channels.Count, guild.TextChannels.Count, guild.VoiceChannels.Count);
				var afkChanStr = String.Format("**AFK Channel:** `{0}` (`{1}` minute{2})", guild.AFKChannel.FormatChannel(), guild.AFKTimeout / 60, GetActions.GetPlural(guild.AFKTimeout / 60));
				var description = String.Join("\n", new List<string>() { ageStr, ownerStr, regionStr, emoteStr, userStr, nickStr, gameStr, voiceStr, roleStr, channelStr, afkChanStr });

				var color = owner.Roles.FirstOrDefault(x => x.Color.RawValue != 0)?.Color;
				var embed = EmbedActions.MakeNewEmbed(null, description, color, thumbnailURL: guild.IconUrl);
				EmbedActions.AddAuthor(embed, guild.FormatGuild());
				EmbedActions.AddFooter(embed, "Guild Info");
				return embed;
			}
			public static EmbedBuilder FormatEmoteInfo(IGuildSettings guildSettings, IEnumerable<IGuild> guilds, Emote emote)
			{
				//Try to find the emoji if global
				var guildsWithEmote = guilds.Where(x => x.HasGlobalEmotes());

				var description = String.Format("**ID:** `{0}`\n", emote.Id);
				if (guildsWithEmote.Any())
				{
					description += String.Format("**From:** `{0}`", String.Join("`, `", guildsWithEmote.Select(x => x.FormatGuild())));
				}

				var embed = EmbedActions.MakeNewEmbed(null, description, thumbnailURL: emote.Url);
				EmbedActions.AddAuthor(embed, emote.Name);
				EmbedActions.AddFooter(embed, "Emoji Info");
				return embed;
			}
			public static EmbedBuilder FormatInviteInfo(IGuildSettings guildSettings, SocketGuild guild, IInviteMetadata invite)
			{
				var inviterStr = String.Format("**Inviter:** `{0}`", invite.Inviter.FormatUser());
				var channelStr = String.Format("**Channel:** `{0}`", guild.Channels.FirstOrDefault(x => x.Id == invite.ChannelId).FormatChannel());
				var usesStr = String.Format("**Uses:** `{0}`", invite.Uses);
				var createdStr = String.Format("**Created At:** `{0}`", FormatDateTime(invite.CreatedAt.UtcDateTime));
				var description = String.Join("\n", new[] { inviterStr, channelStr, usesStr, createdStr });

				var embed = EmbedActions.MakeNewEmbed(null, description);
				EmbedActions.AddAuthor(embed, invite.Code);
				EmbedActions.AddFooter(embed, "Emote Info");
				return embed;
			}
			public static EmbedBuilder FormatBotInfo(IBotSettings globalInfo, IDiscordClient client, ILogModule logModule, IGuild guild)
			{
				var online = String.Format("**Online Since:** `{0}`", FormatDateTime(globalInfo.StartupTime));
				var uptime = String.Format("**Uptime:** `{0}`", GetActions.GetUptime(globalInfo));
				var guildCount = String.Format("**Guild Count:** `{0}`", logModule.TotalGuilds);
				var memberCount = String.Format("**Cumulative Member Count:** `{0}`", logModule.TotalUsers);
				var currShard = String.Format("**Current Shard:** `{0}`", ClientActions.GetShardIdFor(client, guild));
				var description = String.Join("\n", new[] { online, uptime, guildCount, memberCount, currShard });

				var embed = EmbedActions.MakeNewEmbed(null, description);
				EmbedActions.AddAuthor(embed, client.CurrentUser);
				EmbedActions.AddFooter(embed, "Version " + Constants.BOT_VERSION);

				var firstField = logModule.FormatLoggedActions();
				EmbedActions.AddField(embed, "Logged Actions", firstField);

				var secondField = logModule.FormatLoggedCommands();
				EmbedActions.AddField(embed, "Commands", secondField);

				var latency = String.Format("**Latency:** `{0}ms`", ClientActions.GetLatency(client));
				var memory = String.Format("**Memory Usage:** `{0}MB`", GetActions.GetMemory(globalInfo.Windows).ToString("0.00"));
				var threads = String.Format("**Thread Count:** `{0}`", System.Diagnostics.Process.GetCurrentProcess().Threads.Count);
				var thirdField = String.Join("\n", new[] { latency, memory, threads });
				EmbedActions.AddField(embed, "Technical", thirdField);

				return embed;
			}

			public static List<string> FormatMessages(IEnumerable<IMessage> list)
			{
				return list.Select(x => FormatNonDM(x)).ToList();
			}
			public static List<string> FormatDMs(IEnumerable<IMessage> list)
			{
				return list.Select(x => FormatDM(x)).ToList();
			}
			public static string FormatNonDM(IMessage message)
			{
				return String.Format("`[{0}]` `{1}` **IN** `{2}`\n```\n{3}```",
					message.CreatedAt.ToString("HH:mm:ss"),
					message.Author.FormatUser(),
					message.Channel.FormatChannel(),
					RemoveMarkdownChars(FormatMessageContent(message), true));
			}
			public static string FormatDM(IMessage message)
			{
				return String.Format("`[{0}]` `{1}`\n```\n{2}```",
					FormatDateTime(message.CreatedAt),
					message.Author.FormatUser(),
					RemoveMarkdownChars(FormatMessageContent(message), true));
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
							return String.Format("{0} URL: {1}", x.Description, x.Url);
						}
						if (x.Image.HasValue)
						{
							return String.Format("{0} IURL: {1}", x.Description, x.Image.Value.Url);
						}
						else
						{
							return x.Description;
						}
					}).ToArray();

					var formattedDescriptions = "";
					for (int i = 0; i < descriptions.Length; ++i)
					{
						formattedDescriptions += String.Format("Embed {0}: {1}", i + 1, descriptions[i]);
					}

					content += "\n" + formattedDescriptions;
				}
				if (message.Attachments.Any())
				{
					content += " + " + String.Join(" + ", message.Attachments.Select(x => x.Filename));
				}

				return content;
			}

			public static string FormatDateTime(DateTime? dt)
			{
				if (!dt.HasValue)
				{
					return "N/A";
				}

				var ndt = dt.Value.ToUniversalTime();
				return String.Format("{0} {1}, {2} at {3}",
					System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(ndt.Month),
					ndt.Day,
					ndt.Year,
					ndt.ToLongTimeString());
			}
			public static string FormatDateTime(DateTimeOffset? dt)
			{
				return FormatDateTime(dt?.UtcDateTime);
			}

			public static string FormatGame(IUser user)
			{
				var game = user.Game;
				switch (game?.StreamType)
				{
					case StreamType.NotStreaming:
					{
						return String.Format("**Current Game:** `{0}`", EscapeMarkdown(game?.Name, true));
					}
					case StreamType.Twitch:
					{
						return String.Format("**Current Stream:** [{0}]({1})", EscapeMarkdown(game?.Name, true), game?.StreamUrl);
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

			public static string EscapeMarkdown(string str, bool onlyAccentGrave)
			{
				return onlyAccentGrave ? str.Replace("`", "\\`") : str.Replace("`", "\\`").Replace("*", "\\*").Replace("_", "\\_");
			}
			public static string RemoveMarkdownChars(string input, bool replaceNewLines)
			{
				if (String.IsNullOrWhiteSpace(input))
					return "";

				input = new Regex("[*`]", RegexOptions.Compiled).Replace(input, "");

				while (replaceNewLines)
				{
					if (input.Contains("\n\n"))
					{
						input = input.Replace("\n\n", "\n");
					}
					else
					{
						break;
					}
				}

				return input;
			}
			public static string RemoveNewLines(string input)
			{
				return input.Replace(Environment.NewLine, "").Replace("\r", "").Replace("\n", "");
			}

			public static string FormatErrorString(IGuild guild, FailureReason failureReason, object obj)
			{
				var objType = FormatObjectType(obj);
				switch (failureReason)
				{
					case FailureReason.TooFew:
					{
						return String.Format("Unable to find the {0}.", objType);
					}
					case FailureReason.UserInability:
					{
						return String.Format("You are unable to make the given changes to the {0}: `{1}`.", objType, FormatObject(obj));
					}
					case FailureReason.BotInability:
					{
						return String.Format("I am unable to make the given changes to the {0}: `{1}`.", objType, FormatObject(obj));
					}
					case FailureReason.TooMany:
					{
						return String.Format("There are too many {0}s with the same name.", objType);
					}
					case FailureReason.ChannelType:
					{
						return "Invalid channel type for the given variable requirement.";
					}
					case FailureReason.DefaultChannel:
					{
						return "The default channel cannot be modified in that way.";
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
						return String.Format("The option `{0}` is not accepted in this instance.", (obj as Enum).EnumName());
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
				return String.Format("{0}{1}", str1.PadRight(len - str2.Length), str2);
			}
			public static string FormatStringsWithLength(object obj1, object obj2, int right, int left)
			{
				var str1 = obj1.ToString().PadRight(right);
				var str2 = obj2.ToString().PadLeft(left);
				return String.Format("{0}{1}", str1, str2);
			}

			public static string FormatAttribute(PermissionRequirementAttribute attr)
			{
				return attr != null ? String.Format("[{0}]", JoinNonNullStrings(" | ", attr.AllText, attr.AnyText)) : "N/A";
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
					basePerm = String.Format("[{0}]", String.Join(" | ", text));
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
							str += String.Format("**{0}**:\n{1}\n\n", property.Name, formatted);
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
						return String.Format("`{0}`", user.FormatUser());
					}

					var guild = await GuildActions.GetGuild(client, (ulong)value);
					if (guild != null)
					{
						return String.Format("`{0}`", guild.FormatGuild());
					}

					return ((ulong)value).ToString();
				}
				//Because strings are char[] this pointless else if has to be here so it doesn't go into the else if directly below
				else if (value is string)
				{
					return String.IsNullOrWhiteSpace(value.ToString()) ? "`Nothing`" : String.Format("`{0}`", value.ToString());
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
					return String.Format("`{0}`", value.ToString());
				}
			}

			public static string FormatAllGuildSettings(IGuild guild, IGuildSettings guildSettings)
			{
				var str = "";
				foreach (var property in guildSettings.GetType().GetProperties(BindingFlags.Public))
				{
					//Only get public editable properties
					if (property.GetGetMethod() != null && property.GetSetMethod() != null)
					{
						var formatted = FormatGuildSettingInfo(guild as SocketGuild, guildSettings, property);
						if (!String.IsNullOrWhiteSpace(formatted))
						{
							str += String.Format("**{0}**:\n{1}\n\n", property.Name, formatted);
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
					return ((ISetting)value).SettingToString();
				}
				else if (value is ulong)
				{
					var chan = guild.GetChannel((ulong)value);
					if (chan != null)
					{
						return String.Format("`{0}`", chan.FormatChannel());
					}

					var role = guild.GetRole((ulong)value);
					if (role != null)
					{
						return String.Format("`{0}`", role.FormatRole());
					}

					var user = guild.GetUser((ulong)value);
					if (user != null)
					{
						return String.Format("`{0}`", user.FormatUser());
					}

					return ((ulong)value).ToString();
				}
				//Because strings are char[] this pointless else if has to be here so it doesn't go into the else if directly below
				else if (value is string)
				{
					return String.IsNullOrWhiteSpace(value.ToString()) ? "`Nothing`" : String.Format("`{0}`", value.ToString());
				}
				else if (value is System.Collections.IEnumerable)
				{
					return String.Join("\n", ((System.Collections.IEnumerable)value).Cast<object>().Select(x => FormatGuildSettingInfo(guild, x)));
				}
				else
				{
					return String.Format("`{0}`", value.ToString());
				}
			}

			public static string FormatUserReason(IUser user)
			{
				return String.Format("Action by {0}.", user.FormatUser());
			}
			public static string FormatBotReason(string reason)
			{
				if (!String.IsNullOrWhiteSpace(reason))
				{
					reason = String.Format("Automated action. User triggered {0}.", reason.TrimEnd('.'));
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
		}
	}
}