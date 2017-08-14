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
using System.Threading.Tasks;

namespace Advobot
{
	namespace Logging
	{
		public sealed class MyLogModule : ILogModule
		{
			public List<LoggedCommand> RanCommands { get; } = new List<LoggedCommand>();

			public uint TotalUsers { get; private set; } = 0;
			public uint TotalGuilds { get; private set; } = 0;
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
				++SuccessfulCommands;
			}
			public void IncrementFailedCommands()
			{
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
				var a = SuccessfulCommands + FailedCommands;
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

		public sealed class MyLog : ILog
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
				ConsoleActions.WriteLine(String.Format("{0} is now online on shard {1}.", guild.FormatGuild(), ClientActions.GetShardIdFor(_Client, guild)));
				ConsoleActions.WriteLine(String.Format("Current memory usage is: {0}MB.", GetActions.GetMemory(_BotSettings.Windows).ToString("0.00")));
				_Logging.AddUsers(guild.MemberCount);
				_Logging.IncrementGuilds();

				await _GuildSettings.AddGuild(guild);
			}
			public Task OnGuildUnavailable(SocketGuild guild)
			{
				ConsoleActions.WriteLine(String.Format("Guild is now offline {0}.", guild.FormatGuild()));
				_Logging.RemoveUsers(guild.MemberCount);
				_Logging.DecrementGuilds();

				return Task.CompletedTask;
			}
			public async Task OnJoinedGuild(SocketGuild guild)
			{
				ConsoleActions.WriteLine(String.Format("Bot has joined {0}.", guild.FormatGuild()));

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
					ConsoleActions.WriteLine(String.Format("The bot currently has {0} out of {1} possible spots for servers filled. Please increase the shard count.", guilds, curMax));
				}
				//Leave the guild
				if (guilds > curMax)
				{
					await guild.LeaveAsync();
					ConsoleActions.WriteLine(String.Format("Left the guild {0} due to having too many guilds on the client and not enough shards.", guild.FormatGuild()));
				}

				return;
			}
			public Task OnLeftGuild(SocketGuild guild)
			{
				ConsoleActions.WriteLine(String.Format("Bot has left {0}.", guild.FormatGuild()));

				_Logging.RemoveUsers(guild.MemberCount);
				_Logging.DecrementGuilds();

				return Task.CompletedTask;
			}

