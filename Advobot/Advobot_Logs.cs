using Advobot.Actions;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Advobot
{
	namespace Logging
	{
		public sealed class LogModule : MyModuleBase, ILogModule
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

			public BaseLog BotLog { get; private set; }
			public BaseLog ServerLog { get; private set; }
			public BaseLog ModLog { get; private set; }

			public LogModule(IDiscordClient client, IGlobalSettings botInfo, IGuildSettingsModule guildSettingsModule)
			{
				if (client is DiscordSocketClient)
				{
					CreateLogHolder(client as DiscordSocketClient, botInfo, guildSettingsModule);
				}
				else if (client is DiscordShardedClient)
				{
					CreateLogHolder(client as DiscordShardedClient, botInfo, guildSettingsModule);
				}
				else
				{
					throw new ArgumentException("Invalid client provided. Must be either a DiscordSocketClient or a DiscordShardedClient.");
				}
			}

			private void CreateLogHolder(DiscordSocketClient client, IGlobalSettings botInfo, IGuildSettingsModule guildSettingsModule)
			{
				var tempBotLog = new BotLogger(client, botInfo, guildSettingsModule, this);
				var tempServerLog = new ServerLogger(client, botInfo, guildSettingsModule, this);
				var tempModLog = new ModLogger(client, botInfo, guildSettingsModule, this);

				client.MessageReceived += (SocketMessage message) => CommandHandler.HandleCommand(message as SocketUserMessage);
				client.Connected += CommandHandler.LoadInformation;

				client.Log += tempBotLog.Log;
				client.GuildAvailable += tempBotLog.OnGuildAvailable;
				client.GuildUnavailable += tempBotLog.OnGuildUnavailable;
				client.JoinedGuild += tempBotLog.OnJoinedGuild;
				client.LeftGuild += tempBotLog.OnLeftGuild;
				client.UserJoined += tempServerLog.OnUserJoined;
				client.UserLeft += tempServerLog.OnUserLeft;
				client.UserUpdated += tempServerLog.OnUserUpdated;
				client.MessageReceived += tempServerLog.OnMessageReceived;
				client.MessageUpdated += tempServerLog.OnMessageUpdated;
				client.MessageDeleted += tempServerLog.OnMessageDeleted;

				BotLog = tempBotLog;
				ServerLog = tempServerLog;
				ModLog = tempModLog;
			}
			private void CreateLogHolder(DiscordShardedClient client, IGlobalSettings botInfo, IGuildSettingsModule guildSettingsModule)
			{
				var tempBotLog = new BotLogger(client, botInfo, guildSettingsModule, this);
				var tempServerLog = new ServerLogger(client, botInfo, guildSettingsModule, this);
				var tempModLog = new ModLogger(client, botInfo, guildSettingsModule, this);

				client.MessageReceived += (SocketMessage message) => CommandHandler.HandleCommand(message as SocketUserMessage);
				client.Shards.FirstOrDefault().Connected += CommandHandler.LoadInformation;

				client.Log += tempBotLog.Log;
				client.GuildAvailable += tempBotLog.OnGuildAvailable;
				client.GuildUnavailable += tempBotLog.OnGuildUnavailable;
				client.JoinedGuild += tempBotLog.OnJoinedGuild;
				client.LeftGuild += tempBotLog.OnLeftGuild;
				client.UserJoined += tempServerLog.OnUserJoined;
				client.UserLeft += tempServerLog.OnUserLeft;
				client.UserUpdated += tempServerLog.OnUserUpdated;
				client.MessageReceived += tempServerLog.OnMessageReceived;
				client.MessageUpdated += tempServerLog.OnMessageUpdated;
				client.MessageDeleted += tempServerLog.OnMessageDeleted;

				BotLog = tempBotLog;
				ServerLog = tempServerLog;
				ModLog = tempModLog;
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

		public class BaseLog : MyModuleBase
		{
			protected IDiscordClient Client { get; }
			protected IGlobalSettings BotInfo { get; }
			protected IGuildSettingsModule GuildSettingsModule { get; }

			public BaseLog(IDiscordClient client, IGlobalSettings botInfo, IGuildSettingsModule guildSettingsModule)
			{
				Client = client;
				BotInfo = botInfo;
				GuildSettingsModule = guildSettingsModule;
			}
		}

		public sealed class BotLogger : BaseLog
		{
			private readonly ILogModule CurrentLogModule;

			public BotLogger(IDiscordClient client, IGlobalSettings botInfo, IGuildSettingsModule guildSettingsModule, ILogModule currentLogModule) : base(client, botInfo, guildSettingsModule)
			{
				CurrentLogModule = currentLogModule;
			}

			public Task Log(LogMessage msg)
			{
				if (!String.IsNullOrWhiteSpace(msg.Message))
				{
					Messages.WriteLine(msg.Message, msg.Source);
				}

				return Task.CompletedTask;
			}

			public async Task OnGuildAvailable(SocketGuild guild)
			{
				Messages.WriteLine(String.Format("{0} is now online on shard {1}.", guild.FormatGuild(), ClientActions.GetShardIdFor(Client, guild)));
				Messages.WriteLine(String.Format("Current memory usage is: {0}MB.", Gets.GetMemory(BotInfo.Windows).ToString("0.00")));
				CurrentLogModule.AddUsers(guild.MemberCount);
				CurrentLogModule.IncrementGuilds();

				await GuildSettingsModule.AddGuild(guild);
			}

			public Task OnGuildUnavailable(SocketGuild guild)
			{
				Messages.WriteLine(String.Format("Guild is now offline {0}.", guild.FormatGuild()));
				CurrentLogModule.RemoveUsers(guild.MemberCount);
				CurrentLogModule.DecrementGuilds();

				return Task.CompletedTask;
			}

			public async Task OnJoinedGuild(SocketGuild guild)
			{
				Messages.WriteLine(String.Format("Bot has joined {0}.", guild.FormatGuild()));

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
					Messages.WriteLine(String.Format("The bot currently has {0} out of {1} possible spots for servers filled. Please increase the shard count.", guilds, curMax));
				}
				//Leave the guild
				if (guilds > curMax)
				{
					await guild.LeaveAsync();
					Messages.WriteLine(String.Format("Left the guild {0} due to having too many guilds on the client and not enough shards.", guild.FormatGuild()));
				}

				return;
			}

			public Task OnLeftGuild(SocketGuild guild)
			{
				Messages.WriteLine(String.Format("Bot has left {0}.", guild.FormatGuild()));

				CurrentLogModule.RemoveUsers(guild.MemberCount);
				CurrentLogModule.DecrementGuilds();

				return Task.CompletedTask;
			}
		}

		public sealed class ServerLogger : BaseLog
		{
			private readonly ILogModule CurrentLogModule;

			public ServerLogger(IDiscordClient client, IGlobalSettings botInfo, IGuildSettingsModule guildSettingsModule, ILogModule currentLogModule) : base(client, botInfo, guildSettingsModule)
			{
				CurrentLogModule = currentLogModule;
			}

			public async Task OnUserJoined(SocketGuildUser user)
			{
				CurrentLogModule.IncrementUsers();

				if (LogChannels.VerifyServerLoggingAction(BotInfo, GuildSettingsModule, user, LogAction.UserJoined, out VerifiedLoggingAction verified))
				{
					var guild = verified.Guild;
					var guildInfo = verified.GuildInfo;
					var serverLog = verified.LoggingChannel;

					if (guildInfo != null)
					{
						await LogChannels.HandleJoiningUsers(guildInfo, user);
					}

					var curInv = await Invites.GetInviteUserJoinedOn(guildInfo, guild);
					var inviteStr = curInv != null ? String.Format("\n**Invite:** {0}", curInv.Code) : "";
					var userAccAge = (DateTime.UtcNow - user.CreatedAt.ToUniversalTime());
					var ageWarningStr = userAccAge.TotalHours <= 24 ? String.Format("\n**New Account:** {0} hours, {1} minutes old.", (int)userAccAge.TotalHours, (int)userAccAge.Minutes) : "";
					var botOrUserStr = user.IsBot ? "Bot" : "User";

					//Bans people who join with a given word in their name
					if (guildInfo.BannedNamesForJoiningUsers.Any(x => user.Username.CaseInsContains(x.Phrase)))
					{
						await Punishments.AutomaticBan(guild, user.Id, "banned name");
						return;
					}
					//Welcome message
					else
					{
						await Messages.SendGuildNotification(user, guildInfo.WelcomeMessage);
					}

					{
						var embed = Embeds.MakeNewEmbed(null, String.Format("**ID:** {0}{1}{2}", user.Id, inviteStr, ageWarningStr), Constants.JOIN);
						Embeds.AddFooter(embed, String.Format("{0} Joined", botOrUserStr));
						Embeds.AddAuthor(embed, user);
						await Messages.SendEmbedMessage(serverLog, embed);
					}

					CurrentLogModule.IncrementJoins();
				}
				else
				{
					var guildInfo = verified.GuildInfo;
					if (guildInfo == null)
						return;

					await LogChannels.HandleJoiningUsers(guildInfo, user);
				}
			}

			public async Task OnUserLeft(SocketGuildUser user)
			{
				CurrentLogModule.DecrementUsers();

				//Check if the bot was the one that left
				if (user.Id == Properties.Settings.Default.BotID)
				{
					await GuildSettingsModule.RemoveGuild(user.Guild);
					return;
				}

				if (LogChannels.VerifyServerLoggingAction(BotInfo, GuildSettingsModule, user, LogAction.UserLeft, out VerifiedLoggingAction verified))
				{
					var guild = verified.Guild;
					var guildInfo = verified.GuildInfo;
					var serverLog = verified.LoggingChannel;

					//Don't log them to the server if they're someone who was just banned for joining with a banned name
					if (guildInfo.BannedNamesForJoiningUsers.Any(x => user.Username.CaseInsContains(x.Phrase)))
						return;

					await Messages.SendGuildNotification(user, guildInfo.GoodbyeMessage);

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

					CurrentLogModule.IncrementLeaves();
				}
			}

			public async Task OnUserUpdated(SocketUser beforeUser, SocketUser afterUser)
			{
				if (beforeUser.Username == null || afterUser.Username == null || BotInfo.Pause)
					return;

				//Name change
				if (!beforeUser.Username.CaseInsEquals(afterUser.Username))
				{
					foreach (var guild in await Client.GetGuildsAsync())
					{
						if (!(await guild.GetUsersAsync()).Select(x => x.Id).Contains(afterUser.Id))
							return;

						if (LogChannels.VerifyServerLoggingAction(BotInfo, GuildSettingsModule, guild, LogAction.UserLeft, out VerifiedLoggingAction verified))
						{
							var guildInfo = verified.GuildInfo;
							var serverLog = verified.LoggingChannel;

							var embed = Embeds.MakeNewEmbed(null, null, Constants.UEDT);
							Embeds.AddFooter(embed, "Name Changed");
							Embeds.AddField(embed, "Before:", "`" + beforeUser.Username + "`");
							Embeds.AddField(embed, "After:", "`" + afterUser.Username + "`", false);
							Embeds.AddAuthor(embed, afterUser);
							await Messages.SendEmbedMessage(serverLog, embed);

							CurrentLogModule.IncrementUserChanges();
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
					await HandlePotentialBotOwner(message);
					return;
				}

				if (GuildSettingsModule.TryGetSettings(guild, out IGuildSettings guildInfo))
				{
					await HandleCloseWords(guildInfo, message);
					await HandleSpamPreventionVoting(guildInfo, guild, message);

					if (LogChannels.VerifyMessageShouldBeLogged(guildInfo, message))
					{
						await HandleChannelSettings(guildInfo, message);
						await HandleSpamPrevention(guildInfo, guild, message);
						await HandleSlowmodeOrBannedPhrases(guildInfo, guild, message);
						await HandleImageLogging(guildInfo, message);
					}
				}
			}

			public async Task OnMessageUpdated(Cacheable<IMessage, ulong> cached, SocketMessage afterMessage, ISocketMessageChannel channel)
			{
				if (LogChannels.VerifyServerLoggingAction(BotInfo, GuildSettingsModule, channel, LogAction.MessageUpdated, out VerifiedLoggingAction verified))
				{
					var guild = verified.Guild;
					var guildInfo = verified.GuildInfo;
					var serverLog = verified.LoggingChannel;

					var beforeMessage = cached.HasValue ? cached.Value : null;
					if (!LogChannels.VerifyMessageShouldBeLogged(guildInfo, afterMessage))
						return;

					await Spam.HandleBannedPhrases(guildInfo, guild, afterMessage);

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

						CurrentLogModule.IncrementEdits();
					}
					var imageLog = guildInfo.ImageLog?.Object;
					if (imageLog != null)
					{
						//If the before message is not specified always take that as it should be logged. If the embed counts are greater take that as logging too.
						if (beforeMessage?.Embeds.Count() < afterMessage.Embeds.Count())
						{
							await HandleImageLogging(guildInfo, afterMessage);
						}
					}
				}
				else
				{
					var guild = verified.Guild;
					if (guild == null)
						return;
					var guildInfo = verified.GuildInfo;
					if (guildInfo == null)
						return;

					var beforeMessage = cached.HasValue ? cached.Value : null;
					if (LogChannels.VerifyMessageShouldBeLogged(guildInfo, afterMessage))
					{
						await Spam.HandleBannedPhrases(guildInfo, guild, afterMessage);
					}
				}
			}

			public Task OnMessageDeleted(Cacheable<IMessage, ulong> cached, ISocketMessageChannel channel)
			{
				if (LogChannels.VerifyServerLoggingAction(BotInfo, GuildSettingsModule, channel, LogAction.MessageDeleted, out VerifiedLoggingAction verified))
				{
					var guild = verified.Guild;
					var guildInfo = verified.GuildInfo;
					var serverLog = verified.LoggingChannel;

					var message = cached.HasValue ? cached.Value : null;

					//Get the list of deleted messages it contains
					var msgDeletion = guildInfo.MessageDeletion;
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

					CurrentLogModule.IncrementDeletes();

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
							Messages.ExceptionToConsole(e);
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

			private async Task HandlePotentialBotOwner(IMessage message)
			{
				if (message.Content.Equals(Properties.Settings.Default.BotKey) && BotInfo.BotOwnerID == 0)
				{
					BotInfo.SetSetting(SettingOnBot.BotOwnerID, message.Author.Id);
					await Messages.SendDMMessage(message.Channel as IDMChannel, "Congratulations, you are now the owner of the bot.");
				}
			}

			private async Task HandleChannelSettings(IGuildSettings guildInfo, IMessage message)
			{
				var channel = message.Channel as ITextChannel;
				var author = message.Author as IGuildUser;
				if (channel == null || author == null || author.GuildPermissions.Administrator)
					return;

				if (guildInfo.ImageOnlyChannels.Contains(channel.Id))
				{
					if (!(message.Attachments.Any(x => x.Height != null || x.Width != null) || message.Embeds.Any(x => x.Image != null)))
					{
						await message.DeleteAsync();
					}
				}
				if (guildInfo.SanitaryChannels.Contains(channel.Id))
				{
					await message.DeleteAsync();
				}
			}

			private async Task HandleImageLogging(IGuildSettings guildInfo, IMessage message)
			{
				var logChannel = guildInfo.ImageLog?.Object;
				if (logChannel == null || message.Author.Id == Properties.Settings.Default.BotID)
					return;

				if (message.Attachments.Any())
				{
					await LogChannels.LogImage(CurrentLogModule, logChannel, message, false);
				}
				if (message.Embeds.Any())
				{
					await LogChannels.LogImage(CurrentLogModule, logChannel, message, true);
				}
			}

			private async Task HandleCloseWords(IGuildSettings guildInfo, IMessage message)
			{
				if (int.TryParse(message.Content, out int number) && number > 0 && number < 6)
				{
					--number;
					var closeWordList = Variables.ActiveCloseWords.FirstOrDefault(x => x.UserID == message.Author.Id);
					if (!closeWordList.Equals(default(ActiveCloseWord<Quote>)) && closeWordList.List.Count > number)
					{
						var quote = closeWordList.List[number].Word;
						Variables.ActiveCloseWords.ThreadSafeRemove(closeWordList);
						await Messages.SendChannelMessage(message.Channel, quote.Text);
						await Messages.DeleteMessage(message);
					}
					var closeHelpList = Variables.ActiveCloseHelp.FirstOrDefault(x => x.UserID == message.Author.Id);
					if (!closeHelpList.Equals(default(ActiveCloseWord<HelpEntry>)) && closeHelpList.List.Count > number)
					{
						var help = closeHelpList.List[number].Word;
						Variables.ActiveCloseHelp.ThreadSafeRemove(closeHelpList);

						var embed = Embeds.MakeNewEmbed(help.Name, help.ToString());
						Embeds.AddFooter(embed, "Help");
						await Messages.SendEmbedMessage(message.Channel, embed);
						await Messages.DeleteMessage(message);
					}
				}
			}

			private async Task HandleSlowmodeOrBannedPhrases(IGuildSettings guildInfo, SocketGuild guild, IMessage message)
			{
				await Spam.HandleSlowmode(guildInfo, message);
				await Spam.HandleBannedPhrases(guildInfo, guild, message);
			}

			private async Task HandleSpamPrevention(IGuildSettings guildInfo, SocketGuild guild, IMessage message)
			{
				if (Users.GetIfUserCanBeModifiedByUser(Users.GetBot(guild), message.Author))
				{
					await Spam.HandleSpamPrevention(guildInfo, guild, message.Author as IGuildUser, message);
				}
			}

			private async Task HandleSpamPreventionVoting(IGuildSettings guildInfo, SocketGuild guild, IMessage message)
			{
				//TODO: Make this work for all spam types
				//Get the users primed to be kicked/banned by the spam prevention
				var users = guildInfo.SpamPreventionUsers.Where(x =>
				{
					return true
					&& x.PotentialPunishment
					&& message.MentionedUserIds.Contains(x.User.Id)
					&& !x.UsersWhoHaveAlreadyVoted.Contains(message.Author.Id)
					&& x.User.Id != message.Author.Id;
				});

				foreach (var user in users)
				{
					user.IncreaseVotesToKick(message.Author.Id);
					if (user.UsersWhoHaveAlreadyVoted.Count < user.VotesRequired)
						return;

					await user.SpamPreventionPunishment(guildInfo);

					//Reset their current spam count and the people who have already voted on them so they don't get destroyed instantly if they join back
					user.ResetSpamUser();
				}
			}
		}

		public sealed class ModLogger : BaseLog
		{
			private ILogModule CurrentLogModule;

			public ModLogger(IDiscordClient client, IGlobalSettings botInfo, IGuildSettingsModule guildSettingsModule, ILogModule currentLogModule) : base(client, botInfo, guildSettingsModule)
			{
				CurrentLogModule = currentLogModule;
			}

			public async Task LogCommand(MyCommandContext context)
			{
				Messages.WriteLine(new LoggedCommand(context).ToString());
				await Messages.DeleteMessage(context.Message);

				if (LogChannels.VerifyMessageShouldBeLogged(context.GuildInfo, context.Message))
				{
					var modLog = context.GuildInfo.ModLog?.Object;
					if (modLog == null)
						return;

					var embed = Embeds.MakeNewEmbed(null, context.Message.Content);
					Embeds.AddFooter(embed, "Mod Log");
					Embeds.AddAuthor(embed, context.User);
					await Messages.SendEmbedMessage(modLog, embed);
				}
			}
		}
	}
}