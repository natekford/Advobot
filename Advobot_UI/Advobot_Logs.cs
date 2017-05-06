using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Advobot
{
	//Logs are commands which fire on actions. Most of these are solely for logging, but a few are for deleting certain messages.
	public class Bot_Logs : ModuleBase
	{
		public static Task Log(LogMessage msg)
		{
			Console.WriteLine(msg.ToString());
			return Task.CompletedTask;
		}

		public static Task OnGuildAvailable(SocketGuild guild)
		{
			Actions.WriteLine(String.Format("{0}: {1} is now online on shard {2}.", MethodBase.GetCurrentMethod().Name, guild.FormatGuild(), Variables.Client.GetShardFor(guild).ShardId));
			Variables.TotalUsers += guild.MemberCount;
			Variables.TotalGuilds++;

			if (!Variables.Guilds.ContainsKey(guild.Id))
			{
				if (Variables.Bot_ID != 0)
				{
					Actions.LoadGuild(guild);
				}
				else
				{
					Variables.GuildsToBeLoaded.Add(guild);
				}
			}

			return Task.CompletedTask;
		}

		public static Task OnGuildUnavailable(SocketGuild guild)
		{
			Actions.WriteLine(String.Format("{0}: Guild is now offline {1}.", MethodBase.GetCurrentMethod().Name, guild.FormatGuild()));

			Variables.TotalUsers -= (guild.MemberCount + 1);
			if (Variables.TotalUsers < 0)
				Variables.TotalUsers = 0;
			Variables.TotalGuilds--;
			if (Variables.TotalGuilds < 0)
				Variables.TotalGuilds = 0;

			return Task.CompletedTask;
		}

		public static Task OnJoinedGuild(SocketGuild guild)
		{
			Actions.WriteLine(String.Format("{0}: Bot has joined {1}.", MethodBase.GetCurrentMethod().Name, guild.FormatGuild()));

			//Check how many bots are in the guild
			int botCount = 0;
			guild.Users.ToList().ForEach(x =>
			{
				if (x.IsBot)
				{
					++botCount;
				}
			});

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
				Task.Run(async () =>
				{
					await guild.LeaveAsync();
				});
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
				Task.Run(async () =>
				{
					await guild.LeaveAsync();
				});
				//Send a message to the console
				Actions.WriteLine(String.Format("Left the guild {0} due to having too many guilds on the client and not enough shards.", guild.FormatGuild()));
			}

			return Task.CompletedTask;
		}

		public static Task OnLeftGuild(SocketGuild guild)
		{
			Actions.WriteLine(String.Format("{0}: Bot has left {1}.", MethodBase.GetCurrentMethod().Name, guild.FormatGuild()));

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

			var guild = user.Guild;
			if (guild == null)
				return;
			if (!Variables.Guilds.TryGetValue(guild.Id, out BotGuildInfo guildInfo) || !Actions.VerifyLogging(guildInfo, LogActions.UserJoined))
				return;
			var serverLog = guildInfo.ServerLog;
			if (serverLog == null)
				return;

			//Slowmode
			if (guildInfo.SlowmodeGuild != null || guild.TextChannels.Select(x => x.Id).Intersect(guildInfo.SlowmodeChannels.Select(x => x.ChannelID)).Any())
			{
				await Actions.AddSlowmodeUser(guildInfo, user);
			}
			//Antiraid
			var antiRaid = guildInfo.AntiRaid;
			if (antiRaid != null)
			{
				//Give them the mute role
				await user.AddRoleAsync(antiRaid.MuteRole);
				//Add them to the list of users who have been muted
				antiRaid.AddUserToMutedList(user);
			}
			//Antiraid Two - Electric Joinaroo
			var antiJoin = guildInfo.JoinProtection;
			if (antiJoin != null)
			{
				antiJoin.Increase();
				if (antiJoin.JoinCount >= antiJoin.JoinLimit)
				{
					//TODO: Finish implementation later
					//Actions.
				}
			}
			//Welcome message
			await Actions.SendGuildNotification(user, guildInfo.WelcomeMessage);

			var curInv = await Actions.GetInviteUserJoinedOn(guild);
			var inviteString = curInv != null ? String.Format("\n**Invite:** {0}", curInv.Code) : "";
			var userAccAge = (DateTime.UtcNow - user.CreatedAt.ToUniversalTime());
			var ageWarningString = userAccAge.TotalHours <= 24 ? String.Format("\n**New Account:** {0} hours, {1} minutes old.", (int)userAccAge.TotalHours, (int)userAccAge.Minutes) : "";

			if (user.IsBot)
			{
				var embed = Actions.MakeNewEmbed(null, String.Format("**ID:** {0}{1}{2}", user.Id, inviteString, ageWarningString), Constants.JOIN);
				Actions.AddFooter(embed, "Bot Joined");
				Actions.AddAuthor(embed, user.FormatUser(), user.GetAvatarUrl());
				await Actions.SendEmbedMessage(serverLog, embed);
			}
			else
			{
				var embed = Actions.MakeNewEmbed(null, String.Format("**ID:** {0}{1}{2}", user.Id, inviteString, ageWarningString), Constants.JOIN);
				Actions.AddFooter(embed, "User Joined");
				Actions.AddAuthor(embed, user.FormatUser(), user.GetAvatarUrl());
				await Actions.SendEmbedMessage(serverLog, embed);
			}

			++Variables.LoggedJoins;
		}

		public static async Task OnUserLeft(SocketGuildUser user)
		{
			--Variables.TotalUsers;

			var guild = Actions.GetGuild(user);
			if (guild == null)
				return;
			if (!Variables.Guilds.TryGetValue(guild.Id, out BotGuildInfo guildInfo) || !Actions.VerifyLogging(guildInfo, LogActions.UserLeft))
				return;
			var serverLog = guildInfo.ServerLog;
			if (serverLog == null)
				return;

			//Check if the bot was the one that left
			if (user.Id == Variables.Bot_ID)
			{
				Variables.Guilds.Remove(user.Guild.Id);
				return;
			}
			//Goodbye message
			await Actions.SendGuildNotification(user, guildInfo.GoodbyeMessage);
			//Format the length stayed string
			var lengthStayed = "";
			if (user.JoinedAt.HasValue)
			{
				var time = DateTime.UtcNow.Subtract(user.JoinedAt.Value.UtcDateTime);
				lengthStayed = String.Format("\n**Stayed for:** {0}:{1:00}:{2:00}:{3:00}", time.Days, time.Hours, time.Minutes, time.Seconds);
			}

			if (user.IsBot)
			{
				var embed = Actions.MakeNewEmbed(null, String.Format("**ID:** {0}{1}", user.Id, lengthStayed), Constants.LEAV);
				Actions.AddFooter(embed, "Bot Left");
				Actions.AddAuthor(embed, user.FormatUser(), user.GetAvatarUrl());
				await Actions.SendEmbedMessage(serverLog, embed);
			}
			else
			{
				var embed = Actions.MakeNewEmbed(null, String.Format("**ID:** {0}{1}", user.Id, lengthStayed), Constants.LEAV);
				Actions.AddFooter(embed, "User Left");
				Actions.AddAuthor(embed, user.FormatUser(), user.GetAvatarUrl());
				await Actions.SendEmbedMessage(serverLog, embed);
			}

			++Variables.LoggedLeaves;
		}

		public static async Task OnUserUpdated(SocketUser beforeUser, SocketUser afterUser)
		{
			if (beforeUser.Username == null || afterUser.Username == null || Variables.Pause)
				return;

			//Name change
			if (!Actions.CaseInsEquals(beforeUser.Username, afterUser.Username))
			{
				await Variables.Client.GetGuilds().Where(x => x.Users.Contains(afterUser)).ToList().ForEachAsync(async inputGuild =>
				{
					if (!Variables.Guilds.TryGetValue(inputGuild.Id, out BotGuildInfo guildInfo) || !Actions.VerifyLogging(guildInfo, LogActions.UserUpdated))
						return;
					var guild = guildInfo.Guild;
					if (guild == null)
						return;
					var serverLog = guildInfo.ServerLog;
					if (serverLog == null)
						return;

					var embed = Actions.MakeNewEmbed(null, null, Constants.UEDT);
					Actions.AddFooter(embed, "Name Changed");
					Actions.AddField(embed, "Before:", "`" + beforeUser.Username + "`");
					Actions.AddField(embed, "After:", "`" + afterUser.Username + "`", false);
					Actions.AddAuthor(embed, afterUser.FormatUser(), afterUser.GetAvatarUrl());
					await Actions.SendEmbedMessage(serverLog, embed);

					++Variables.LoggedUserChanges;
				});
			}
		}

		public static async Task OnMessageReceived(SocketMessage message)
		{
			var guild = Actions.GetGuild(message);
			if (guild == null)
			{
				//Check if the user is trying to become the bot owner by DMing the bot is key
				await Message_Received_Actions.BotOwner(message);
			}
			else if (Variables.Guilds.TryGetValue(guild.Id, out BotGuildInfo guildInfo))
			{
				if (!Actions.VerifyMessageShouldBeLogged(message))
					return;

				if (!guildInfo.IgnoredCommandChannels.Contains(message.Channel.Id))
				{
					await Message_Received_Actions.ModifyPreferences(guildInfo, guild, message);
					await Message_Received_Actions.CloseWords(guildInfo, guild, message);
				}
				if (!guildInfo.IgnoredLogChannels.Contains(message.Channel.Id))
				{
					await Message_Received_Actions.VotingOnSpamPrevention(guildInfo, guild, message);
					await Message_Received_Actions.SpamPrevention(guildInfo, guild, message);
					await Message_Received_Actions.SlowmodeOrBannedPhrases(guildInfo, guild, message);
					await Message_Received_Actions.ImageLog(guildInfo, guildInfo.ImageLog, message);
				}
			}
		}
		
		public static async Task OnMessageUpdated(Cacheable<IMessage, ulong> beforeMessage, SocketMessage afterMessage, ISocketMessageChannel channel)
		{
			var beforeMessageValue = beforeMessage.HasValue ? beforeMessage.Value : null;
			var guild = Actions.GetGuild(channel);
			if (guild == null)
				return;
			if (!Variables.Guilds.TryGetValue(guild.Id, out BotGuildInfo guildInfo))
				return;
			await Actions.BannedPhrases(guildInfo, afterMessage);
			if (!Actions.VerifyLogging(guildInfo, afterMessage, LogActions.MessageUpdated))
				return;
			var serverLog = guildInfo.ServerLog;
			if (serverLog == null)
				return;

			//If the before message is not specified always take that as it should be logged. If the embed counts are greater take that as logging too.
			if (beforeMessageValue?.Embeds.Count() < afterMessage.Embeds.Count())
			{
				await Message_Received_Actions.ImageLog(guildInfo, serverLog, afterMessage);
			}

			var beforeMsgContent = Actions.ReplaceMarkdownChars(beforeMessageValue?.Content ?? "");
			var afterMsgContent = Actions.ReplaceMarkdownChars(afterMessage.Content);
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

		public static async Task OnMessageDeleted(Cacheable<IMessage, ulong> message, ISocketMessageChannel channel)
		{
			var messageValue = message.HasValue ? message.Value : null;
			var guild = Actions.GetGuild(channel);
			if (guild == null)
				return;
			if (!Variables.Guilds.TryGetValue(guild.Id, out BotGuildInfo guildInfo) || !Actions.VerifyLogging(guildInfo, messageValue, LogActions.MessageDeleted))
				return;
			var serverLog = guildInfo.ServerLog;
			if (serverLog == null)
				return;

			//Get the info stored for that guild on the bot
			if (!Variables.Guilds.TryGetValue(guild.Id, out BotGuildInfo botInfo))
				return;

			//Get the list of deleted messages it contains
			lock (botInfo.MessageDeletion)
			{
				botInfo.MessageDeletion.AddToList(message.Value);
			}

			//Use a token so the messages do not get sent prematurely
			var cancelToken = botInfo.MessageDeletion.CancelToken;
			if (cancelToken != null)
			{
				cancelToken.Cancel();
			}
			cancelToken = new CancellationTokenSource();
			botInfo.MessageDeletion.SetCancelToken(cancelToken);

			//Increment the deleted messages count
			++Variables.LoggedDeletes;

			//Make a separate task in order to not mess up the other commands
			var t = Task.Run(async () =>
			{
				try
				{
					await Task.Delay(TimeSpan.FromSeconds(Constants.TIME_TO_WAIT_BEFORE_MESSAGE_PRINT_TO_THE_SERVER_LOG), cancelToken.Token);
				}
				catch (TaskCanceledException)
				{
					return;
				}

				//Give the messages to a new list so they can be removed from the old one
				List<IMessage> deletedMessages;
				lock (botInfo.MessageDeletion)
				{
					deletedMessages = new List<IMessage>(botInfo.MessageDeletion.GetList().Select(x => x as IMessage));
					botInfo.MessageDeletion.ClearList();
				}

				//Put the message content into a list of strings for easy usage
				var deletedMessagesContent = Actions.FormatDeletedMessages(deletedMessages.Where(x => x.CreatedAt != null).OrderBy(x => x.CreatedAt.Ticks).ToList());
				await Actions.SendDeleteMessage(guild, serverLog, deletedMessagesContent);
			});

			//To get it to not want an await
			await Task.Yield();
		}
	}

	public class Mod_Logs : ModuleBase
	{
		public static async Task LogCommand(BotGuildInfo guildInfo, CommandContext context)
		{
			//Write into the console what the command was and who said it
			var user = context.User;
			Actions.WriteLine(String.Format("{0} on {1}: \"{2}\"", user.FormatUser(), context.Guild.FormatGuild(), context.Message.Content));
			Variables.GuildsThatHaveBeenToldTheBotDoesNotWorkWithoutAdministratorAndWillBeIgnoredThuslyUntilTheyGiveTheBotAdministratorOrTheBotRestarts.Remove(context.Guild);
			await Actions.DeleteMessage(context.Message);

			var modLog = guildInfo.ModLog;
			if (modLog == null)
				return;

			//Make the embed
			var embed = Actions.MakeNewEmbed(description: context.Message.Content);
			Actions.AddFooter(embed, "Mod Log");
			Actions.AddAuthor(embed, String.Format("{0} in #{1}", user.FormatUser(), context.Channel.Name), context.User.GetAvatarUrl());
			await Actions.SendEmbedMessage(modLog, embed);
		}
	}

	public class Message_Received_Actions : ModuleBase
	{
		//public static async Task xd(IGuild guild, IUser user)
		//{
		//	const string XD = "xd";

		//	//Make sure the user is valid
		//	var guildUser = user as IGuildUser;
		//	if (guildUser == null)
		//		return;

		//	//Make sure the user's nickname isn't already xd and make sure the bot has the position to modify this user's nickname
		//	if (!Actions.CaseInsEquals(guildUser.Nickname, XD) && Actions.GetPosition(guild, user) < Actions.GetPosition(guild, await Actions.GetBot(guild)))
		//	{
		//		await guildUser.ModifyAsync(x => x.Nickname = XD);
		//	}
		//}

		public static async Task ImageLog(BotGuildInfo guildInfo, ITextChannel logChannel, IMessage message)
		{
			if (logChannel == null || message.Author.Id == Variables.Bot_ID)
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

		public static async Task BotOwner(IMessage message)
		{
			//See if they're on the list to be a potential bot owner
			if (Variables.PotentialBotOwners.Contains(message.Author.Id))
			{
				//If the key they input is the same as the bots key then they become owner
				if (message.Content.Equals(Properties.Settings.Default.BotKey))
				{
					Variables.BotInfo.SetBotOwner(message.Author.Id);
					Actions.SaveBotInfo();
					Variables.PotentialBotOwners.Clear();
					await Actions.SendDMMessage(message.Channel as IDMChannel, "Congratulations, you are now the owner of the bot.");
				}
				else
				{
					Variables.PotentialBotOwners.Remove(message.Author.Id);
					await Actions.SendDMMessage(message.Channel as IDMChannel, "That is the incorrect key.");
				}
			}
		}

		public static async Task ModifyPreferences(BotGuildInfo guildInfo, IGuild guild, IMessage message)
		{
			//Check if it's the owner of the guild saying something
			if (message.Author.Id == guild.OwnerId || Actions.GetIfUserIsOwnerButBotIsOwner(guild, message.Author))
			{
				//If the message is only 'yes' then check if they're enabling or deleting preferences
				if (Actions.CaseInsEquals(message.Content, "yes"))
				{
					if (guildInfo.EnablingPrefs)
					{
						//Enable preferences
						await Actions.EnablePreferences(guildInfo, guild, message as IUserMessage);
					}
					else if (guildInfo.DeletingPrefs)
					{
						//Delete preferences
						await Actions.DeletePreferences(guildInfo, guild, message as IUserMessage);
					}
				}
			}
		}

		public static async Task CloseWords(BotGuildInfo guildInfo, IGuild guild, IMessage message)
		{
			//Get the number
			if (!int.TryParse(message.Content, out int number))
				return;

			if (number > 0 && number < 6)
			{
				--number;
				var closeWordList = Variables.ActiveCloseWords.FirstOrDefault(x => x.User == message.Author as IGuildUser);
				if (closeWordList.User != null && closeWordList.List.Count > number)
				{
					var remind = Variables.Guilds[guild.Id].Reminds.FirstOrDefault(x => Actions.CaseInsEquals(x.Name, closeWordList.List[number].Name));
					Variables.ActiveCloseWords.ThreadSafeRemove(closeWordList);
					await Actions.SendChannelMessage(message.Channel, remind.Text);
					await Actions.DeleteMessage(message);
				}
				var closeHelpList = Variables.ActiveCloseHelp.FirstOrDefault(x => x.User == message.Author as IGuildUser);
				if (closeHelpList.User != null && closeHelpList.List.Count > number)
				{
					var help = closeHelpList.List[number].Help;
					Variables.ActiveCloseHelp.ThreadSafeRemove(closeHelpList);
					await Actions.SendEmbedMessage(message.Channel, Actions.AddFooter(Actions.MakeNewEmbed(help.Name, Actions.GetHelpString(help)), "Help"));
					await Actions.DeleteMessage(message);
				}
			}
		}

		public static async Task SlowmodeOrBannedPhrases(BotGuildInfo guildInfo, IGuild guild, IMessage message)
		{
			//Make sure the message is a valid message to do this to
			if (message == null || message.Author.IsBot || message.Author.IsWebhook)
				return;

			if (guildInfo.SlowmodeGuild != null || guildInfo.SlowmodeChannels.Any(x => x.ChannelID == message.Channel.Id))
			{
				await Actions.Slowmode(guildInfo, message);
			}
			if (guildInfo.BannedPhrases.Strings.Any() || guildInfo.BannedPhrases.Regex.Any())
			{
				await Actions.BannedPhrases(guildInfo, message);
			}
		}

		public static async Task SpamPrevention(BotGuildInfo guildInfo, IGuild guild, IMessage msg)
		{
			var author = msg.Author as IGuildUser;
			if (Actions.GetUserPosition(guild, author) >= Actions.GetUserPosition(guild, Actions.GetBot(guild)))
				return;

			var global = guildInfo.GlobalSpamPrevention;
			var isSpam = await Actions.SpamCheck(global, guild, author, msg);

			if (!isSpam)
				return;

			await Actions.DeleteMessage(msg);
		}

		public static async Task VotingOnSpamPrevention(BotGuildInfo guildInfo, IGuild guild, IMessage message)
		{
			//Get the users primed to be kicked/banned by the spam prevention
			var users = guildInfo.GlobalSpamPrevention.SpamPreventionUsers.Where(x => x.PotentialKick).ToList();
			if (!users.Any())
				return;

			//Cross reference the almost kicked users and the mentioned users
			await users.Where(x => message.MentionedUserIds.Contains(x.User.Id)).ToList().ForEachAsync(async x =>
			{
				//Check if mentioned users contains any users almost kicked. Check if the person has already voted. Don't allow users to vote on themselves.
				if (x.UsersWhoHaveAlreadyVoted.Contains(message.Author.Id) || x.User.Id == message.Author.Id)
					return;
				//Increment the votes on that user
				x.IncreaseVotesToKick();
				//Add the author to the already voted list
				x.AddUserToVotedList(message.Author.Id);
				//Check if the bot can even kick/ban this user or if they should be punished
				if (Actions.GetUserPosition(guild, x.User) >= Actions.GetUserPosition(guild, Actions.GetBot(guild)) || x.VotesToKick < x.VotesRequired)
					return;
				//Check if they've already been kicked to determine if they should be banned or kicked
				await (x.AlreadyKicked ? guild.AddBanAsync(x.User, 1) : x.User.KickAsync());
				//Reset their current spam count and the people who have already voted on them so they don't get destroyed instantly if they join back
				x.ResetSpamUser();
			});
		}
	}
}