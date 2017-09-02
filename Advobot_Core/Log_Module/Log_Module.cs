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
		public sealed class MyLogModule : ILogModule
		{
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

			public ILog Log { get; private set; }

			public MyLogModule(IDiscordClient client, IBotSettings botSettings, IGuildSettingsModule guildSettings, ITimersModule timers)
			{
				if (client is DiscordSocketClient)
				{
					CreateLogHolder(client as DiscordSocketClient, botSettings, guildSettings, timers);
				}
				else if (client is DiscordShardedClient)
				{
					CreateLogHolder(client as DiscordShardedClient, botSettings, guildSettings, timers);
				}
				else
				{
					throw new ArgumentException("Invalid client provided. Must be either a DiscordSocketClient or a DiscordShardedClient.");
				}
			}

			private void CreateLogHolder(DiscordSocketClient client, IBotSettings botSettings, IGuildSettingsModule guildSettings, ITimersModule timers)
			{
				Log = new MyLog(client, botSettings, guildSettings, this, timers);

				client.Log						+= Log.Log;
				client.GuildAvailable			+= Log.OnGuildAvailable;
				client.GuildUnavailable			+= Log.OnGuildUnavailable;
				client.JoinedGuild				+= Log.OnJoinedGuild;
				client.LeftGuild				+= Log.OnLeftGuild;
				client.UserJoined				+= Log.OnUserJoined;
				client.UserLeft					+= Log.OnUserLeft;
				client.UserUpdated				+= Log.OnUserUpdated;
				client.MessageReceived			+= Log.OnMessageReceived;
				client.MessageUpdated			+= Log.OnMessageUpdated;
				client.MessageDeleted			+= Log.OnMessageDeleted;
			}
			private void CreateLogHolder(DiscordShardedClient client, IBotSettings botSettings, IGuildSettingsModule guildSettings, ITimersModule timers)
			{
				Log = new MyLog(client, botSettings, guildSettings, this, timers);

				client.Log						+= Log.Log;
				client.GuildAvailable			+= Log.OnGuildAvailable;
				client.GuildUnavailable			+= Log.OnGuildUnavailable;
				client.JoinedGuild				+= Log.OnJoinedGuild;
				client.LeftGuild				+= Log.OnLeftGuild;
				client.UserJoined				+= Log.OnUserJoined;
				client.UserLeft					+= Log.OnUserLeft;
				client.UserUpdated				+= Log.OnUserUpdated;
				client.MessageReceived			+= Log.OnMessageReceived;
				client.MessageUpdated			+= Log.OnMessageUpdated;
				client.MessageDeleted			+= Log.OnMessageDeleted;
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
		}

		internal sealed class MyLog : ILog
		{
			private IDiscordClient _Client { get; }
			private IBotSettings _BotSettings { get; }
			private IGuildSettingsModule _GuildSettings { get; }
			private ILogModule _Logging { get; }
			private ITimersModule _Timers { get; }

			public MyLog(IDiscordClient client, IBotSettings botSettings, IGuildSettingsModule guildSettings, ILogModule logging, ITimersModule timers)
			{
				_Client = client;
				_BotSettings = botSettings;
				_GuildSettings = guildSettings;
				_Logging = logging;
				_Timers = timers;
			}

			//Bot
			public Task Log(LogMessage msg)
			{
				if (!String.IsNullOrWhiteSpace(msg.Message))
				{
					ConsoleActions.WriteLine(msg.Message, msg.Source);
				}

				return Task.CompletedTask;
			}
			public async Task OnGuildAvailable(SocketGuild guild)
			{
				ConsoleActions.WriteLine($"{guild.FormatGuild()} is now online on shard {ClientActions.GetShardIdFor(_Client, guild)}.");
				ConsoleActions.WriteLine($"Current memory usage is: {GetActions.GetMemory().ToString("0.00")}MB.");
				_Logging.AddUsers(guild.MemberCount);
				_Logging.IncrementGuilds();

				await _GuildSettings.AddGuild(guild);
			}
			public Task OnGuildUnavailable(SocketGuild guild)
			{
				ConsoleActions.WriteLine($"Guild is now offline {guild.FormatGuild()}.");
				_Logging.RemoveUsers(guild.MemberCount);
				_Logging.DecrementGuilds();

				return Task.CompletedTask;
			}
			public async Task OnJoinedGuild(SocketGuild guild)
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

				//Warn if at the maximum
				var guilds = (await _Client.GetGuildsAsync()).Count;
				var shards = ClientActions.GetShardCount(_Client);
				var curMax = shards * 2500;
				if (guilds + 100 >= curMax)
				{
					ConsoleActions.WriteLine($"The bot currently has {guilds} out of {curMax} possible spots for servers filled. Please increase the shard count.");
				}
				//Leave the guild
				if (guilds > curMax)
				{
					await guild.LeaveAsync();
					ConsoleActions.WriteLine($"Left the guild {guild.FormatGuild()} due to having too many guilds on the client and not enough shards.");
				}

				return;
			}
			public Task OnLeftGuild(SocketGuild guild)
			{
				ConsoleActions.WriteLine($"Bot has left {guild.FormatGuild()}.");

				_Logging.RemoveUsers(guild.MemberCount);
				_Logging.DecrementGuilds();

				return Task.CompletedTask;
			}

			//Server
			public async Task OnUserJoined(SocketGuildUser user)
			{
				IGuild guild;
				IGuildSettings guildSettings;
				_Logging.IncrementUsers();
				_Logging.IncrementJoins();

				if (!Verification.VerifyBotLogging(_BotSettings, _GuildSettings, user, out var verified))
				{
					return;
				}
				else
				{
					guild = verified.Guild;
					guildSettings = verified.GuildSettings;
					await Other.HandleJoiningUsersForRaidPrevention(_Timers, guildSettings, user);
				}

				if (!Verification.VerifyLogAction(verified.GuildSettings))
				{
					return;
				}

				//Bans people who join with a given word in their name
				if (guildSettings.BannedNamesForJoiningUsers.Any(x => user.Username.CaseInsContains(x.Phrase)))
				{
					await PunishmentActions.AutomaticBan(guild, user.Id, "banned name");
					return;
				}

				var inviteStr = await FormattingActions.FormatUserInviteJoin(guildSettings, guild);
				var ageWarningStr = FormattingActions.FormatUserAccountAgeWarning(user);
				var embed = EmbedActions.MakeNewEmbed(null, $"**ID:** {user.Id}{inviteStr}{ageWarningStr}", Constants.JOIN);
				EmbedActions.AddFooter(embed, (user.IsBot ? "Bot Joined" : "User Joined"));
				EmbedActions.AddAuthor(embed, user);
				await MessageActions.SendEmbedMessage(guildSettings.ServerLog, embed);

				//Welcome message
				if (verified.GuildSettings.WelcomeMessage != null)
				{
					await MessageActions.SendGuildNotification(user, verified.GuildSettings.WelcomeMessage);
				}
			}
			public async Task OnUserLeft(SocketGuildUser user)
			{
				IGuild guild;
				IGuildSettings guildSettings;
				_Logging.DecrementUsers();
				_Logging.IncrementLeaves();

				//Check if the bot was the one that left
				if (user.Id == Properties.Settings.Default.BotID)
				{
					await _GuildSettings.RemoveGuild(user.Guild);
					return;
				}

				if (!Verification.VerifyBotLogging(_BotSettings, _GuildSettings, user, out var verified) ||
					!Verification.VerifyLogAction(verified.GuildSettings) ||
					//Don't log them to the server if they're someone who was just banned for joining with a banned name
					verified.GuildSettings.BannedNamesForJoiningUsers.Any(x => user.Username.CaseInsContains(x.Phrase)))
				{
					return;
				}
				else
				{
					guild = verified.Guild;
					guildSettings = verified.GuildSettings;
				}

				var embed = EmbedActions.MakeNewEmbed(null, $"**ID:** {user.Id}{FormattingActions.FormatUserStayLength(user)}", Constants.LEAV);
				EmbedActions.AddFooter(embed, (user.IsBot ? "Bot Left" : "User Left"));
				EmbedActions.AddAuthor(embed, user);
				await MessageActions.SendEmbedMessage(guildSettings.ServerLog, embed);

				//Goodbye message
				if (verified.GuildSettings.GoodbyeMessage != null)
				{
					await MessageActions.SendGuildNotification(user, verified.GuildSettings.GoodbyeMessage);
				}
			}
			public async Task OnUserUpdated(SocketUser beforeUser, SocketUser afterUser)
			{
				_Logging.IncrementUserChanges();

				if (beforeUser.Username == null || afterUser.Username == null || _BotSettings.Pause || beforeUser.Username.CaseInsEquals(afterUser.Username))
				{
					return;
				}

				foreach (var guild in (await _Client.GetGuildsAsync()).Where(x => (x as SocketGuild).Users.Select(y => y.Id).Contains(afterUser.Id)))
				{
					if (!Verification.VerifyBotLogging(_BotSettings, _GuildSettings, guild, out VerifiedLoggingAction verified) ||
						!Verification.VerifyLogAction(verified.GuildSettings))
					{
						return;
					}

					var embed = EmbedActions.MakeNewEmbed(null, null, Constants.UEDT);
					EmbedActions.AddFooter(embed, "Name Changed");
					EmbedActions.AddField(embed, "Before:", "`" + beforeUser.Username + "`");
					EmbedActions.AddField(embed, "After:", "`" + afterUser.Username + "`", false);
					EmbedActions.AddAuthor(embed, afterUser);
					await MessageActions.SendEmbedMessage(verified.GuildSettings.ServerLog, embed);
				}
			}
			public async Task OnMessageReceived(SocketMessage message)
			{
				IGuild guild;
				IGuildSettings guildSettings;
				if (!Verification.VerifyBotLogging(_BotSettings, _GuildSettings, message, out var verified))
				{
					return;
				}

				guild = verified.Guild;
				guildSettings = verified.GuildSettings;

				//Allow closewords to be handled on an unlogged channel, but don't allow anything else.
				await MessageRecieved.HandleCloseWords(_BotSettings, guildSettings, message, _Timers);
				if (Verification.VerifyLogAction(verified.GuildSettings))
				{
					await MessageRecieved.HandleChannelSettings(guildSettings, message);
					await MessageRecieved.HandleSpamPrevention(guildSettings, guild, message, _Timers);
					await SpamActions.HandleSlowmode(guildSettings, message);
					await SpamActions.HandleBannedPhrases(_Timers, guildSettings, guild, message);
					await MessageRecieved.HandleImageLogging(_Logging, guildSettings.ImageLog, message);
				}
			}
			public async Task OnMessageUpdated(Cacheable<IMessage, ulong> cached, SocketMessage afterMessage, ISocketMessageChannel channel)
			{
				IGuild guild;
				IGuildSettings guildSettings;
				_Logging.IncrementEdits();

				if (!Verification.VerifyBotLogging(_BotSettings, _GuildSettings, afterMessage, out var verified) ||
					!Verification.VerifyLogAction(verified.GuildSettings))
				{
					return;
				}
				else
				{
					guild = verified.Guild;
					guildSettings = verified.GuildSettings;
				}

				var beforeMessage = cached.HasValue ? cached.Value : null;
				await SpamActions.HandleBannedPhrases(_Timers, guildSettings, guild, afterMessage);

				//If the before message is not specified always take that as it should be logged. If the embed counts are greater take that as logging too.
				if (guildSettings.ImageLog != null && beforeMessage?.Embeds.Count() < afterMessage.Embeds.Count())
				{
					await MessageRecieved.HandleImageLogging(_Logging, guildSettings.ImageLog, afterMessage);
				}
				if (guildSettings.ServerLog != null)
				{
					var beforeMsgContent = String.IsNullOrWhiteSpace(beforeMessage?.Content) ? "Empty or unable to be gotten." : beforeMessage?.Content.RemoveAllMarkdown().RemoveDuplicateNewLines();
					var afterMsgContent = String.IsNullOrWhiteSpace(afterMessage.Content) ? "Empty or unable to be gotten." : afterMessage.Content.RemoveAllMarkdown().RemoveDuplicateNewLines();

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
					EmbedActions.AddAuthor(embed, afterMessage.Author);
					await MessageActions.SendEmbedMessage(guildSettings.ServerLog, embed);
				}
			}
			public Task OnMessageDeleted(Cacheable<IMessage, ulong> cached, ISocketMessageChannel channel)
			{
				IGuild guild;
				IGuildSettings guildSettings;
				_Logging.IncrementDeletes();

				if (!Verification.VerifyBotLogging(_BotSettings, _GuildSettings, channel, out var verified) ||
					!Verification.VerifyLogAction(verified.GuildSettings))
				{
					return Task.FromResult(0);
				}
				else
				{
					guild = verified.Guild;
					guildSettings = verified.GuildSettings;
				}

				var message = cached.HasValue ? cached.Value : null;

				//Get the list of deleted messages it contains
				var msgDeletion = guildSettings.MessageDeletion;
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

				//I don't know why, but this doesn't run correctly when awaited
				var t = Task.Run(async () =>
				{
					try
					{
						await Task.Delay(TimeSpan.FromSeconds(Constants.SECONDS_DEFAULT), cancelToken.Token);
					}
					catch (TaskCanceledException)
					{
						return;
					}
					catch (Exception e)
					{
						ConsoleActions.ExceptionToConsole(e);
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
					await MessageActions.SendMessageContainingFormattedDeletedMessages(guild, guildSettings.ServerLog, formattedMessages);
				});
				return Task.FromResult(0);
			}
		}

		internal static class MessageRecieved
		{
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
					await Other.LogImage(logging, logChannel, message, false);
				}
				if (message.Embeds.Any())
				{
					await Other.LogImage(logging, logChannel, message, true);
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
				if (message.Author.CanBeModifiedByUser(UserActions.GetBot(guild)))
				{
					await SpamActions.HandleSpamPrevention(guildSettings, guild, message.Author as IGuildUser, message, timers);
				}

				//TODO: Make this work for all spam types
				//Get the users primed to be punished by the spam prevention
				var users = guildSettings.SpamPreventionUsers.Where(x =>
				{
					return true
					&& x.PotentialPunishment
					&& x.User.Id != message.Author.Id
					&& message.MentionedUserIds.Contains(x.User.Id)
					&& !x.UsersWhoHaveAlreadyVoted.Contains(message.Author.Id);
				});

				foreach (var user in users)
				{
					user.IncreaseVotesToKick(message.Author.Id);
					if (user.UsersWhoHaveAlreadyVoted.Count < user.VotesRequired)
						return;

					await user.SpamPreventionPunishment(guildSettings, timers);

					//Reset their current spam count and the people who have already voted on them so they don't get destroyed instantly if they join back
					user.ResetSpamUser();
				}
			}
		}

		internal static class Verification
		{
			private static SortedDictionary<string, LogAction> _ServerLogMethodLogActions = new SortedDictionary<string, LogAction>
			{
				{ nameof(MyLog.OnUserJoined), LogAction.UserJoined },
				{ nameof(MyLog.OnUserLeft), LogAction.UserLeft },
				{ nameof(MyLog.OnUserUpdated), LogAction.UserUpdated },
				{ nameof(MyLog.OnMessageReceived), LogAction.MessageReceived },
				{ nameof(MyLog.OnMessageUpdated), LogAction.MessageUpdated },
				{ nameof(MyLog.OnMessageDeleted), LogAction.MessageDeleted },
			};

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
		}

		internal static class Other
		{
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
					if (antiJoin.GetSpamCount() >= antiJoin.UserCount)
					{
						await antiJoin.RaidPreventionPunishment(guildSettings, user, timers);
						if (guildSettings.ServerLog != null)
						{
							await MessageActions.SendEmbedMessage(guildSettings.ServerLog, EmbedActions.MakeNewEmbed("Anti Rapid Join Mute", $"**User:** {user.FormatUser()}"));
						}
					}
				}
			}
		}
	}
}