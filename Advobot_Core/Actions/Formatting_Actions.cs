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
				var channels = guild.TextChannels.Where(x => guildUser.GetPermissions(x).ReadMessages).OrderBy(x => x.Position).Select(x => x.Name).ToList();
				channels.AddRange(guild.VoiceChannels.Where(x => guildUser.GetPermissions(x).Connect).OrderBy(x => x.Position).Select(x => x.Name + " (Voice)"));
				var users = guild.Users.Where(x => x.JoinedAt != null).OrderBy(x => x.JoinedAt.Value.Ticks).ToList();

				var desc = String.Join("\n", new[]
				{
					$"**ID:** `{guildUser.Id}`",
					$"**Nickname:** `{(String.IsNullOrWhiteSpace(guildUser.Nickname) ? "NO NICKNAME" : EscapeMarkdown(guildUser.Nickname, true))}`",
					FormatDateTimeForCreatedAtMessage(guildUser.CreatedAt),
					$"**Joined:** `{FormatDateTime(guildUser.JoinedAt.Value.UtcDateTime)}` (`{users.IndexOf(guildUser) + 1}` to join the guild)\n",
					FormatGame(guildUser),
					$"**Online status:** `{guildUser.Status}`",
				});

				var color = roles.OrderBy(x => x.Position).LastOrDefault(x => x.Color.RawValue != 0)?.Color;
				var embed = EmbedActions.MakeNewEmbed(null, desc, color, thumbnailURL: user.GetAvatarUrl());

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

				var embed = EmbedActions.MakeNewEmbed(null, desc, null, thumbnailURL: user.GetAvatarUrl());
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
				var embed = EmbedActions.MakeNewEmbed(null, desc, color, thumbnailURL: guild.IconUrl);
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

				var embed = EmbedActions.MakeNewEmbed(null, description, thumbnailURL: emote.Url);
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
					$"**Online Since:** `{FormatDateTime(globalInfo.StartupTime)}`",
					$"**Uptime:** `{GetActions.GetUptime(globalInfo)}`",
					$"**Guild Count:** `{logModule.TotalGuilds}`",
					$"**Cumulative Member Count:** `{logModule.TotalUsers}`",
					$"**Current Shard:** `{ClientActions.GetShardIdFor(client, guild)}`",
				});

				var embed = EmbedActions.MakeNewEmbed(null, desc);
				EmbedActions.AddAuthor(embed, client.CurrentUser);
				EmbedActions.AddFooter(embed, "Version " + Constants.BOT_VERSION);

				var firstField = logModule.FormatLoggedActions();
				EmbedActions.AddField(embed, "Logged Actions", firstField);

				var secondField = logModule.FormatLoggedCommands();
				EmbedActions.AddField(embed, "Commands", secondField);

				var thirdField = String.Join("\n", new[]
				{
					$"**Latency:** `{ClientActions.GetLatency(client)}ms`",
					$"**Memory Usage:** `{GetActions.GetMemory(globalInfo.Windows).ToString("0.00")}MB`",
					$"**Thread Count:** `{System.Diagnostics.Process.GetCurrentProcess().Threads.Count}`",
				});
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
				return $"`[{0}]` `{1}` **IN** `{2}`\n```\n{3}```",
					message.CreatedAt.ToString("HH:mm:ss"),
					message.Author.FormatUser(),
					message.Channel.FormatChannel(),
					RemoveMarkdownChars(FormatMessageContent(message), true));
			}
			public static string FormatDM(IMessage message)
			{
				return $"`[{0}]` `{1}`\n```\n{2}```",
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

			public static string FormatGame(IUser user)
			{
				var game = user.Game;
				switch (game?.StreamType)
				{
					case StreamType.NotStreaming:
					{
						return $"**Current Game:** `{0}`", EscapeMarkdown(game?.Name, true));
					}
					case StreamType.Twitch:
					{
						return $"**Current Stream:** [{0}]({1})", EscapeMarkdown(game?.Name, true), game?.StreamUrl);
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
						return $"Unable to find the {0}.", objType);
					}
					case FailureReason.UserInability:
					{
						return $"You are unable to make the given changes to the {0}: `{1}`.", objType, FormatObject(obj));
					}
					case FailureReason.BotInability:
					{
						return $"I am unable to make the given changes to the {0}: `{1}`.", objType, FormatObject(obj));
					}
					case FailureReason.TooMany:
					{
						return $"There are too many {0}s with the same name.", objType);
					}
					case FailureReason.ChannelType:
					{
						return "Invalid channel type for the given variable requirement.";
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
						return $"The option `{0}` is not accepted in this instance.", (obj as Enum).EnumName());
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
				return $"{0}{1}", str1.PadRight(len - str2.Length), str2);
			}
			public static string FormatStringsWithLength(object obj1, object obj2, int right, int left)
			{
				var str1 = obj1.ToString().PadRight(right);
				var str2 = obj2.ToString().PadLeft(left);
				return $"{0}{1}", str1, str2);
			}

			public static string FormatAttribute(PermissionRequirementAttribute attr)
			{
				return attr != null ? $"[{0}]", JoinNonNullStrings(" | ", attr.AllText, attr.AnyText)) : "N/A";
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
					basePerm = $"[{0}]", String.Join(" | ", text));
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
							str += $"**{0}**:\n{1}\n\n", property.Name, formatted);
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
						return $"`{0}`", user.FormatUser());
					}

					var guild = await GuildActions.GetGuild(client, (ulong)value);
					if (guild != null)
					{
						return $"`{0}`", guild.FormatGuild());
					}

					return ((ulong)value).ToString();
				}
				//Because strings are char[] this pointless else if has to be here so it doesn't go into the else if directly below
				else if (value is string)
				{
					return String.IsNullOrWhiteSpace(value.ToString()) ? "`Nothing`" : $"`{0}`", value.ToString());
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
					return $"`{0}`", value.ToString());
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
							str += $"**{0}**:\n{1}\n\n", property.Name, formatted);
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
				var reasonStr = reason == null ? "" : $"Reason: {0}.", reason);
				return $"Action by {0}.{1}", user.FormatUser(), reasonStr);
			}
			public static string FormatBotReason(string reason)
			{
				if (!String.IsNullOrWhiteSpace(reason))
				{
					reason = $"Automated action. User triggered {0}.", reason.TrimEnd('.'));
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