			//Server
			public async Task OnUserJoined(SocketGuildUser user)
			{
				_Logging.IncrementUsers();

				if (OtherLogActions.VerifyServerLoggingAction(_BotSettings, _GuildSettings, user, LogAction.UserJoined, out VerifiedLoggingAction verified))
				{
					var guild = verified.Guild;
					var guildSettings = verified.GuildSettings;
					var serverLog = verified.LoggingChannel;

					if (guildSettings != null)
					{
						await OtherLogActions.HandleJoiningUsersForRaidPrevention(_Timers, guildSettings, user);
					}

					//Bans people who join with a given word in their name
					if (guildSettings.BannedNamesForJoiningUsers.Any(x => user.Username.CaseInsContains(x.Phrase)))
					{
						await PunishmentActions.AutomaticBan(guild, user.Id, "banned name");
						return;
					}

					var curInv = await InviteActions.GetInviteUserJoinedOn(guildSettings, guild);
					var inviteStr = "";
					if (curInv != null)
					{
						inviteStr = String.Format("\n**Invite:** {0}", curInv.Code);
					}
					var userAccAge = (DateTime.UtcNow - user.CreatedAt.ToUniversalTime());
					var ageWarningStr = "";
					if (userAccAge.TotalHours < 24)
					{
						ageWarningStr = String.Format("\n**New Account:** {0} hours, {1} minutes old.", (int)userAccAge.TotalHours, userAccAge.Minutes);
					}

					var embed = EmbedActions.MakeNewEmbed(null, String.Format("**ID:** {0}{1}{2}", user.Id, inviteStr, ageWarningStr), Constants.JOIN);
					EmbedActions.AddFooter(embed, (user.IsBot ? "Bot Joined" : "User Joined"));
					EmbedActions.AddAuthor(embed, user);
					await MessageActions.SendEmbedMessage(serverLog, embed);

					_Logging.IncrementJoins();
				}
				else
				{
					var guildSettings = verified.GuildSettings;
					if (guildSettings == null)
						return;

					await OtherLogActions.HandleJoiningUsersForRaidPrevention(_Timers, guildSettings, user);
				}

				//Welcome message
				await MessageActions.SendGuildNotification(user, verified.GuildSettings.WelcomeMessage);
			}
			public async Task OnUserLeft(SocketGuildUser user)
			{
				_Logging.DecrementUsers();

				//Check if the bot was the one that left
				if (user.Id == Properties.Settings.Default.BotID)
				{
					await _GuildSettings.RemoveGuild(user.Guild);
					return;
				}

				if (OtherLogActions.VerifyServerLoggingAction(_BotSettings, _GuildSettings, user, LogAction.UserLeft, out VerifiedLoggingAction verified))
				{
					var guild = verified.Guild;
					var guildSettings = verified.GuildSettings;
					var serverLog = verified.LoggingChannel;

					//Don't log them to the server if they're someone who was just banned for joining with a banned name
					if (guildSettings.BannedNamesForJoiningUsers.Any(x => user.Username.CaseInsContains(x.Phrase)))
						return;

					var timeStayedStr = "";
					if (user.JoinedAt.HasValue)
					{
						var timeStayed = (DateTime.UtcNow - user.JoinedAt.Value.ToUniversalTime());
						timeStayedStr = String.Format("\n**Stayed for:** {0}:{1:00}:{2:00}:{3:00}", timeStayed.Days, timeStayed.Hours, timeStayed.Minutes, timeStayed.Seconds);
					}

					var embed = EmbedActions.MakeNewEmbed(null, String.Format("**ID:** {0}{1}", user.Id, timeStayedStr), Constants.LEAV);
					EmbedActions.AddFooter(embed, (user.IsBot ? "Bot Left" : "User Left"));
					EmbedActions.AddAuthor(embed, user);
					await MessageActions.SendEmbedMessage(serverLog, embed);

					_Logging.IncrementLeaves();
				}

				//Goodbye message
				await MessageActions.SendGuildNotification(user, verified.GuildSettings.GoodbyeMessage);
			}
			public async Task OnUserUpdated(SocketUser beforeUser, SocketUser afterUser)
			{
				if (beforeUser.Username == null || afterUser.Username == null || _BotSettings.Pause)
					return;

				//Name change
				if (!beforeUser.Username.CaseInsEquals(afterUser.Username))
				{
					foreach (var guild in await _Client.GetGuildsAsync())
					{
						if (!(await guild.GetUsersAsync()).Select(x => x.Id).Contains(afterUser.Id))
							return;

						if (OtherLogActions.VerifyServerLoggingAction(_BotSettings, _GuildSettings, guild, LogAction.UserLeft, out VerifiedLoggingAction verified))
						{
							var guildSettings = verified.GuildSettings;
							var serverLog = verified.LoggingChannel;

							var embed = EmbedActions.MakeNewEmbed(null, null, Constants.UEDT);
							EmbedActions.AddFooter(embed, "Name Changed");
							EmbedActions.AddField(embed, "Before:", "`" + beforeUser.Username + "`");
							EmbedActions.AddField(embed, "After:", "`" + afterUser.Username + "`", false);
							EmbedActions.AddAuthor(embed, afterUser);
							await MessageActions.SendEmbedMessage(serverLog, embed);

							_Logging.IncrementUserChanges();
						}
					}
				}
			}
			public async Task OnMessageReceived(SocketMessage message)
			{
				var guild = message.GetGuild() as SocketGuild;
				if (_GuildSettings.TryGetSettings(guild, out IGuildSettings guildSettings))
				{
					await OnMessageReceivedActions.HandleCloseWords(_BotSettings, guildSettings, message, _Timers);
					await OnMessageReceivedActions.HandleSpamPreventionVoting(guildSettings, guild, message, _Timers);

					if (OtherLogActions.VerifyMessageShouldBeLogged(guildSettings, message))
					{
						await OnMessageReceivedActions.HandleChannelSettings(guildSettings, message);
						await OnMessageReceivedActions.HandleSpamPrevention(guildSettings, guild, message, _Timers);
						await OnMessageReceivedActions.HandleSlowmodeOrBannedPhrases(guildSettings, guild, message, _Timers);
						await OnMessageReceivedActions.HandleImageLogging(_Logging, guildSettings, message);
					}
				}
			}
			public async Task OnMessageUpdated(Cacheable<IMessage, ulong> cached, SocketMessage afterMessage, ISocketMessageChannel channel)
			{
				if (!OtherLogActions.VerifyServerLoggingAction(_BotSettings, _GuildSettings, channel, LogAction.MessageUpdated, out VerifiedLoggingAction verified)
					|| !OtherLogActions.VerifyMessageShouldBeLogged(verified.GuildSettings, afterMessage))
				{
					return;
				}

				var guild = verified.Guild;
				var guildSettings = verified.GuildSettings;
				var serverLog = verified.LoggingChannel;

				var beforeMessage = cached.HasValue ? cached.Value : null;

				await SpamActions.HandleBannedPhrases(_Timers, guildSettings, guild, afterMessage);

				var imageLog = guildSettings.ImageLog;
				if (imageLog != null && beforeMessage?.Embeds.Count() < afterMessage.Embeds.Count())
				{
					//If the before message is not specified always take that as it should be logged. If the embed counts are greater take that as logging too.
					await OnMessageReceivedActions.HandleImageLogging(_Logging, guildSettings, afterMessage);
				}

				if (serverLog != null)
				{
					var beforeMsgContent = String.IsNullOrWhiteSpace(beforeMessage?.Content) ? "Empty or unable to be gotten." : FormattingActions.RemoveMarkdownChars(beforeMessage?.Content, true);
					var afterMsgContent = String.IsNullOrWhiteSpace(afterMessage.Content) ? "Empty or unable to be gotten." : FormattingActions.RemoveMarkdownChars(afterMessage.Content, true);

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
					EmbedActions.AddField(embed, "Before:", String.Format("`{0}`", beforeMsgContent));
					EmbedActions.AddField(embed, "After:", String.Format("`{0}`", afterMsgContent), false);
					EmbedActions.AddAuthor(embed, afterMessage.Author);
					await MessageActions.SendEmbedMessage(serverLog, embed);

					_Logging.IncrementEdits();
				}
			}
			public async Task OnMessageDeleted(Cacheable<IMessage, ulong> cached, ISocketMessageChannel channel)
			{
				if (OtherLogActions.VerifyServerLoggingAction(_BotSettings, _GuildSettings, channel, LogAction.MessageDeleted, out VerifiedLoggingAction verified))
				{
					var guild = verified.Guild;
					var guildSettings = verified.GuildSettings;
					var serverLog = verified.LoggingChannel;

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

					_Logging.IncrementDeletes();

					//TODO: make sure this is working correctly
					await Task.Run(async () =>
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
						await MessageActions.SendMessageContainingFormattedDeletedMessages(guild, serverLog, formattedMessages);
					});
				}
			}

			//Mod
			public async Task LogCommand(IMyCommandContext context)
			{
				var loggedCommand = new LoggedCommand(context);
				_Logging.RanCommands.Add(loggedCommand);

				ConsoleActions.WriteLine(loggedCommand.ToString());
				await MessageActions.DeleteMessage(context.Message);

				if (OtherLogActions.VerifyMessageShouldBeLogged(context.GuildSettings, context.Message))
				{
					var modLog = context.GuildSettings.ModLog;
					if (modLog == null)
						return;

					var embed = EmbedActions.MakeNewEmbed(null, context.Message.Content);
					EmbedActions.AddFooter(embed, "Mod Log");
					EmbedActions.AddAuthor(embed, context.User);
					await MessageActions.SendEmbedMessage(modLog, embed);
				}
			}
		}

		public static class OnMessageReceivedActions
		{
			public static async Task HandleChannelSettings(IGuildSettings guildSettings, IMessage message)
			{
				var channel = message.Channel as ITextChannel;
				var author = message.Author as IGuildUser;
				if (channel == null || author == null || author.GuildPermissions.Administrator)
					return;

				if (guildSettings.ImageOnlyChannels.Contains(channel.Id) && !(message.Attachments.Any(x => x.Height != null || x.Width != null) || message.Embeds.Any(x => x.Image != null)))
				{
					await message.DeleteAsync();
				}
				if (guildSettings.SanitaryChannels.Contains(channel.Id))
				{
					await message.DeleteAsync();
				}
			}
			public static async Task HandleImageLogging(ILogModule logging, IGuildSettings guildSettings, IMessage message)
			{
				var logChannel = guildSettings.ImageLog;
				if (logChannel == null || message.Author.Id == Properties.Settings.Default.BotID)
					return;

				if (message.Attachments.Any())
				{
					await OtherLogActions.LogImage(logging, logChannel, message, false);
				}
				if (message.Embeds.Any())
				{
					await OtherLogActions.LogImage(logging, logChannel, message, true);
				}
			}
			public static async Task HandleCloseWords(IBotSettings botSettings, IGuildSettings guildSettings, IMessage message, ITimersModule timers = null)
			{
				if (timers != null && int.TryParse(message.Content, out int number) && number > 0 && number < 6)
				{
					--number;
					var closeWordList = timers.GetOutActiveCloseQuote(message.Author.Id);
					if (!closeWordList.Equals(default(ActiveCloseWord<Quote>)) && closeWordList.List.Count > number)
					{
						await MessageActions.SendChannelMessage(message.Channel, closeWordList.List[number].Word.Text);
						await MessageActions.DeleteMessage(message);
					}
					var closeHelpList = timers.GetOutActiveCloseHelp(message.Author.Id);
					if (!closeHelpList.Equals(default(ActiveCloseWord<HelpEntry>)) && closeHelpList.List.Count > number)
					{
						var help = closeHelpList.List[number].Word;
						var embed = EmbedActions.MakeNewEmbed(help.Name, help.ToString(), prefix: GetActions.GetPrefix(botSettings, guildSettings));
						EmbedActions.AddFooter(embed, "Help");
						await MessageActions.SendEmbedMessage(message.Channel, embed);
						await MessageActions.DeleteMessage(message);
					}
				}
			}
			public static async Task HandleSlowmodeOrBannedPhrases(IGuildSettings guildSettings, SocketGuild guild, IMessage message, ITimersModule timers = null)
			{
				await SpamActions.HandleSlowmode(guildSettings, message);
				await SpamActions.HandleBannedPhrases(timers, guildSettings, guild, message);
			}
			public static async Task HandleSpamPrevention(IGuildSettings guildSettings, SocketGuild guild, IMessage message, ITimersModule timers  = null)
			{
				if (UserActions.GetIfUserCanBeModifiedByUser(UserActions.GetBot(guild), message.Author))
				{
					await SpamActions.HandleSpamPrevention(guildSettings, guild, message.Author as IGuildUser, message, timers);
				}
			}
			public static async Task HandleSpamPreventionVoting(IGuildSettings guildSettings, SocketGuild guild, IMessage message, ITimersModule timers = null)
			{
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

		public static class OtherLogActions
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
						var desc = String.Format("**Channel:** `{0}`\n**Message ID:** `{1}`", message.Channel.FormatChannel(), message.Id);
						var embed = EmbedActions.MakeNewEmbed(null, desc, Constants.ATCH, attachmentURL);
						EmbedActions.AddFooter(embed, "Attached Image");
						EmbedActions.AddAuthor(embed, message.Author, attachmentURL);
						await MessageActions.SendEmbedMessage(channel, embed);

						currentLogModule.IncrementImages();
					}
					//Gif attachment
					else if (Constants.VALID_GIF_EXTENTIONS.CaseInsContains(Path.GetExtension(attachmentURL)))
					{
						var desc = String.Format("**Channel:** `{0}`\n**Message ID:** `{1}`", message.Channel.FormatChannel(), message.Id);
						var embed = EmbedActions.MakeNewEmbed(null, desc, Constants.ATCH, attachmentURL);
						EmbedActions.AddFooter(embed, "Attached Gif");
						EmbedActions.AddAuthor(embed, message.Author, attachmentURL);
						await MessageActions.SendEmbedMessage(channel, embed);

						currentLogModule.IncrementGifs();
					}
					//Random file attachment
					else
					{
						var desc = String.Format("**Channel:** `{0}`\n**Message ID:** `{1}`", message.Channel.FormatChannel(), message.Id);
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
					var desc = String.Format("**Channel:** `{0}`\n**Message ID:** `{1}`", message.Channel.FormatChannel(), message.Id);
					var embed = EmbedActions.MakeNewEmbed(null, desc, Constants.ATCH, embedURL);
					EmbedActions.AddFooter(embed, "Embedded Image");
					EmbedActions.AddAuthor(embed, message.Author, embedURL);
					await MessageActions.SendEmbedMessage(channel, embed);

					currentLogModule.IncrementImages();
				}
				//Embedded videos/gifs
				foreach (var videoEmbed in videoEmbeds.GroupBy(x => x.Url).Select(x => x.First()))
				{
					var desc = String.Format("**Channel:** `{0}`\n**Message ID:** `{1}`", message.Channel.FormatChannel(), message.Id);
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
							await MessageActions.SendEmbedMessage(guildSettings.ServerLog, EmbedActions.MakeNewEmbed("Anti Rapid Join Mute", String.Format("**User:** {0}", user.FormatUser())));
						}
					}
				}
			}

			public static bool VerifyLoggingIsEnabledOnThisChannel(IGuildSettings guildSettings, IMessage message)
			{
				return !guildSettings.IgnoredLogChannels.Contains(message.Channel.Id);
			}
			public static bool VerifyMessageShouldBeLogged(IGuildSettings guildSettings, IMessage message)
			{
				//Ignore null messages, webhook messages, bot messages, and commands on channels that shouldn't be logged
				return !(message == null || message.Author.IsWebhook || (message.Author.IsBot && message.Author.Id != Properties.Settings.Default.BotID) || !VerifyLoggingIsEnabledOnThisChannel(guildSettings, message));
			}
			public static bool VerifyServerLoggingAction(IBotSettings botSettings, IGuildSettingsModule guildSettingsModule, IGuildUser user, LogAction logAction, out VerifiedLoggingAction verifLoggingAction)
			{
				return VerifyServerLoggingAction(botSettings, guildSettingsModule, user.Guild, logAction, out verifLoggingAction);
			}
			public static bool VerifyServerLoggingAction(IBotSettings botSettings, IGuildSettingsModule guildSettingsModule, ISocketMessageChannel channel, LogAction logAction, out VerifiedLoggingAction verifLoggingAction)
			{
				if (!VerifyServerLoggingAction(botSettings, guildSettingsModule, channel.GetGuild() as SocketGuild, logAction, out verifLoggingAction))
				{
					return false;
				}
				return !verifLoggingAction.Equals(default(VerifiedLoggingAction)) && !verifLoggingAction.GuildSettings.IgnoredLogChannels.Contains(channel.Id);
			}
			public static bool VerifyServerLoggingAction(IBotSettings botSettings, IGuildSettingsModule guildSettingsModule, IGuild guild, LogAction logAction, out VerifiedLoggingAction verifLoggingAction)
			{
				verifLoggingAction = new VerifiedLoggingAction(null, null, null);
				if (botSettings.Pause || !guildSettingsModule.TryGetSettings(guild, out IGuildSettings guildSettings))
				{
					return false;
				}

				if (guildSettings.ServerLog == null || !guildSettings.LogActions.Contains(logAction))
				{
					return false;
				}

				verifLoggingAction = new VerifiedLoggingAction(guild, guildSettings, guildSettings.ServerLog);
				return true;
			}
		}
	}
}