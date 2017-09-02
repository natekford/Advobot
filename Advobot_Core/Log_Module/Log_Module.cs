using Advobot.Actions;
using Advobot.Enums;
using Advobot.Interfaces;
using Advobot.NonSavedClasses;
using Advobot.SavedClasses;
using Advobot.Structs;
using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Advobot.Attributes;
using System.Threading.Tasks;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Advobot
{
	namespace Logging
	{
		/// <summary>
		/// This is probably the second worst part of the bot, right behind the UI. Slightly ahead of saving settings though.
		/// </summary>
		public sealed class MyLogModule : ILogModule
		{
			private IDiscordClient _Client { get; }
			private IBotSettings _BotSettings { get; }
			private IGuildSettingsModule _GuildSettings { get; }
			private ITimersModule _Timers { get; }

			public List<LoggedCommand> RanCommands { get; } = new List<LoggedCommand>();

			public uint TotalUsers { get; private set; } = 0;
			public uint TotalGuilds { get; private set; } = 0;
			public uint AttemptedCommands { get; private set; } = 0;
			public uint SuccessfulCommands { get; private set; } = 0;
			public uint FailedCommands { get; private set; } = 0;
			public uint LoggedJoins { get; private set; } = 0;
			public uint LoggedLeaves { get; private set; } = 0;
			public uint LoggedUserChanges { get; private set; } = 0;
			public uint LoggedEdits { get; private set; } = 0;
			public uint LoggedDeletes { get; private set; } = 0;
			public uint LoggedMessages { get; private set; } = 0;
			public uint LoggedImages { get; private set; } = 0;
			public uint LoggedGifs { get; private set; } = 0;
			public uint LoggedFiles { get; private set; } = 0;

			public MyLogModule(IServiceProvider provider)
			{
				_Client = (IDiscordClient)provider.GetService(typeof(IDiscordClient));
				_BotSettings = (IBotSettings)provider.GetService(typeof(IBotSettings));
				_GuildSettings = (IGuildSettingsModule)provider.GetService(typeof(IGuildSettingsModule));
				_Timers = (ITimersModule)provider.GetService(typeof(ITimersModule));

				if (_Client is DiscordSocketClient)
				{
					CreateLogHolder(_Client as DiscordSocketClient);
				}
				else if (_Client is DiscordShardedClient)
				{
					CreateLogHolder(_Client as DiscordShardedClient);
				}
				else
				{
					throw new ArgumentException("Invalid client provided. Must be either a DiscordSocketClient or a DiscordShardedClient.");
				}
			}

			private void CreateLogHolder(DiscordSocketClient client)
			{
				client.Log						+= Log;
				client.GuildAvailable			+= OnGuildAvailable;
				client.GuildUnavailable			+= OnGuildUnavailable;
				client.JoinedGuild				+= OnJoinedGuild;
				client.LeftGuild				+= OnLeftGuild;
				client.UserJoined				+= OnUserJoined;
				client.UserLeft					+= OnUserLeft;
				client.UserUpdated				+= OnUserUpdated;
				client.MessageReceived			+= OnMessageReceived;
				client.MessageUpdated			+= OnMessageUpdated;
				client.MessageDeleted			+= OnMessageDeleted;
			}
			private void CreateLogHolder(DiscordShardedClient client)
			{
				client.Log						+= Log;
				client.GuildAvailable			+= OnGuildAvailable;
				client.GuildUnavailable			+= OnGuildUnavailable;
				client.JoinedGuild				+= OnJoinedGuild;
				client.LeftGuild				+= OnLeftGuild;
				client.UserJoined				+= OnUserJoined;
				client.UserLeft					+= OnUserLeft;
				client.UserUpdated				+= OnUserUpdated;
				client.MessageReceived			+= OnMessageReceived;
				client.MessageUpdated			+= OnMessageUpdated;
				client.MessageDeleted			+= OnMessageDeleted;
			}

			public void AddUsers(int users)
			{
				TotalUsers += (uint)users;
			}
			public void RemoveUsers(int users)
			{
				TotalUsers -= (uint)users;
			}
			public void IncrementUsers()
			{
				++TotalUsers;
			}
			public void DecrementUsers()
			{
				--TotalUsers;
			}
			public void IncrementGuilds()
			{
				++TotalGuilds;
			}
			public void DecrementGuilds()
			{
				--TotalGuilds;
			}
			public void IncrementSuccessfulCommands()
			{
				++AttemptedCommands;
				++SuccessfulCommands;
			}
			public void IncrementFailedCommands()
			{
				++AttemptedCommands;
				++FailedCommands;
			}
			public void IncrementJoins()
			{
				++LoggedJoins;
			}
			public void IncrementLeaves()
			{
				++LoggedLeaves;
			}
			public void IncrementUserChanges()
			{
				++LoggedUserChanges;
			}
			public void IncrementEdits()
			{
				++LoggedEdits;
			}
			public void IncrementDeletes()
			{
				++LoggedDeletes;
			}
			public void IncrementMessages()
			{
				++LoggedMessages;
			}
			public void IncrementImages()
			{
				++LoggedImages;
			}
			public void IncrementGifs()
			{
				++LoggedGifs;
			}
			public void IncrementFiles()
			{
				++LoggedFiles;
			}

			public string FormatLoggedCommands()
			{
				var a = AttemptedCommands;
				var s = SuccessfulCommands;
				var f = FailedCommands;
				var maxNumLen = new[] { a, s, f }.Max().ToString().Length;

				var aStr = "**Attempted:**";
				var sStr = "**Successful:**";
				var fStr = "**Failed:**";
				var maxStrLen = new[] { aStr, sStr, fStr }.Max(x => x.Length);

				var leftSpacing = maxNumLen;
				var rightSpacing = maxStrLen + 1;

				var attempted = FormattingActions.FormatStringsWithLength(aStr, a, rightSpacing, leftSpacing);
				var successful = FormattingActions.FormatStringsWithLength(sStr, s, rightSpacing, leftSpacing);
				var failed = FormattingActions.FormatStringsWithLength(fStr, f, rightSpacing, leftSpacing);
				return String.Join("\n", new[] { attempted, successful, failed });
			}
			public string FormatLoggedActions()
			{
				var j = LoggedJoins;
				var l = LoggedLeaves;
				var u = LoggedUserChanges;
				var e = LoggedEdits;
				var d = LoggedDeletes;
				var i = LoggedImages;
				var g = LoggedGifs;
				var f = LoggedFiles;
				var leftSpacing = new[] { j, l, u, e, d, i, g, f }.Max().ToString().Length;

				const string jTitle = "**Joins:**";
				const string lTitle = "**Leaves:**";
				const string uTitle = "**User Changes:**";
				const string eTitle = "**Edits:**";
				const string dTitle = "**Deletes:**";
				const string iTitle = "**Images:**";
				const string gTitle = "**Gifs:**";
				const string fTitle = "**Files:**";
				var rightSpacing = new[] { jTitle, lTitle, uTitle, eTitle, dTitle, iTitle, gTitle, fTitle }.Max(x => x.Length) + 1;

				var joins = FormattingActions.FormatStringsWithLength(jTitle, j, rightSpacing, leftSpacing);
				var leaves = FormattingActions.FormatStringsWithLength(lTitle, l, rightSpacing, leftSpacing);
				var userChanges = FormattingActions.FormatStringsWithLength(uTitle, u, rightSpacing, leftSpacing);
				var edits = FormattingActions.FormatStringsWithLength(eTitle, e, rightSpacing, leftSpacing);
				var deletes = FormattingActions.FormatStringsWithLength(dTitle, d, rightSpacing, leftSpacing);
				var images = FormattingActions.FormatStringsWithLength(iTitle, i, rightSpacing, leftSpacing);
				var gifs = FormattingActions.FormatStringsWithLength(gTitle, g, rightSpacing, leftSpacing);
				var files = FormattingActions.FormatStringsWithLength(fTitle, f, rightSpacing, leftSpacing);
				return String.Join("\n", new[] { joins, leaves, userChanges, edits, deletes, images, gifs, files });
			}

			#region Bot
			internal Task Log(LogMessage msg)
			{
				if (!String.IsNullOrWhiteSpace(msg.Message))
				{
					ConsoleActions.WriteLine(msg.Message, msg.Source);
				}
				return Task.CompletedTask;
			}
			internal async Task OnGuildAvailable(SocketGuild guild)
			{
				ConsoleActions.WriteLine($"{guild.FormatGuild()} is now online on shard {ClientActions.GetShardIdFor(_Client, guild)}.");
				ConsoleActions.WriteLine($"Current memory usage is: {GetActions.GetMemory().ToString("0.00")}MB.");
				this.AddUsers(guild.MemberCount);
				this.IncrementGuilds();
				await _GuildSettings.AddGuild(guild);
			}
			internal Task OnGuildUnavailable(SocketGuild guild)
			{
				ConsoleActions.WriteLine($"Guild is now offline {guild.FormatGuild()}.");
				this.RemoveUsers(guild.MemberCount);
				this.DecrementGuilds();
				return Task.CompletedTask;
			}
			internal async Task OnJoinedGuild(SocketGuild guild)
			{
				ConsoleActions.WriteLine($"Bot has joined {guild.FormatGuild()}.");

				//Determine what percentage of bot users to leave at
				var users = guild.MemberCount;
				double percentage;
				if (users <= 8)
				{
					percentage = .7;
				}
				else if (users <= 25)
				{
					percentage = .5;
				}
				else if (users <= 40)
				{
					percentage = .4;
				}
				else if (users <= 120)
				{
					percentage = .3;
				}
				else
				{
					percentage = .2;
				}

				//Leave if too many bots
				if ((double)guild.Users.Count(x => x.IsBot) / users > percentage)
				{
					await guild.LeaveAsync();
				}

				//Warn if at the maximum else leave
				var guilds = (await _Client.GetGuildsAsync()).Count;
				var shards = ClientActions.GetShardCount(_Client);
				var curMax = shards * 2500;
				if (guilds + 100 >= curMax)
				{
					ConsoleActions.WriteLine($"The bot currently has {guilds} out of {curMax} possible spots for servers filled. Increase the shard count soon.");
				}
				else if (guilds > curMax)
				{
					await guild.LeaveAsync();
					ConsoleActions.WriteLine($"Left the guild {guild.FormatGuild()} due to having too many guilds on the client and not enough shards.");
				}
			}
			internal Task OnLeftGuild(SocketGuild guild)
			{
				ConsoleActions.WriteLine($"Bot has left {guild.FormatGuild()}.");
				this.RemoveUsers(guild.MemberCount);
				this.DecrementGuilds();
				return Task.CompletedTask;
			}
			#endregion

			#region Server
			internal async Task OnUserJoined(SocketGuildUser user)
			{
				this.IncrementUsers();
				this.IncrementJoins();

				if (HelperFunctions.VerifyBotLogging(_BotSettings, _GuildSettings, user, out var verified))
				{
					//Bans people who join with a given word in their name
					if (verified.GuildSettings.BannedNamesForJoiningUsers.Any(x => user.Username.CaseInsContains(x.Phrase)))
					{
						await PunishmentActions.AutomaticBan(verified.Guild, user.Id, "banned name");
						return;
					}

					await HelperFunctions.HandleJoiningUsersForRaidPrevention(_Timers, verified.GuildSettings, user);
					if (HelperFunctions.VerifyLogAction(verified.GuildSettings))
					{
						var inviteStr = await FormattingActions.FormatUserInviteJoin(verified.GuildSettings, verified.Guild);
						var ageWarningStr = FormattingActions.FormatUserAccountAgeWarning(user);
						var embed = EmbedActions.MakeNewEmbed(null, $"**ID:** {user.Id}{inviteStr}{ageWarningStr}", Constants.JOIN);
						EmbedActions.AddFooter(embed, (user.IsBot ? "Bot Joined" : "User Joined"));
						EmbedActions.AddAuthor(embed, user);
						await MessageActions.SendEmbedMessage(verified.GuildSettings.ServerLog, embed);
					}

					//Welcome message
					if (verified.GuildSettings.WelcomeMessage != null)
					{
						await MessageActions.SendGuildNotification(user, verified.GuildSettings.WelcomeMessage);
					}
				}
			}
			internal async Task OnUserLeft(SocketGuildUser user)
			{
				this.DecrementUsers();
				this.IncrementLeaves();

				//Check if the bot was the one that left
				if (user.Id == Properties.Settings.Default.BotID)
				{
					await _GuildSettings.RemoveGuild(user.Guild);
					return;
				}

				if (HelperFunctions.VerifyBotLogging(_BotSettings, _GuildSettings, user, out var verified))
				{
					//Don't log them to the server if they're someone who was just banned for joining with a banned name
					if (verified.GuildSettings.BannedNamesForJoiningUsers.Any(x => user.Username.CaseInsContains(x.Phrase)))
					{
						return;
					}
					else if (HelperFunctions.VerifyLogAction(verified.GuildSettings))
					{
						var embed = EmbedActions.MakeNewEmbed(null, $"**ID:** {user.Id}{FormattingActions.FormatUserStayLength(user)}", Constants.LEAV);
						EmbedActions.AddFooter(embed, (user.IsBot ? "Bot Left" : "User Left"));
						EmbedActions.AddAuthor(embed, user);
						await MessageActions.SendEmbedMessage(verified.GuildSettings.ServerLog, embed);
					}

					//Goodbye message
					if (verified.GuildSettings.GoodbyeMessage != null)
					{
						await MessageActions.SendGuildNotification(user, verified.GuildSettings.GoodbyeMessage);
					}
				}
			}
			internal async Task OnUserUpdated(SocketUser beforeUser, SocketUser afterUser)
			{
				if (_BotSettings.Pause || beforeUser.Username.CaseInsEquals(afterUser.Username))
				{
					return;
				}

				foreach (var guild in (await _Client.GetGuildsAsync()).Where(x => (x as SocketGuild).Users.Select(y => y.Id).Contains(afterUser.Id)))
				{
					if (HelperFunctions.VerifyBotLogging(_BotSettings, _GuildSettings, guild, out VerifiedLoggingAction verified) && HelperFunctions.VerifyLogAction(verified.GuildSettings))
					{
						this.IncrementUserChanges();
						var embed = EmbedActions.MakeNewEmbed(null, null, Constants.UEDT);
						EmbedActions.AddFooter(embed, "Name Changed");
						EmbedActions.AddField(embed, "Before:", "`" + beforeUser.Username + "`");
						EmbedActions.AddField(embed, "After:", "`" + afterUser.Username + "`", false);
						EmbedActions.AddAuthor(embed, afterUser);
						await MessageActions.SendEmbedMessage(verified.GuildSettings.ServerLog, embed);
					}
				}
			}
			internal async Task OnMessageReceived(SocketMessage message)
			{
				if (HelperFunctions.DisallowBots(message) && HelperFunctions.VerifyBotLogging(_BotSettings, _GuildSettings, message, out var verified))
				{
					//Allow closewords to be handled on an unlogged channel, but don't allow anything else.
					await HelperFunctions.HandleCloseWords(_BotSettings, verified.GuildSettings, message, _Timers);
					if (HelperFunctions.VerifyLogAction(verified.GuildSettings))
					{
						await HelperFunctions.HandleChannelSettings(verified.GuildSettings, message);
						await HelperFunctions.HandleSpamPrevention(verified.GuildSettings, verified.Guild, message, _Timers);
						await HelperFunctions.HandleSlowmode(verified.GuildSettings, message);
						await HelperFunctions.HandleBannedPhrases(_Timers, verified.GuildSettings, verified.Guild, message);
						await HelperFunctions.HandleImageLogging(this, verified.GuildSettings.ImageLog, message);
					}
				}
			}
			internal async Task OnMessageUpdated(Cacheable<IMessage, ulong> cached, SocketMessage message, ISocketMessageChannel channel)
			{
				if (HelperFunctions.DisallowBots(message) && HelperFunctions.VerifyBotLogging(_BotSettings, _GuildSettings, message, out var verified) && HelperFunctions.VerifyLogAction(verified.GuildSettings))
				{
					this.IncrementEdits();
					await HelperFunctions.HandleBannedPhrases(_Timers, verified.GuildSettings, verified.Guild, message);

					//If the before message is not specified always take that as it should be logged. If the embed counts are greater take that as logging too.
					var beforeMessage = cached.HasValue ? cached.Value : null;
					if (verified.GuildSettings.ImageLog != null && beforeMessage?.Embeds.Count() < message.Embeds.Count())
					{
						await HelperFunctions.HandleImageLogging(this, verified.GuildSettings.ImageLog, message);
					}
					if (verified.GuildSettings.ServerLog != null)
					{
						var beforeMsgContent = String.IsNullOrWhiteSpace(beforeMessage?.Content) ? "Empty or unable to be gotten." : beforeMessage?.Content.RemoveAllMarkdown().RemoveDuplicateNewLines();
						var afterMsgContent = String.IsNullOrWhiteSpace(message.Content) ? "Empty or unable to be gotten." : message.Content.RemoveAllMarkdown().RemoveDuplicateNewLines();
						if (beforeMsgContent.Equals(afterMsgContent))
						{
							return;
						}
						else if (beforeMsgContent.Length + afterMsgContent.Length > Constants.MAX_MESSAGE_LENGTH_LONG)
						{
							beforeMsgContent = beforeMsgContent.Length > 667 ? "Long message" : beforeMsgContent;
							afterMsgContent = afterMsgContent.Length > 667 ? "Long message" : afterMsgContent;
						}

						var embed = EmbedActions.MakeNewEmbed(null, null, Constants.MEDT);
						EmbedActions.AddFooter(embed, "Message Updated");
						EmbedActions.AddField(embed, "Before:", $"`{beforeMsgContent}`");
						EmbedActions.AddField(embed, "After:", $"`{afterMsgContent}`", false);
						EmbedActions.AddAuthor(embed, message.Author);
						await MessageActions.SendEmbedMessage(verified.GuildSettings.ServerLog, embed);
					}
				}
			}
			internal Task OnMessageDeleted(Cacheable<IMessage, ulong> cached, ISocketMessageChannel channel)
			{
				//Ignore uncached messages since not much can be done with them
				var message = cached.HasValue ? cached.Value : null;
				if (message != null && HelperFunctions.VerifyBotLogging(_BotSettings, _GuildSettings, channel, out var verified) && HelperFunctions.VerifyLogAction(verified.GuildSettings))
				{
					this.IncrementDeletes();

					//Get the list of deleted messages it contains
					var msgDeletion = verified.GuildSettings.MessageDeletion;
					lock (msgDeletion)
					{
						msgDeletion.AddToList(message);
					}

					//Use a token so the messages do not get sent prematurely
					var cancelToken = msgDeletion.CancelToken;
					if (cancelToken != null)
					{
						cancelToken.Cancel();
					}
					msgDeletion.SetCancelToken(cancelToken = new CancellationTokenSource());

					//I don't know why, but this doesn't run correctly when awaited and it also doesn't work correctly when this method is made async. (sends messages one by one)
					Task.Run(async () =>
					{
						try
						{
							await Task.Delay(TimeSpan.FromSeconds(Constants.SECONDS_DEFAULT), cancelToken.Token);
						}
						catch (Exception)
						{
							return;
						}

						//Give the messages to a new list so they can be removed from the old one
						List<IMessage> deletedMessages;
						lock (msgDeletion)
						{
							deletedMessages = new List<IMessage>(msgDeletion.GetList() ?? new List<IMessage>());
							msgDeletion.ClearList();
						}

						//Put the message content into a list of strings for easy usage
						var formattedMessages = FormattingActions.FormatMessages(deletedMessages.OrderBy(x => x?.CreatedAt.Ticks));
						await MessageActions.SendMessageContainingFormattedDeletedMessages(verified.Guild, verified.GuildSettings.ServerLog, formattedMessages);
					});
				}
				return Task.FromResult(0);
			}
			#endregion
		}

		internal static class HelperFunctions
		{
			#region Verification
			private static SortedDictionary<string, LogAction> _ServerLogMethodLogActions = new SortedDictionary<string, LogAction>
			{
				{ nameof(MyLogModule.OnUserJoined), LogAction.UserJoined },
				{ nameof(MyLogModule.OnUserLeft), LogAction.UserLeft },
				{ nameof(MyLogModule.OnUserUpdated), LogAction.UserUpdated },
				{ nameof(MyLogModule.OnMessageReceived), LogAction.MessageReceived },
				{ nameof(MyLogModule.OnMessageUpdated), LogAction.MessageUpdated },
				{ nameof(MyLogModule.OnMessageDeleted), LogAction.MessageDeleted },
			};

			public static bool DisallowBots(IMessage message)
			{
				return !message.Author.IsBot && !message.Author.IsWebhook;
			}
			public static bool VerifyLogAction(IGuildSettings guildSettings, [CallerMemberName] string callingMethod = null)
			{
				return guildSettings.LogActions.Contains(_ServerLogMethodLogActions[callingMethod]);
			}
			public static bool VerifyBotLogging(IBotSettings botSettings, IGuildSettingsModule guildSettingsModule, IMessage message, out VerifiedLoggingAction verifLoggingAction)
			{
				var allOtherLogRequirements = VerifyBotLogging(botSettings, guildSettingsModule, message.Channel.GetGuild(), out verifLoggingAction);
				var isNotWebhook = !message.Author.IsWebhook;
				var isNotBot = !message.Author.IsBot || message.Author.Id == Properties.Settings.Default.BotID;
				var channelShouldBeLogged = !verifLoggingAction.GuildSettings.IgnoredLogChannels.Contains(message.Channel.Id);
				return allOtherLogRequirements && isNotWebhook && isNotBot && channelShouldBeLogged;
			}
			public static bool VerifyBotLogging(IBotSettings botSettings, IGuildSettingsModule guildSettingsModule, IGuildUser user, out VerifiedLoggingAction verifLoggingAction)
			{
				return VerifyBotLogging(botSettings, guildSettingsModule, user.Guild, out verifLoggingAction);
			}
			public static bool VerifyBotLogging(IBotSettings botSettings, IGuildSettingsModule guildSettingsModule, IChannel channel, out VerifiedLoggingAction verifLoggingAction)
			{
				var allOtherLogRequirements = VerifyBotLogging(botSettings, guildSettingsModule, channel.GetGuild(), out verifLoggingAction);
				var channelShouldBeLogged = !verifLoggingAction.GuildSettings.IgnoredLogChannels.Contains(channel.Id);
				return allOtherLogRequirements && channelShouldBeLogged;
			}
			public static bool VerifyBotLogging(IBotSettings botSettings, IGuildSettingsModule guildSettingsModule, IGuild guild, out VerifiedLoggingAction verifLoggingAction)
			{
				if (botSettings.Pause || !guildSettingsModule.TryGetSettings(guild, out IGuildSettings guildSettings))
				{
					verifLoggingAction = default(VerifiedLoggingAction);
					return false;
				}

				verifLoggingAction = new VerifiedLoggingAction(guild, guildSettings);
				return true;
			}
			#endregion

			#region Message Received
			//I could use switches for these but I think they make the methods look way too long and harder to read
			private static Dictionary<SpamType, Func<IMessage, int>> _GetSpamNumberFuncs = new Dictionary<SpamType, Func<IMessage, int>>
			{
				{ SpamType.Message, (message) => int.MaxValue },
				{ SpamType.LongMessage, (message) => message.Content?.Length ?? 0 },
				{ SpamType.Link, (message) => message.Content?.Split(' ')?.Count(x => Uri.IsWellFormedUriString(x, UriKind.Absolute)) ?? 0 },
				{ SpamType.Image, (message) => message.Attachments.Where(x => x.Height != null || x.Width != null).Count() + message.Embeds.Where(x => x.Image != null || x.Video != null).Count() },
				{ SpamType.Mention, (message) => message.MentionedUserIds.Distinct().Count() },
			};
			private static Dictionary<PunishmentType, Func<BannedPhraseUser, int>> _BannedPhrasePunishmentFuncs = new Dictionary<PunishmentType, Func<BannedPhraseUser, int>>
			{
				{ PunishmentType.RoleMute, (user) => { user.IncreaseRoleCount(); return user.MessagesForRole; } },
				{ PunishmentType.Kick, (user) => { user.IncreaseKickCount(); return user.MessagesForKick; } },
				{ PunishmentType.Ban, (user) => { user.IncreaseBanCount(); return user.MessagesForBan; } },
			};
			private static Dictionary<PunishmentType, Action<BannedPhraseUser>> _BannedPhraseResets = new Dictionary<PunishmentType, Action<BannedPhraseUser>>
			{
				{ PunishmentType.RoleMute, (user) => user.ResetRoleCount() },
				{ PunishmentType.Kick, (user) => user.ResetKickCount() },
				{ PunishmentType.Ban, (user) => user.ResetBanCount() },
			};

			public static async Task HandleChannelSettings(IGuildSettings guildSettings, IMessage message)
			{
				var author = message.Author as IGuildUser;
				if (author == null || author.GuildPermissions.Administrator)
				{
					return;
				}

				if (guildSettings.ImageOnlyChannels.Contains(message.Channel.Id)
					&& !(message.Attachments.Any(x => x.Height != null || x.Width != null) || message.Embeds.Any(x => x.Image != null)))
				{
					await message.DeleteAsync();
				}
			}
			public static async Task HandleImageLogging(ILogModule logging, ITextChannel logChannel, IMessage message)
			{
				if (message.Attachments.Any())
				{
					await HelperFunctions.LogImage(logging, logChannel, message, false);
				}
				if (message.Embeds.Any())
				{
					await HelperFunctions.LogImage(logging, logChannel, message, true);
				}
			}
			public static async Task HandleCloseWords(IBotSettings botSettings, IGuildSettings guildSettings, IMessage message, ITimersModule timers = null)
			{
				if (timers == null || !int.TryParse(message.Content, out int number) || number < 1 || number > 6)
				{
					return;
				}

				--number;
				var closeWordList = timers.GetOutActiveCloseQuote(message.Author.Id);
				if (!closeWordList.Equals(default(ActiveCloseWord<Quote>)) && closeWordList.List.Count > number)
				{
					await MessageActions.SendChannelMessage(message.Channel, closeWordList.List[number].Word.Text);
				}
				var closeHelpList = timers.GetOutActiveCloseHelp(message.Author.Id);
				if (!closeHelpList.Equals(default(ActiveCloseWord<HelpEntry>)) && closeHelpList.List.Count > number)
				{
					var help = closeHelpList.List[number].Word;
					var embed = EmbedActions.MakeNewEmbed(help.Name, help.ToString(), prefix: GetActions.GetPrefix(botSettings, guildSettings));
					EmbedActions.AddFooter(embed, "Help");
					await MessageActions.SendEmbedMessage(message.Channel, embed);
				}
			}
			public static async Task HandleSpamPrevention(IGuildSettings guildSettings, IGuild guild, IMessage message, ITimersModule timers  = null)
			{
				//TODO: Make sure this works
				if (message.Author.CanBeModifiedByUser(UserActions.GetBot(guild)))
				{
					var spamUser = guildSettings.SpamPreventionUsers.FirstOrDefault(x => x.User.Id == message.Author.Id);
					if (spamUser == null)
					{
						guildSettings.SpamPreventionUsers.ThreadSafeAdd(spamUser = new SpamPreventionUser(message.Author as IGuildUser));
					}

					var spam = false;
					foreach (var spamType in Enum.GetValues(typeof(SpamType)).Cast<SpamType>())
					{
						var spamPrev = guildSettings.SpamPreventionDictionary[spamType];
						if (spamPrev == null || !spamPrev.Enabled)
						{
							continue;
						}

						//Ticks should be small enough that this will not allow duplicates of the same message, but can still allow rapidly spammed messages
						var userSpamList = spamUser.SpamLists[spamType];
						if (_GetSpamNumberFuncs[spamType](message) >= spamPrev.RequiredSpamPerMessageOrTimeInterval && !userSpamList.Any(x => x.GetTime().Ticks == message.CreatedAt.UtcTicks))
						{
							userSpamList.ThreadSafeAdd(new BasicTimeInterface(message.CreatedAt.UtcDateTime));
						}

						if (spamUser.CheckIfAllowedToPunish(spamPrev, spamType))
						{
							//Make sure they have the lowest vote count required to kick and the most severe punishment type
							await MessageActions.DeleteMessage(message);
							spamUser.ChangeVotesRequired(spamPrev.VotesForKick);
							spamUser.ChangePunishmentType(spamPrev.PunishmentType);
							spamUser.EnablePunishable();
							spam = true;
						}
					}

					if (spam)
					{
						var content = $"The user `{message.Author.FormatUser()}` needs `{spamUser.VotesRequired - spamUser.UsersWhoHaveAlreadyVoted.Count}` votes to be kicked. Vote by mentioning them.";
						await MessageActions.MakeAndDeleteSecondaryMessage(message.Channel, null, content, 10, timers);
					}
				}

				//Get the users who are able to be punished by the spam prevention
				var users = guildSettings.SpamPreventionUsers.Where(x => true
					&& x.PotentialPunishment
					&& x.User.Id != message.Author.Id
					&& message.MentionedUserIds.Contains(x.User.Id)
					&& !x.UsersWhoHaveAlreadyVoted.Contains(message.Author.Id));

				foreach (var user in users)
				{
					user.IncreaseVotesToKick(message.Author.Id);
					if (user.UsersWhoHaveAlreadyVoted.Count < user.VotesRequired)
					{
						return;
					}

					await user.SpamPreventionPunishment(guildSettings, timers);

					//Reset their current spam count and the people who have already voted on them so they don't get destroyed instantly if they join back
					user.ResetSpamUser();
				}
			}
			public static async Task HandleSlowmode(IGuildSettings guildSettings, IMessage message)
			{
				//Don't bother doing stuff on the user if they're immune
				var slowmode = guildSettings.Slowmode;
				if (slowmode == null || !slowmode.Enabled || (message.Author as IGuildUser).RoleIds.Intersect(slowmode.ImmuneRoleIds).Any())
				{
					return;
				}

				var user = slowmode.Users.FirstOrDefault(x => x.User.Id == message.Author.Id);
				if (user == null)
				{
					slowmode.Users.ThreadSafeAdd(user = new SlowmodeUser(message.Author as IGuildUser, slowmode.BaseMessages, slowmode.Interval));
				}

				//If the user still has messages left, check if this is the first of their interval. Start a countdown if it is. Else lower by one and/or delete the message.
				if (user.CurrentMessagesLeft > 0)
				{
					if (user.CurrentMessagesLeft == user.BaseMessages)
					{
						user.SetNewTime();
					}

					user.LowerMessagesLeft();
				}
				else
				{
					await MessageActions.DeleteMessage(message);
				}
			}
			public static async Task HandleBannedPhrases(ITimersModule timers, IGuildSettings guildSettings, IGuild guild, IMessage message)
			{
				//Ignore admins and messages older than an hour. (Accidentally deleted something important once due to not having these checks in place, but this should stop most accidental deletions)
				if ((message.Author as IGuildUser).GuildPermissions.Administrator || (int)DateTime.UtcNow.Subtract(message.CreatedAt.UtcDateTime).TotalHours > 0)
				{
					return;
				}

				var str = guildSettings.BannedPhraseStrings.FirstOrDefault(x => message.Content.CaseInsContains(x.Phrase));
				if (str != null)
				{
					await HandleBannedPhrasePunishments(timers, guildSettings, guild, message, str);
					return;
				}

				var regex = guildSettings.BannedPhraseRegex.FirstOrDefault(x => SpamActions.CheckIfRegMatch(message.Content, x.Phrase));
				if (regex != null)
				{
					await HandleBannedPhrasePunishments(timers, guildSettings, guild, message, regex);
					return;
				}
			}
			public static async Task HandleBannedPhrasePunishments(ITimersModule timers, IGuildSettings guildSettings, IGuild guild, IMessage message, BannedPhrase phrase)
			{
				await MessageActions.DeleteMessage(message);

				var user = guildSettings.BannedPhraseUsers.FirstOrDefault(x => x.User.Id == message.Author.Id);
				if (user == null)
				{
					guildSettings.BannedPhraseUsers.Add(user = new BannedPhraseUser(message.Author as IGuildUser));
				}

				//Get the banned phrases punishments from the guild
				if (!SpamActions.TryGetPunishment(guildSettings, phrase.Punishment, _BannedPhrasePunishmentFuncs[phrase.Punishment](user), out BannedPhrasePunishment punishment))
				{
					return;
				}

				//TODO: include all automatic punishments in this
				await PunishmentActions.AutomaticPunishments(guildSettings, user.User, phrase.Punishment, false, punishment.PunishmentTime, timers);
				_BannedPhraseResets[phrase.Punishment](user);
			}
			#endregion

			public static async Task LogImage(ILogModule currentLogModule, ITextChannel channel, IMessage message, bool embeds)
			{
				var attachmentURLs = new List<string>();
				var embedURLs = new List<string>();
				var videoEmbeds = new List<IEmbed>();
				if (!embeds && message.Attachments.Any())
				{
					//If attachment, the file is hosted on discord which has a concrete URL name for files (cdn.discordapp.com/attachments/.../x.png)
					attachmentURLs = message.Attachments.Select(x => x.Url).Distinct().ToList();
				}
				else if (embeds && message.Embeds.Any())
				{
					//If embed this is slightly trickier, but only images/videos can embed (AFAIK)
					foreach (var embed in message.Embeds)
					{
						if (embed.Video == null)
						{
							//If no video then it has to be just an image
							if (!String.IsNullOrEmpty(embed.Thumbnail?.Url))
							{
								embedURLs.Add(embed.Thumbnail?.Url);
							}
							if (!String.IsNullOrEmpty(embed.Image?.Url))
							{
								embedURLs.Add(embed.Image?.Url);
							}
						}
						else
						{
							//Add the video URL and the thumbnail URL
							videoEmbeds.Add(embed);
						}
					}
				}
				//Attached files
				foreach (var attachmentURL in attachmentURLs)
				{
					//Image attachment
					if (Constants.VALID_IMAGE_EXTENSIONS.CaseInsContains(Path.GetExtension(attachmentURL)))
					{
						var desc = $"**Channel:** `{message.Channel.FormatChannel()}`\n**Message Id:** `{message.Id}`";
						var embed = EmbedActions.MakeNewEmbed(null, desc, Constants.ATCH, attachmentURL);
						EmbedActions.AddFooter(embed, "Attached Image");
						EmbedActions.AddAuthor(embed, message.Author, attachmentURL);
						await MessageActions.SendEmbedMessage(channel, embed);

						currentLogModule.IncrementImages();
					}
					//Gif attachment
					else if (Constants.VALID_GIF_EXTENTIONS.CaseInsContains(Path.GetExtension(attachmentURL)))
					{
						var desc = $"**Channel:** `{message.Channel.FormatChannel()}`\n**Message Id:** `{message.Id}`";
						var embed = EmbedActions.MakeNewEmbed(null, desc, Constants.ATCH, attachmentURL);
						EmbedActions.AddFooter(embed, "Attached Gif");
						EmbedActions.AddAuthor(embed, message.Author, attachmentURL);
						await MessageActions.SendEmbedMessage(channel, embed);

						currentLogModule.IncrementGifs();
					}
					//Random file attachment
					else
					{
						var desc = $"**Channel:** `{message.Channel.FormatChannel()}`\n**Message Id:** `{message.Id}`";
						var embed = EmbedActions.MakeNewEmbed(null, desc, Constants.ATCH, attachmentURL);
						EmbedActions.AddFooter(embed, "Attached File");
						EmbedActions.AddAuthor(embed, message.Author, attachmentURL);
						await MessageActions.SendEmbedMessage(channel, embed);

						currentLogModule.IncrementFiles();
					}
				}
				//Embedded images
				foreach (var embedURL in embedURLs.Distinct())
				{
					var desc = $"**Channel:** `{message.Channel.FormatChannel()}`\n**Message Id:** `{message.Id}`";
					var embed = EmbedActions.MakeNewEmbed(null, desc, Constants.ATCH, embedURL);
					EmbedActions.AddFooter(embed, "Embedded Image");
					EmbedActions.AddAuthor(embed, message.Author, embedURL);
					await MessageActions.SendEmbedMessage(channel, embed);

					currentLogModule.IncrementImages();
				}
				//Embedded videos/gifs
				foreach (var videoEmbed in videoEmbeds.GroupBy(x => x.Url).Select(x => x.First()))
				{
					var desc = $"**Channel:** `{message.Channel.FormatChannel()}`\n**Message Id:** `{message.Id}`";
					var embed = EmbedActions.MakeNewEmbed(null, desc, Constants.ATCH, videoEmbed.Thumbnail?.Url);
					EmbedActions.AddFooter(embed, "Embedded " + (Constants.VALID_GIF_EXTENTIONS.CaseInsContains(Path.GetExtension(videoEmbed.Thumbnail?.Url)) ? "Gif" : "Video"));
					EmbedActions.AddAuthor(embed, message.Author, videoEmbed.Url);
					await MessageActions.SendEmbedMessage(channel, embed);

					currentLogModule.IncrementGifs();
				}
			}
			public static async Task HandleJoiningUsersForRaidPrevention(ITimersModule timers, IGuildSettings guildSettings, IGuildUser user)
			{
				var antiRaid = guildSettings.RaidPreventionDictionary[RaidType.Regular];
				if (antiRaid != null && antiRaid.Enabled)
				{
					await antiRaid.RaidPreventionPunishment(guildSettings, user, timers);
				}
				var antiJoin = guildSettings.RaidPreventionDictionary[RaidType.RapidJoins];
				if (antiJoin != null && antiJoin.Enabled)
				{
					antiJoin.Add(user.JoinedAt.Value.UtcDateTime);
					if (antiJoin.GetSpamCount() < antiJoin.UserCount)
					{
						return;
					}

					await antiJoin.RaidPreventionPunishment(guildSettings, user, timers);
					if (guildSettings.ServerLog == null)
					{
						return;
					}

					await MessageActions.SendEmbedMessage(guildSettings.ServerLog, EmbedActions.MakeNewEmbed("Anti Rapid Join Mute", $"**User:** {user.FormatUser()}"));
				}
			}
		}
	}
}