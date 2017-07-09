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
	//Logs are commands which fire on actions. Most of these are solely for logging, but a few are for deleting certain messages.
	public class Bot_Logs : ModuleBase
	{
		public static Task Log(LogMessage msg)
		{
			if (!String.IsNullOrWhiteSpace(msg.Message))
			{
				Actions.WriteLine(msg.Message, msg.Source);
			}

			return Task.CompletedTask;
		}

		public static async Task OnGuildAvailable(SocketGuild guild)
		{
			Actions.WriteLine(String.Format("{0} is now online on shard {1}.", guild.FormatGuild(), Variables.Client.GetShardFor(guild).ShardId));
			Actions.WriteLine(String.Format("Current memory usage is: {0}MB", Actions.GetMemory().ToString("0.00")));
			Variables.TotalUsers += guild.MemberCount;
			Variables.TotalGuilds++;

			if (!Variables.Guilds.ContainsKey(guild.Id))
			{
				if (Variables.BotID != 0)
				{
					await Actions.CreateOrGetGuildInfo(guild);
				}
				else
				{
					Variables.GuildsToBeLoaded.Add(guild);
				}
			}

			return;
		}

		public static Task OnGuildUnavailable(SocketGuild guild)
		{
			Actions.WriteLine(String.Format("Guild is now offline {0}.", guild.FormatGuild()));
			Variables.TotalUsers = Math.Max(0, Variables.TotalUsers - guild.MemberCount);
			Variables.TotalGuilds = Math.Max(0, Variables.TotalGuilds--);

			return Task.CompletedTask;
		}

		public static async Task OnJoinedGuild(SocketGuild guild)
		{
			Actions.WriteLine(String.Format("Bot has joined {0}.", guild.FormatGuild()));

			//Check how many bots are in the guild
			var botCount = guild.Users.Count(x => x.IsBot);

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
			if (botCount / (users * 1.0) > percentage)
			{
				await guild.LeaveAsync();
			}

			//Warn if at the maximum
			var guilds = Variables.Client.GetGuilds().Count;
			var shards = Variables.Client.GetShards().Count;
			var curMax = shards * 2500;
			if (guilds + 100 >= curMax)
			{
				Actions.WriteLine(String.Format("The bot currently has {0} out of {1} possible spots for servers filled. Please increase the shard count.", guilds, curMax));
			}
			//Leave the guild
			if (guilds > curMax)
			{
				await guild.LeaveAsync();
				Actions.WriteLine(String.Format("Left the guild {0} due to having too many guilds on the client and not enough shards.", guild.FormatGuild()));
			}

			return;
		}

		public static Task OnLeftGuild(SocketGuild guild)
		{
			Actions.WriteLine(String.Format("Bot has left {0}.", guild.FormatGuild()));

			Variables.TotalUsers -= (guild.MemberCount + 1);
			Variables.TotalGuilds--;

			return Task.CompletedTask;
		}
	}

	public class Server_Logs : ModuleBase
	{
		public static async Task OnUserJoined(SocketGuildUser user)
		{
			++Variables.TotalUsers;

			if (Actions.VerifyServerLoggingAction(user, LogAction.UserJoined, out VerifiedLoggingAction verified))
			{
				var guild = verified.Guild;
				var guildInfo = verified.GuildInfo;
				var serverLog = verified.LoggingChannel;

				if (guildInfo != null)
				{
					await Actions.HandleJoiningUsers(guildInfo, user);
				}

				var curInv = await Actions.GetInviteUserJoinedOn(guildInfo, guild);
				var inviteStr = curInv != null ? String.Format("\n**Invite:** {0}", curInv.Code) : "";
				var userAccAge = (DateTime.UtcNow - user.CreatedAt.ToUniversalTime());
				var ageWarningStr = userAccAge.TotalHours <= 24 ? String.Format("\n**New Account:** {0} hours, {1} minutes old.", (int)userAccAge.TotalHours, (int)userAccAge.Minutes) : "";
				var botOrUserStr = user.IsBot ? "Bot" : "User";

				//Bans people who join with a given word in their name
				if (((List<BannedPhrase>)guildInfo.GetSetting(SettingOnGuild.BannedNamesForJoiningUsers)).Any(x => Actions.CaseInsIndexOf(user.Username, x.Phrase)))
				{
					await Actions.BotBanUser(guild, user.Id, 1, "banned name");
					return;
				}
				//Welcome message
				else
				{
					await Actions.SendGuildNotification(user, ((GuildNotification)guildInfo.GetSetting(SettingOnGuild.WelcomeMessage)));
				}

				{
					var embed = Actions.MakeNewEmbed(null, String.Format("**ID:** {0}{1}{2}", user.Id, inviteStr, ageWarningStr), Constants.JOIN);
					Actions.AddFooter(embed, String.Format("{0} Joined", botOrUserStr));
					Actions.AddAuthor(embed, user.FormatUser(), user.GetAvatarUrl());
					await Actions.SendEmbedMessage(serverLog, embed);
				}

				++Variables.LoggedJoins;
			}
			else
			{
				var guildInfo = verified.GuildInfo;
				if (guildInfo == null)
					return;

				await Actions.HandleJoiningUsers(guildInfo, user);
			}
		}

		public static async Task OnUserLeft(SocketGuildUser user)
		{
			--Variables.TotalUsers;

			//Check if the bot was the one that left
			if (user.Id == Variables.BotID)
			{
				Variables.Guilds.Remove(user.Guild.Id);
				return;
			}

			if (Actions.VerifyServerLoggingAction(user, LogAction.UserLeft, out VerifiedLoggingAction verified))
			{
				var guild = verified.Guild;
				var guildInfo = verified.GuildInfo;
				var serverLog = verified.LoggingChannel;

				//Don't log them to the server if they're someone who was just banned for joining with a banned name
				if (((List<BannedPhrase>)guildInfo.GetSetting(SettingOnGuild.BannedNamesForJoiningUsers)).Any(x => Actions.CaseInsIndexOf(user.Username, x.Phrase)))
					return;

				await Actions.SendGuildNotification(user, ((GuildNotification)guildInfo.GetSetting(SettingOnGuild.GoodbyeMessage)));

				var lengthStayed = "";
				if (user.JoinedAt.HasValue)
				{
					var time = DateTime.UtcNow.Subtract(user.JoinedAt.Value.UtcDateTime);
					lengthStayed = String.Format("\n**Stayed for:** {0}:{1:00}:{2:00}:{3:00}", time.Days, time.Hours, time.Minutes, time.Seconds);
				}
				var botOrUserStr = user.IsBot ? "Bot" : "User";

				var embed = Actions.MakeNewEmbed(null, String.Format("**ID:** {0}{1}", user.Id, lengthStayed), Constants.LEAV);
				Actions.AddFooter(embed, String.Format("{0} Left", botOrUserStr));
				Actions.AddAuthor(embed, user.FormatUser(), user.GetAvatarUrl());
				await Actions.SendEmbedMessage(serverLog, embed);

				++Variables.LoggedLeaves;
			}
		}

		public static async Task OnUserUpdated(SocketUser beforeUser, SocketUser afterUser)
		{
			if (beforeUser.Username == null || afterUser.Username == null || Variables.Pause)
				return;

			//Name change
			if (!Actions.CaseInsEquals(beforeUser.Username, afterUser.Username))
			{
				foreach (var guild in Variables.Client.GetGuilds().Where(x => x.Users.Contains(afterUser)))
				{
					if (Actions.VerifyServerLoggingAction(guild, LogAction.UserLeft, out VerifiedLoggingAction verified))
					{
						var guildInfo = verified.GuildInfo;
						var serverLog = verified.LoggingChannel;

						var embed = Actions.MakeNewEmbed(null, null, Constants.UEDT);
						Actions.AddFooter(embed, "Name Changed");
						Actions.AddField(embed, "Before:", "`" + beforeUser.Username + "`");
						Actions.AddField(embed, "After:", "`" + afterUser.Username + "`", false);
						Actions.AddAuthor(embed, afterUser.FormatUser(), afterUser.GetAvatarUrl());
						await Actions.SendEmbedMessage(serverLog, embed);

						++Variables.LoggedUserChanges;
					}
				}
			}
		}

		public static async Task OnMessageReceived(SocketMessage message)
		{
			var guild = Actions.GetGuild(message) as SocketGuild;
			if (guild == null)
			{
				//Check if the user is trying to become the bot owner by DMing the bot its key
				await Message_Received_Actions.HandlePotentialBotOwner(message);
				return;
			}

			var guildInfo = await Actions.CreateOrGetGuildInfo(guild);
			await Message_Received_Actions.HandleCloseWords(guildInfo, message);
			await Message_Received_Actions.HandleSpamPreventionVoting(guildInfo, guild, message);

			if (Actions.VerifyMessageShouldBeLogged(guildInfo, message))
			{
				await Message_Received_Actions.HandleChannelSettings(guildInfo, message);
				await Message_Received_Actions.HandleSpamPrevention(guildInfo, guild, message);
				await Message_Received_Actions.HandleSlowmodeOrBannedPhrases(guildInfo, guild, message);
				await Message_Received_Actions.HandleImageLogging(guildInfo, message);
			}
		}

		public static async Task OnMessageUpdated(Cacheable<IMessage, ulong> cached, SocketMessage afterMessage, ISocketMessageChannel channel)
		{
			if (Actions.VerifyServerLoggingAction(channel, LogAction.MessageUpdated, out VerifiedLoggingAction verified))
			{
				var guild = verified.Guild;
				var guildInfo = verified.GuildInfo;
				var serverLog = verified.LoggingChannel;

				var beforeMessage = cached.HasValue ? cached.Value : null;
				if (!Actions.VerifyMessageShouldBeLogged(guildInfo, afterMessage))
					return;
				await Actions.HandleBannedPhrases(guildInfo, guild, afterMessage);

				if (serverLog != null)
				{
					var beforeMsgContent = Actions.ReplaceMarkdownChars(beforeMessage?.Content ?? "", true);
					var afterMsgContent = Actions.ReplaceMarkdownChars(afterMessage.Content, true);
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

					var embed = Actions.MakeNewEmbed(null, null, Constants.MEDT);
					Actions.AddFooter(embed, "Message Updated");
					Actions.AddField(embed, "Before:", String.Format("`{0}`", beforeMsgContent));
					Actions.AddField(embed, "After:", String.Format("`{0}`", afterMsgContent), false);
					Actions.AddAuthor(embed, String.Format("{0} in #{1}", afterMessage.Author.FormatUser(), afterMessage.Channel), afterMessage.Author.GetAvatarUrl());
					await Actions.SendEmbedMessage(serverLog, embed);
					++Variables.LoggedEdits;
				}
				var imageLog = ((DiscordObjectWithID<ITextChannel>)guildInfo.GetSetting(SettingOnGuild.ImageLog));
				if (imageLog != null)
				{
					//If the before message is not specified always take that as it should be logged. If the embed counts are greater take that as logging too.
					if (beforeMessage?.Embeds.Count() < afterMessage.Embeds.Count())
					{
						await Message_Received_Actions.HandleImageLogging(guildInfo, afterMessage);
						++Variables.LoggedEdits;
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
				if (Actions.VerifyMessageShouldBeLogged(guildInfo, afterMessage))
				{
					await Actions.HandleBannedPhrases(guildInfo, guild, afterMessage);
				}
			}
		}

		public static async Task OnMessageDeleted(Cacheable<IMessage, ulong> cached, ISocketMessageChannel channel)
		{
			if (Actions.VerifyServerLoggingAction(channel, LogAction.MessageDeleted, out VerifiedLoggingAction verified))
			{
				var guild = verified.Guild;
				var guildInfo = verified.GuildInfo;
				var serverLog = verified.LoggingChannel;

				var message = cached.HasValue ? cached.Value : null;

				//Get the list of deleted messages it contains
				var msgDeletion = ((MessageDeletion)guildInfo.GetSetting(SettingOnGuild.MessageDeletion));
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

				//Increment the deleted messages count
				++Variables.LoggedDeletes;

				//Make a separate task in order to not mess up the other commands
				Actions.DontWaitForResultOfBigUnimportantFunction(null, async () =>
				{
					try
					{
						await Task.Delay(TimeSpan.FromSeconds(Constants.TIME_TO_WAIT_BEFORE_MESSAGE_PRINT_TO_THE_SERVER_LOG), cancelToken.Token);
					}
					catch (TaskCanceledException)
					{
						return;
					}
					catch (Exception e)
					{
						Actions.ExceptionToConsole(e);
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
					await Actions.SendDeleteMessage(guild, serverLog, Actions.FormatMessages(deletedMessages.Where(x => x.CreatedAt != null).OrderBy(x => x.CreatedAt.Ticks)));
				});

				//To get it to not want an await
				await Task.Yield();
			}
		}
	}

	public class Mod_Logs : ModuleBase
	{
		public static async Task LogCommand(BotGuildInfo guildInfo, ICommandContext context)
		{
			Variables.GuildsThatHaveBeenToldTheBotDoesNotWorkWithoutAdministratorAndWillBeIgnoredThuslyUntilTheyGiveTheBotAdministratorOrTheBotRestarts.ThreadSafeRemove(context.Guild.Id);
			Actions.WriteLine(new LoggedCommand(context).ToString());

			if (Actions.VerifyMessageShouldBeLogged(guildInfo, context.Message))
			{
				var modLog = ((DiscordObjectWithID<ITextChannel>)guildInfo.GetSetting(SettingOnGuild.ModLog))?.Object;
				if (modLog == null)
					return;

				var embed = Actions.MakeNewEmbed(null, context.Message.Content);
				Actions.AddFooter(embed, "Mod Log");
				Actions.AddAuthor(embed, String.Format("{0} in #{1}", context.User.FormatUser(), context.Channel.Name), context.User.GetAvatarUrl());
				await Actions.SendEmbedMessage(modLog, embed);
			}
		}
	}

	public class Message_Received_Actions : ModuleBase
	{
		public static async Task HandlePotentialBotOwner(IMessage message)
		{
			if (message.Content.Equals(Properties.Settings.Default.BotKey) && ((ulong)Variables.BotInfo.GetSetting(SettingOnBot.BotOwnerID)) == 0)
			{
				Variables.BotInfo.SetSetting(SettingOnBot.BotOwnerID, message.Author.Id);
				await Actions.SendDMMessage(message.Channel as IDMChannel, "Congratulations, you are now the owner of the bot.");
			}
		}

		public static async Task HandleChannelSettings(BotGuildInfo guildInfo, IMessage message)
		{
			var channel = message.Channel as ITextChannel;
			var author = message.Author as IGuildUser;
			if (channel == null || author == null || author.GuildPermissions.Administrator)
				return;

			if (((List<ulong>)guildInfo.GetSetting(SettingOnGuild.ImageOnlyChannels)).Contains(channel.Id))
			{
				if (!(message.Attachments.Any(x => x.Height != null || x.Width != null) || message.Embeds.Any(x => x.Image != null)))
				{
					await message.DeleteAsync();
				}
			}
			if (((List<ulong>)guildInfo.GetSetting(SettingOnGuild.SanitaryChannels)).Contains(channel.Id))
			{
				await message.DeleteAsync();
			}
		}

		public static async Task HandleImageLogging(BotGuildInfo guildInfo, IMessage message)
		{
			var logChannel = ((DiscordObjectWithID<ITextChannel>)guildInfo.GetSetting(SettingOnGuild.ImageLog))?.Object;
			if (logChannel == null || message.Author.Id == Variables.BotID)
				return;

			if (message.Attachments.Any())
			{
				await Actions.LogImage(logChannel, message, false);
			}
			if (message.Embeds.Any())
			{
				await Actions.LogImage(logChannel, message, true);
			}
		}

		public static async Task HandleCloseWords(BotGuildInfo guildInfo, IMessage message)
		{
			if (int.TryParse(message.Content, out int number) && number > 0 && number < 6)
			{
				--number;
				var closeWordList = Variables.ActiveCloseWords.FirstOrDefault(x => x.UserID == message.Author.Id);
				if (!closeWordList.Equals(default(ActiveCloseWord<Quote>)) && closeWordList.List.Count > number)
				{
					var quote = closeWordList.List[number].Word;
					Variables.ActiveCloseWords.ThreadSafeRemove(closeWordList);
					await Actions.SendChannelMessage(message.Channel, quote.Text);
					await Actions.DeleteMessage(message);
				}
				var closeHelpList = Variables.ActiveCloseHelp.FirstOrDefault(x => x.UserID == message.Author.Id);
				if (!closeHelpList.Equals(default(ActiveCloseWord<HelpEntry>)) && closeHelpList.List.Count > number)
				{
					var help = closeHelpList.List[number].Word;
					Variables.ActiveCloseHelp.ThreadSafeRemove(closeHelpList);
					await Actions.SendEmbedMessage(message.Channel, Actions.AddFooter(Actions.MakeNewEmbed(help.Name, Actions.GetHelpString(help, Actions.GetPrefix(guildInfo))), "Help"));
					await Actions.DeleteMessage(message);
				}
			}
		}

		public static async Task HandleSlowmodeOrBannedPhrases(BotGuildInfo guildInfo, SocketGuild guild, IMessage message)
		{
			var smGuild = ((SlowmodeGuild)guildInfo.GetSetting(SettingOnGuild.SlowmodeGuild));
			var smChan = ((List<SlowmodeChannel>)guildInfo.GetSetting(SettingOnGuild.SlowmodeChannels)).FirstOrDefault(x => x.ChannelID == message.Channel.Id);
			if (smGuild != null || smChan != null)
			{
				await Actions.HandleSlowmode(smGuild, smChan, message);
			}

			var bannedStrings = (List<BannedPhrase>)guildInfo.GetSetting(SettingOnGuild.BannedPhraseStrings);
			var bannedRegex = (List<BannedPhrase>)guildInfo.GetSetting(SettingOnGuild.BannedPhraseRegex);
			if (bannedStrings.Any() || bannedRegex.Any())
			{
				await Actions.HandleBannedPhrases(guildInfo, guild, message);
			}
		}

		public static async Task HandleSpamPrevention(BotGuildInfo guildInfo, SocketGuild guild, IMessage message)
		{
			if (Actions.GetUserPosition(guild, message.Author) < Actions.GetUserPosition(guild, Actions.GetBot(guild)))
			{
				await Actions.SpamCheck(guildInfo, guild, message.Author as IGuildUser, message);
			}
		}

		public static async Task HandleSpamPreventionVoting(BotGuildInfo guildInfo, SocketGuild guild, IMessage message)
		{
			//TODO: Make this work for all spam types
			//Get the users primed to be kicked/banned by the spam prevention
			var users = ((List<SpamPreventionUser>)guildInfo.GetSetting(SettingOnGuild.SpamPreventionUsers)).Where(x =>
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
				if (Actions.GetUserPosition(guild, user.User) >= Actions.GetUserPosition(guild, Actions.GetBot(guild)) || user.VotesToKick < user.VotesRequired)
					return;

				await user.Punish(guildInfo, guild);

				//Reset their current spam count and the people who have already voted on them so they don't get destroyed instantly if they join back
				user.ResetSpamUser();
			}
		}
	}
}