using Advobot.Actions;
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

				client.Log += Log.Log;
				client.GuildAvailable += Log.OnGuildAvailable;
				client.GuildUnavailable += Log.OnGuildUnavailable;
				client.JoinedGuild += Log.OnJoinedGuild;
				client.LeftGuild += Log.OnLeftGuild;
				client.UserJoined += Log.OnUserJoined;
				client.UserLeft += Log.OnUserLeft;
				client.UserUpdated += Log.OnUserUpdated;
				client.MessageReceived += Log.OnMessageReceived;
				client.MessageUpdated += Log.OnMessageUpdated;
				client.MessageDeleted += Log.OnMessageDeleted;
			}
			private void CreateLogHolder(DiscordShardedClient client, IBotSettings botSettings, IGuildSettingsModule guildSettings, ITimersModule timers)
			{
				Log = new MyLog(client, botSettings, guildSettings, this, timers);

				client.Log += Log.Log;
				client.GuildAvailable += Log.OnGuildAvailable;
				client.GuildUnavailable += Log.OnGuildUnavailable;
				client.JoinedGuild += Log.OnJoinedGuild;
				client.LeftGuild += Log.OnLeftGuild;
				client.UserJoined += Log.OnUserJoined;
				client.UserLeft += Log.OnUserLeft;
				client.UserUpdated += Log.OnUserUpdated;
				client.MessageReceived += Log.OnMessageReceived;
				client.MessageUpdated += Log.OnMessageUpdated;
				client.MessageDeleted += Log.OnMessageDeleted;
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

				var attempted = Formatting.FormatStringsWithLength(aStr, a, rightSpacing, leftSpacing);
				var successful = Formatting.FormatStringsWithLength(sStr, s, rightSpacing, leftSpacing);
				var failed = Formatting.FormatStringsWithLength(fStr, f, rightSpacing, leftSpacing);
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

				var joins = Formatting.FormatStringsWithLength(jTitle, j, rightSpacing, leftSpacing);
				var leaves = Formatting.FormatStringsWithLength(lTitle, l, rightSpacing, leftSpacing);
				var userChanges = Formatting.FormatStringsWithLength(uTitle, u, rightSpacing, leftSpacing);
				var edits = Formatting.FormatStringsWithLength(eTitle, e, rightSpacing, leftSpacing);
				var deletes = Formatting.FormatStringsWithLength(dTitle, d, rightSpacing, leftSpacing);
				var images = Formatting.FormatStringsWithLength(iTitle, i, rightSpacing, leftSpacing);
				var gifs = Formatting.FormatStringsWithLength(gTitle, g, rightSpacing, leftSpacing);
				var files = Formatting.FormatStringsWithLength(fTitle, f, rightSpacing, leftSpacing);
				return String.Join("\n", new[] { joins, leaves, userChanges, edits, deletes, images, gifs, files });
			}
		}

		public sealed class MyLog : ILog
		{
			private IDiscordClient Client { get; }
			private IBotSettings BotSettings { get; }
			private IGuildSettingsModule GuildSettings { get; }
			private ILogModule Logging { get; }
			private ITimersModule Timers { get; }

			public MyLog(IDiscordClient client, IBotSettings botSettings, IGuildSettingsModule guildSettings, ILogModule logging, ITimersModule timers)
			{
				Client = client;
				BotSettings = botSettings;
				GuildSettings = guildSettings;
				Logging = logging;
				Timers = timers;
			}

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
				ConsoleActions.WriteLine(String.Format("{0} is now online on shard {1}.", guild.FormatGuild(), ClientActions.GetShardIdFor(Client, guild)));
				ConsoleActions.WriteLine(String.Format("Current memory usage is: {0}MB.", Gets.GetMemory(BotSettings.Windows).ToString("0.00")));
				Logging.AddUsers(guild.MemberCount);
				Logging.IncrementGuilds();

				await GuildSettings.AddGuild(guild);
			}
			public Task OnGuildUnavailable(SocketGuild guild)
			{
				ConsoleActions.WriteLine(String.Format("Guild is now offline {0}.", guild.FormatGuild()));
				Logging.RemoveUsers(guild.MemberCount);
				Logging.DecrementGuilds();

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
				var guilds = (await Client.GetGuildsAsync()).Count;
				var shards = ClientActions.GetShardCount(Client);
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

				Logging.RemoveUsers(guild.MemberCount);
				Logging.DecrementGuilds();

				return Task.CompletedTask;
			}

			public async Task OnUserJoined(SocketGuildUser user)
			{
				Logging.IncrementUsers();

				if (OtherLogActions.VerifyServerLoggingAction(BotSettings, GuildSettings, user, LogAction.UserJoined, out VerifiedLoggingAction verified))
				{
					var guild = verified.Guild;
					var guildSettings = verified.GuildSettings;
					var serverLog = verified.LoggingChannel;

					if (guildSettings != null)
					{
						await OtherLogActions.HandleJoiningUsers(Timers, guildSettings, user);
					}

					var curInv = await Invites.GetInviteUserJoinedOn(guildSettings, guild);
					var inviteStr = curInv != null ? String.Format("\n**Invite:** {0}", curInv.Code) : "";
					var userAccAge = (DateTime.UtcNow - user.CreatedAt.ToUniversalTime());
					var ageWarningStr = userAccAge.TotalHours <= 24 ? String.Format("\n**New Account:** {0} hours, {1} minutes old.", (int)userAccAge.TotalHours, (int)userAccAge.Minutes) : "";
					var botOrUserStr = user.IsBot ? "Bot" : "User";

					//Bans people who join with a given word in their name
					if (guildSettings.BannedNamesForJoiningUsers.Any(x => user.Username.CaseInsContains(x.Phrase)))
					{
						await Punishments.AutomaticBan(guild, user.Id, "banned name");
						return;
					}
					//Welcome message
					else
					{
						await Messages.SendGuildNotification(user, guildSettings.WelcomeMessage);
					}

					{
						var embed = Embeds.MakeNewEmbed(null, String.Format("**ID:** {0}{1}{2}", user.Id, inviteStr, ageWarningStr), Constants.JOIN);
						Embeds.AddFooter(embed, String.Format("{0} Joined", botOrUserStr));
						Embeds.AddAuthor(embed, user);
						await Messages.SendEmbedMessage(serverLog, embed);
					}

					Logging.IncrementJoins();
				}
				else
				{
					var guildSettings = verified.GuildSettings;
					if (guildSettings == null)
						return;

					await OtherLogActions.HandleJoiningUsers(Timers, guildSettings, user);
				}
			}
			public async Task OnUserLeft(SocketGuildUser user)
			{
				Logging.DecrementUsers();

				//Check if the bot was the one that left
				if (user.Id == Properties.Settings.Default.BotID)
				{
					await GuildSettings.RemoveGuild(user.Guild);
					return;
				}

				if (OtherLogActions.VerifyServerLoggingAction(BotSettings, GuildSettings, user, LogAction.UserLeft, out VerifiedLoggingAction verified))
				{
					var guild = verified.Guild;
					var guildSettings = verified.GuildSettings;
					var serverLog = verified.LoggingChannel;

					//Don't log them to the server if they're someone who was just banned for joining with a banned name
					if (guildSettings.BannedNamesForJoiningUsers.Any(x => user.Username.CaseInsContains(x.Phrase)))
						return;

					await Messages.SendGuildNotification(user, guildSettings.GoodbyeMessage);

					var lengthStayed = "";
					if (user.JoinedAt.HasValue)
					{
						var time = DateTime.UtcNow.Subtract(user.JoinedAt.Value.UtcDateTime);
						lengthStayed = String.Format("\n**Stayed for:** {0}:{1:00}:{2:00}:{3:00}", time.Days, time.Hours, time.Minutes, time.Seconds);
					}
					var botOrUserStr = user.IsBot ? "Bot" : "User";

					var embed = Embeds.MakeNewEmbed(null, String.Format("**ID:** {0}{1}", user.Id, lengthStayed), Constants.LEAV);
					Embeds.AddFooter(embed, String.Format("{0} Left", botOrUserStr));
					Embeds.AddAuthor(embed, user);
					await Messages.SendEmbedMessage(serverLog, embed);

					Logging.IncrementLeaves();
				}
			}
			public async Task OnUserUpdated(SocketUser beforeUser, SocketUser afterUser)
			{
				if (beforeUser.Username == null || afterUser.Username == null || BotSettings.Pause)
					return;

				//Name change
				if (!beforeUser.Username.CaseInsEquals(afterUser.Username))
				{
					foreach (var guild in await Client.GetGuildsAsync())
					{
						if (!(await guild.GetUsersAsync()).Select(x => x.Id).Contains(afterUser.Id))
							return;

						if (OtherLogActions.VerifyServerLoggingAction(BotSettings, GuildSettings, guild, LogAction.UserLeft, out VerifiedLoggingAction verified))
						{
							var guildSettings = verified.GuildSettings;
							var serverLog = verified.LoggingChannel;

							var embed = Embeds.MakeNewEmbed(null, null, Constants.UEDT);
							Embeds.AddFooter(embed, "Name Changed");
							Embeds.AddField(embed, "Before:", "`" + beforeUser.Username + "`");
							Embeds.AddField(embed, "After:", "`" + afterUser.Username + "`", false);
							Embeds.AddAuthor(embed, afterUser);
							await Messages.SendEmbedMessage(serverLog, embed);

							Logging.IncrementUserChanges();
						}
					}
				}
			}
			public async Task OnMessageReceived(SocketMessage message)
			{
				var guild = message.GetGuild() as SocketGuild;
				if (guild == null)
				{
					//Check if the user is trying to become the bot owner by DMing the bot its key
					await OnMessageReceivedActions.HandlePotentialBotOwner(BotSettings, message);
					return;
				}

				if (GuildSettings.TryGetSettings(guild, out IGuildSettings guildSettings))
				{
					await OnMessageReceivedActions.HandleCloseWords(Timers, guildSettings, message);
					await OnMessageReceivedActions.HandleSpamPreventionVoting(Timers, guildSettings, guild, message);

					if (OtherLogActions.VerifyMessageShouldBeLogged(guildSettings, message))
					{
						await OnMessageReceivedActions.HandleChannelSettings(guildSettings, message);
						await OnMessageReceivedActions.HandleSpamPrevention(Timers, guildSettings, guild, message);
						await OnMessageReceivedActions.HandleSlowmodeOrBannedPhrases(Timers, guildSettings, guild, message);
						await OnMessageReceivedActions.HandleImageLogging(Logging, guildSettings, message);
					}
				}
			}
			public async Task OnMessageUpdated(Cacheable<IMessage, ulong> cached, SocketMessage afterMessage, ISocketMessageChannel channel)
			{
				if (OtherLogActions.VerifyServerLoggingAction(BotSettings, GuildSettings, channel, LogAction.MessageUpdated, out VerifiedLoggingAction verified))
				{
					var guild = verified.Guild;
					var guildSettings = verified.GuildSettings;
					var serverLog = verified.LoggingChannel;

					var beforeMessage = cached.HasValue ? cached.Value : null;
					if (!OtherLogActions.VerifyMessageShouldBeLogged(guildSettings, afterMessage))
						return;

					await Spam.HandleBannedPhrases(Timers, guildSettings, guild, afterMessage);

					if (serverLog != null)
					{
						var beforeMsgContent = Formatting.RemoveMarkdownChars(beforeMessage?.Content ?? "", true);
						var afterMsgContent = Formatting.RemoveMarkdownChars(afterMessage.Content, true);
						beforeMsgContent = String.IsNullOrWhiteSpace(beforeMsgContent) ? "Empty or unable to be gotten." : beforeMsgContent;
						afterMsgContent = String.IsNullOrWhiteSpace(afterMsgContent) ? "Empty or unable to be gotten." : afterMsgContent;

						if (beforeMsgContent.Equals(afterMsgContent))
						{
							return;
						}
						else if (beforeMsgContent.Length + afterMsgContent.Length > Constants.MAX_MESSAGE_LENGTH_LONG)
						{
							beforeMsgContent = beforeMsgContent.Length > 667 ? "LONG MESSAGE" : beforeMsgContent;
							afterMsgContent = afterMsgContent.Length > 667 ? "LONG MESSAGE" : afterMsgContent;
						}

						var embed = Embeds.MakeNewEmbed(null, null, Constants.MEDT);
						Embeds.AddFooter(embed, "Message Updated");
						Embeds.AddField(embed, "Before:", String.Format("`{0}`", beforeMsgContent));
						Embeds.AddField(embed, "After:", String.Format("`{0}`", afterMsgContent), false);
						Embeds.AddAuthor(embed, afterMessage.Author);
						await Messages.SendEmbedMessage(serverLog, embed);

						Logging.IncrementEdits();
					}
					var imageLog = guildSettings.ImageLog;
					if (imageLog != null)
					{
						//If the before message is not specified always take that as it should be logged. If the embed counts are greater take that as logging too.
						if (beforeMessage?.Embeds.Count() < afterMessage.Embeds.Count())
						{
							await OnMessageReceivedActions.HandleImageLogging(Logging, guildSettings, afterMessage);
						}
					}
				}
				else
				{
					var guild = verified.Guild;
					if (guild == null)
						return;
					var guildSettings = verified.GuildSettings;
					if (guildSettings == null)
						return;

					var beforeMessage = cached.HasValue ? cached.Value : null;
					if (OtherLogActions.VerifyMessageShouldBeLogged(guildSettings, afterMessage))
					{
						await Spam.HandleBannedPhrases(Timers, guildSettings, guild, afterMessage);
					}
				}
			}
			public Task OnMessageDeleted(Cacheable<IMessage, ulong> cached, ISocketMessageChannel channel)
			{
				if (OtherLogActions.VerifyServerLoggingAction(BotSettings, GuildSettings, channel, LogAction.MessageDeleted, out VerifiedLoggingAction verified))
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
					cancelToken = new CancellationTokenSource();
					msgDeletion.SetCancelToken(cancelToken);

					Logging.IncrementDeletes();

					//Make async so doesn't publish prematurely
					Task.Run(async () =>
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
							deletedMessages = new List<IMessage>(msgDeletion.GetList());
							msgDeletion.ClearList();
						}

						//Put the message content into a list of strings for easy usage
						var formattedMessages = Formatting.FormatMessages(deletedMessages.OrderBy(x => x?.CreatedAt.Ticks));
						await Messages.SendMessageContainingFormattedDeletedMessages(guild, serverLog, formattedMessages);
					});
				}

				return Task.FromResult(0);
			}

			public async Task LogCommand(IMyCommandContext context)
			{
				ConsoleActions.WriteLine(new LoggedCommand(context).ToString());
				await Messages.DeleteMessage(context.Message);

				if (OtherLogActions.VerifyMessageShouldBeLogged(context.GuildSettings, context.Message))
				{
					var modLog = context.GuildSettings.ModLog;
					if (modLog == null)
						return;

					var embed = Embeds.MakeNewEmbed(null, context.Message.Content);
					Embeds.AddFooter(embed, "Mod Log");
					Embeds.AddAuthor(embed, context.User);
					await Messages.SendEmbedMessage(modLog, embed);
				}
			}
		}

		public static class OnMessageReceivedActions
		{
			public static async Task HandlePotentialBotOwner(IBotSettings botSettings, IMessage message)
			{
				if (message.Content.Equals(Properties.Settings.Default.BotKey) && botSettings.BotOwnerID == 0)
				{
					botSettings.BotOwnerID = message.Author.Id;
					await Messages.SendDMMessage(message.Channel as IDMChannel, "Congratulations, you are now the owner of the bot.");
				}
			}
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
			public static async Task HandleCloseWords(ITimersModule timers, IGuildSettings guildSettings, IMessage message)
			{
				if (int.TryParse(message.Content, out int number) && number > 0 && number < 6)
				{
					--number;
					var closeWordList = timers.ActiveCloseQuotes.FirstOrDefault(x => x.UserID == message.Author.Id);
					if (!closeWordList.Equals(default(ActiveCloseWord<Quote>)) && closeWordList.List.Count > number)
					{
						var quote = closeWordList.List[number].Word;
						timers.ActiveCloseQuotes.ThreadSafeRemove(closeWordList);

						await Messages.SendChannelMessage(message.Channel, quote.Text);
						await Messages.DeleteMessage(message);
					}
					var closeHelpList = timers.ActiveCloseHelp.FirstOrDefault(x => x.UserID == message.Author.Id);
					if (!closeHelpList.Equals(default(ActiveCloseWord<HelpEntry>)) && closeHelpList.List.Count > number)
					{
						var help = closeHelpList.List[number].Word;
						timers.ActiveCloseHelp.ThreadSafeRemove(closeHelpList);

						var embed = Embeds.MakeNewEmbed(help.Name, help.ToString());
						Embeds.AddFooter(embed, "Help");
						await Messages.SendEmbedMessage(message.Channel, embed);
						await Messages.DeleteMessage(message);
					}
				}
			}
			public static async Task HandleSlowmodeOrBannedPhrases(ITimersModule timers, IGuildSettings guildSettings, SocketGuild guild, IMessage message)
			{
				await Spam.HandleSlowmode(guildSettings, message);
				await Spam.HandleBannedPhrases(timers, guildSettings, guild, message);
			}
			public static async Task HandleSpamPrevention(ITimersModule timers, IGuildSettings guildSettings, SocketGuild guild, IMessage message)
			{
				if (Users.GetIfUserCanBeModifiedByUser(Users.GetBot(guild), message.Author))
				{
					await Spam.HandleSpamPrevention(timers, guildSettings, guild, message.Author as IGuildUser, message);
				}
			}
			public static async Task HandleSpamPreventionVoting(ITimersModule timers, IGuildSettings guildSettings, SocketGuild guild, IMessage message)
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

					await user.SpamPreventionPunishment(timers, guildSettings);

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
						var embed = Embeds.MakeNewEmbed(null, desc, Constants.ATCH, attachmentURL);
						Embeds.AddFooter(embed, "Attached Image");
						Embeds.AddAuthor(embed, message.Author, attachmentURL);
						await Messages.SendEmbedMessage(channel, embed);

						currentLogModule.IncrementImages();
					}
					//Gif attachment
					else if (Constants.VALID_GIF_EXTENTIONS.CaseInsContains(Path.GetExtension(attachmentURL)))
					{
						var desc = String.Format("**Channel:** `{0}`\n**Message ID:** `{1}`", message.Channel.FormatChannel(), message.Id);
						var embed = Embeds.MakeNewEmbed(null, desc, Constants.ATCH, attachmentURL);
						Embeds.AddFooter(embed, "Attached Gif");
						Embeds.AddAuthor(embed, message.Author, attachmentURL);
						await Messages.SendEmbedMessage(channel, embed);

						currentLogModule.IncrementGifs();
					}
					//Random file attachment
					else
					{
						var desc = String.Format("**Channel:** `{0}`\n**Message ID:** `{1}`", message.Channel.FormatChannel(), message.Id);
						var embed = Embeds.MakeNewEmbed(null, desc, Constants.ATCH, attachmentURL);
						Embeds.AddFooter(embed, "Attached File");
						Embeds.AddAuthor(embed, message.Author, attachmentURL);
						await Messages.SendEmbedMessage(channel, embed);

						currentLogModule.IncrementFiles();
					}
				}
				//Embedded images
				foreach (var embedURL in embedURLs.Distinct())
				{
					var desc = String.Format("**Channel:** `{0}`\n**Message ID:** `{1}`", message.Channel.FormatChannel(), message.Id);
					var embed = Embeds.MakeNewEmbed(null, desc, Constants.ATCH, embedURL);
					Embeds.AddFooter(embed, "Embedded Image");
					Embeds.AddAuthor(embed, message.Author, embedURL);
					await Messages.SendEmbedMessage(channel, embed);

					currentLogModule.IncrementImages();
				}
				//Embedded videos/gifs
				foreach (var videoEmbed in videoEmbeds.GroupBy(x => x.Url).Select(x => x.First()))
				{
					var desc = String.Format("**Channel:** `{0}`\n**Message ID:** `{1}`", message.Channel.FormatChannel(), message.Id);
					var embed = Embeds.MakeNewEmbed(null, desc, Constants.ATCH, videoEmbed.Thumbnail?.Url);
					Embeds.AddFooter(embed, "Embedded " + (Constants.VALID_GIF_EXTENTIONS.CaseInsContains(Path.GetExtension(videoEmbed.Thumbnail?.Url)) ? "Gif" : "Video"));
					Embeds.AddAuthor(embed, message.Author, videoEmbed.Url);
					await Messages.SendEmbedMessage(channel, embed);

					currentLogModule.IncrementGifs();
				}
			}
			public static async Task HandleJoiningUsers(ITimersModule timers, IGuildSettings guildSettings, IGuildUser user)
			{
				//Slowmode
				{
					var smGuild = guildSettings.SlowmodeGuild;
					if (smGuild != null)
					{
						smGuild.Users.ThreadSafeAdd(new SlowmodeUser(user, smGuild.BaseMessages, smGuild.Interval));
					}
					var smChannels = guildSettings.SlowmodeChannels;
					if (smChannels.Any())
					{
						smChannels.Where(x => (user.Guild as SocketGuild).TextChannels.Select(y => y.Id).Contains(x.ChannelID)).ToList().ForEach(smChan =>
						{
							smChan.Users.ThreadSafeAdd(new SlowmodeUser(user, smChan.BaseMessages, smChan.Interval));
						});
					}
				}

				//Raid Prevention
				{
					var antiRaid = guildSettings.RaidPreventionDictionary[RaidType.Regular];
					if (antiRaid != null && antiRaid.Enabled)
					{
						await antiRaid.RaidPreventionPunishment(timers, guildSettings, user);
					}
					var antiJoin = guildSettings.RaidPreventionDictionary[RaidType.RapidJoins];
					if (antiJoin != null && antiJoin.Enabled)
					{
						antiJoin.Add(user.JoinedAt.Value.UtcDateTime);
						if (antiJoin.GetSpamCount() >= antiJoin.RequiredCount)
						{
							await antiJoin.RaidPreventionPunishment(timers, guildSettings, user);
							if (guildSettings.ServerLog != null)
							{
								await Messages.SendEmbedMessage(guildSettings.ServerLog, Embeds.MakeNewEmbed("Anti Rapid Join Mute", String.Format("**User:** {0}", user.FormatUser())));
							}
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
				//Ignore null messages
				if (message == null)
					return false;
				//Ignore webhook messages
				else if (message.Author.IsWebhook)
					return false;
				//Ignore bot messgaes
				else if (message.Author.IsBot && message.Author.Id != Properties.Settings.Default.BotID)
					return false;
				//Ignore commands on channels that shouldn't be logged
				else if (!VerifyLoggingIsEnabledOnThisChannel(guildSettings, message))
					return false;
				return true;
			}
			public static bool VerifyServerLoggingAction(IBotSettings botSettings, IGuildSettingsModule guildSettingsModule, IGuildUser user, LogAction logAction, out VerifiedLoggingAction verifLoggingAction)
			{
				return VerifyServerLoggingAction(botSettings, guildSettingsModule, user.Guild, logAction, out verifLoggingAction);
			}
			public static bool VerifyServerLoggingAction(IBotSettings botSettings, IGuildSettingsModule guildSettingsModule, ISocketMessageChannel channel, LogAction logAction, out VerifiedLoggingAction verifLoggingAction)
			{
				return VerifyServerLoggingAction(botSettings, guildSettingsModule, channel.GetGuild() as SocketGuild, logAction, out verifLoggingAction) && !verifLoggingAction.GuildSettings.IgnoredLogChannels.Contains(channel.Id);
			}
			public static bool VerifyServerLoggingAction(IBotSettings botSettings, IGuildSettingsModule guildSettingsModule, IGuild guild, LogAction logAction, out VerifiedLoggingAction verifLoggingAction)
			{
				verifLoggingAction = new VerifiedLoggingAction(null, null, null);
				if (botSettings.Pause || !guildSettingsModule.TryGetSettings(guild, out IGuildSettings guildSettings))
					return false;

				var serverLog = guildSettings.ServerLog;
				verifLoggingAction = new VerifiedLoggingAction(guild, guildSettings, serverLog);
				return serverLog != null && guildSettings.LogActions.Contains(logAction);
			}
		}
	}
}