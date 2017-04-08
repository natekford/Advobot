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
			Actions.WriteLine(String.Format("{0}: {1} is now online on shard {2}.", MethodBase.GetCurrentMethod().Name, Actions.FormatGuild(guild), Variables.Client.GetShardFor(guild).ShardId));

			if (!Variables.Guilds.ContainsKey(guild.Id))
			{
				Variables.TotalUsers += guild.MemberCount;
				Variables.TotalGuilds++;

				//Making sure the guild is not already in the botguildinfo list
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
			}

			return Task.CompletedTask;
		}

		public static Task OnGuildUnavailable(SocketGuild guild)
		{
			Actions.WriteLine(String.Format("{0}: Guild is now offline {1}.", MethodBase.GetCurrentMethod().Name, Actions.FormatGuild(guild)));

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
			Actions.WriteLine(String.Format("{0}: Bot has joined {1}.", MethodBase.GetCurrentMethod().Name, Actions.FormatGuild(guild)));

			//Check how many bots are in the guild
			int botCount = 0;
			guild.Users.ToList().ForEach(x =>
			{
				if (x.IsBot)
				{
					++botCount;
				}
			});

			//Get the number of users in the guild
			var users = guild.MemberCount;
			//Determine what percentage of bot users to leave at
			double percentage;
			if (users <= 10)
			{
				//Allows up to 7 bots
				percentage = .7;
			}
			else if (users <= 25)
			{
				//Allows up to 12 bots
				percentage = .5;
			}
			else if (users <= 50)
			{
				//Allows up to 17 bots
				percentage = .3;
			}
			else if (users <= 100)
			{
				//Allows up to 25 bots
				percentage = .25;
			}
			else
			{
				percentage = .15;
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
			if (guilds + 100 >= shards * 2500)
			{
				Actions.WriteLine(String.Format("The bot currently has {0} out of {1} possible spots for servers filled. Please increase the shard count.", guilds, shards * 2500));
			}
			//Leave the guild
			if (guilds > shards * 2500)
			{
				Task.Run(async () =>
				{
					await guild.LeaveAsync();
				});
				//Send a message to the console
				Actions.WriteLine(String.Format("Left the guild {0} due to having too many guilds on the client and not enough shards.", Actions.FormatGuild(guild)));
			}

			return Task.CompletedTask;
		}

		public static Task OnLeftGuild(SocketGuild guild)
		{
			Actions.WriteLine(String.Format("{0}: Bot has left {1}.", MethodBase.GetCurrentMethod().Name, Actions.FormatGuild(guild)));

			Variables.TotalUsers -= (guild.MemberCount + 1);
			Variables.TotalGuilds--;

			return Task.CompletedTask;
		}
	}

	public class Server_Logs : ModuleBase
	{
		//TODO: Remove most of the events that will get replaced by the audit log
		public static async Task OnUserJoined(SocketGuildUser user)
		{
			++Variables.TotalUsers;

			if (!Variables.Guilds.TryGetValue(user.Guild.Id, out BotGuildInfo guildInfo))
				return;
			var guild = Actions.VerifyGuild(user, LogActions.UserJoined);
			if (guild == null)
				return;
			var serverLog = guildInfo.ServerLog;
			if (serverLog == null)
				return;

			//Slowmode
			if (guildInfo.SlowmodeGuild != null || (await guild.GetTextChannelsAsync()).Select(x => x.Id).Intersect(guildInfo.SlowmodeChannels.Select(x => x.ChannelID)).Any())
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
			//Welcome message
			await Actions.SendGuildNotification(user, guildInfo.WelcomeMessage);
			//Invite string
			var curInv = await Actions.GetInviteUserJoinedOn(guild);
			var inviteString = curInv != null ? String.Format("\n**Invite:** {0}", curInv.Code) : "";
			//Check if the user is a new account
			var userAccAge = (int)(DateTime.UtcNow - user.CreatedAt.ToUniversalTime()).TotalHours;
			var ageWarningString = userAccAge <= 24 ? String.Format("\n**New Account:** {0} hours old", userAccAge) : "";

			if (user.IsBot)
			{
				var embed = Actions.MakeNewEmbed(null, String.Format("**ID:** {0}{1}{2}", user.Id, inviteString, ageWarningString), Constants.JOIN);
				Actions.AddFooter(embed, "Bot Joined");
				Actions.AddAuthor(embed, Actions.FormatUser(user), user.GetAvatarUrl());
				await Actions.SendEmbedMessage(serverLog, embed);
			}
			else
			{
				var embed = Actions.MakeNewEmbed(null, String.Format("**ID:** {0}{1}{2}", user.Id, inviteString, ageWarningString), Constants.JOIN);
				Actions.AddFooter(embed, "User Joined");
				Actions.AddAuthor(embed, Actions.FormatUser(user), user.GetAvatarUrl());
				await Actions.SendEmbedMessage(serverLog, embed);
			}

			++Variables.LoggedJoins;
		}

		public static async Task OnUserLeft(SocketGuildUser user)
		{
			--Variables.TotalUsers;

			var guild = Actions.VerifyGuild(user, LogActions.UserLeft);
			if (guild == null)
				return;
			if (!Variables.Guilds.TryGetValue(guild.Id, out BotGuildInfo guildInfo))
				return;
			var serverLog = guildInfo.ServerLog;
			if (serverLog == null)
				return;

			//Check if the bot was the one that left
			if (user == user.Guild.GetUser(Variables.Bot_ID))
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
				Actions.AddAuthor(embed, Actions.FormatUser(user), user.GetAvatarUrl());
				await Actions.SendEmbedMessage(serverLog, embed);
			}
			else
			{
				var embed = Actions.MakeNewEmbed(null, String.Format("**ID:** {0}{1}", user.Id, lengthStayed), Constants.LEAV);
				Actions.AddFooter(embed, "User Left");
				Actions.AddAuthor(embed, Actions.FormatUser(user), user.GetAvatarUrl());
				await Actions.SendEmbedMessage(serverLog, embed);
			}

			++Variables.LoggedLeaves;
		}

		public static async Task OnUserUnbanned(SocketUser user, SocketGuild inputGuild)
		{
			var guild = Actions.VerifyGuild(inputGuild, LogActions.UserUnbanned);
			if (guild == null)
				return;
			if (!Variables.Guilds.TryGetValue(guild.Id, out BotGuildInfo guildInfo))
				return;
			var serverLog = guildInfo.ServerLog;
			if (serverLog == null)
				return;

			var embed = Actions.MakeNewEmbed(null, "**ID:** " + user.Id, Constants.UNBN);
			Actions.AddFooter(embed, "User Unbanned");
			Actions.AddAuthor(embed, Actions.FormatUser(user), user.GetAvatarUrl());
			await Actions.SendEmbedMessage(serverLog, embed);

			++Variables.LoggedUnbans;
		}

		public static async Task OnUserBanned(SocketUser user, SocketGuild inputGuild)
		{
			//Check if the bot was the one banned
			if (user == inputGuild.GetUser(Variables.Bot_ID))
			{
				Variables.Guilds.Remove(inputGuild.Id);
				return;
			}

			var guild = Actions.VerifyGuild(inputGuild, LogActions.UserBanned);
			if (guild == null)
				return;
			if (!Variables.Guilds.TryGetValue(guild.Id, out BotGuildInfo guildInfo))
				return;
			var serverLog = guildInfo.ServerLog;
			if (serverLog == null)
				return;

			var embed = Actions.MakeNewEmbed(null, "**ID:** " + user.Id, Constants.BANN);
			Actions.AddFooter(embed, "User Banned");
			Actions.AddAuthor(embed, Actions.FormatUser(user), user.GetAvatarUrl());
			await Actions.SendEmbedMessage(serverLog, embed);

			++Variables.LoggedBans;
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
					var guild = Actions.VerifyGuild(inputGuild, LogActions.UserUpdated);
					if (guild == null)
						return;
					if (!Variables.Guilds.TryGetValue(guild.Id, out BotGuildInfo guildInfo))
						return;
					var serverLog = guildInfo.ServerLog;
					if (serverLog == null)
						return;

					var embed = Actions.MakeNewEmbed(null, null, Constants.UEDT);
					Actions.AddFooter(embed, "Name Changed");
					Actions.AddField(embed, "Before:", "`" + beforeUser.Username + "`");
					Actions.AddField(embed, "After:", "`" + afterUser.Username + "`", false);
					Actions.AddAuthor(embed, Actions.FormatUser(afterUser), afterUser.GetAvatarUrl());
					await Actions.SendEmbedMessage(serverLog, embed);

					++Variables.LoggedUserChanges;
				});
			}
		}

		public static async Task OnGuildMemberUpdated(SocketGuildUser beforeUser, SocketGuildUser afterUser)
		{
			var guild = Actions.VerifyGuild(afterUser, LogActions.GuildMemberUpdated);
			if (guild == null)
				return;
			if (!Variables.Guilds.TryGetValue(guild.Id, out BotGuildInfo guildInfo))
				return;
			var serverLog = guildInfo.ServerLog;
			if (serverLog == null)
				return;

			//Nickname change
			if (beforeUser.Nickname != afterUser.Nickname)
			{
				//Format the nicknames
				var originalNickname = String.IsNullOrWhiteSpace(beforeUser.Nickname) ? Constants.NO_NN : beforeUser.Nickname;
				var newNickname = String.IsNullOrWhiteSpace(afterUser.Nickname) ? Constants.NO_NN : afterUser.Nickname;

				if (guildInfo.FAWRNicknames.Contains(newNickname))
					return;

				//These ones are across more lines than the previous ones up above because it makes it easier to remember what is doing what
				var embed = Actions.MakeNewEmbed(null, null, Constants.UEDT);
				Actions.AddFooter(embed, "Nickname Changed");
				Actions.AddField(embed, "Before:", "`" + originalNickname + "`");
				Actions.AddField(embed, "After:", "`" + newNickname + "`", false);
				Actions.AddAuthor(embed, Actions.FormatUser(afterUser), afterUser.GetAvatarUrl());
				await Actions.SendEmbedMessage(serverLog, embed);

				++Variables.LoggedUserChanges;
			}

			//Role change
			var firstNotSecond = beforeUser.Roles.Except(afterUser.Roles).ToList();
			var secondNotFirst = afterUser.Roles.Except(beforeUser.Roles).ToList();
			var rolesChange = new List<string>();
			if (firstNotSecond.Any())
			{
				//Get or create the roleloss
				lock (guildInfo.RoleLoss)
				{
					guildInfo.RoleLoss.AddToList(afterUser);
				}

				//Use a token so the messages do not get sent prematurely
				var cancelToken = guildInfo.RoleLoss.CancelToken;
				if (cancelToken != null)
				{
					cancelToken.Cancel();
				}
				cancelToken = new CancellationTokenSource();
				guildInfo.RoleLoss.SetCancelToken(cancelToken);


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

					//Make a copy of the list so they can be removed from the old one
					List<IGuildUser> users;
					lock (guildInfo.RoleLoss)
					{
						users = new List<IGuildUser>(guildInfo.RoleLoss.GetList().Select(x => x as IGuildUser));
						guildInfo.RoleLoss.ClearList();
					}

					firstNotSecond.ForEach(x =>
					{
						//Ignore deleted roles
						if (Variables.DeletedRoles.Contains(x.Id))
							return;
						rolesChange.Add(x.Name);
					});

					//If no roles then return so as to not send a blank message
					if (!rolesChange.Any())
						return;

					var embed = Actions.MakeNewEmbed(null, String.Format("**Role{0} Lost:** {1}", rolesChange.Count != 1 ? "s" : "", String.Join(", ", rolesChange)), Constants.UEDT);
					Actions.AddFooter(embed, "Role Lost");
					Actions.AddAuthor(embed, Actions.FormatUser(afterUser), afterUser.GetAvatarUrl());
					await Actions.SendEmbedMessage(serverLog, embed);

					++Variables.LoggedUserChanges;
				});
			}
			else if (secondNotFirst.Any())
			{
				//Not necessary to have in a separate task like the method above
				secondNotFirst.ForEach(x =>
				{
					if (!guildInfo.FAWRRoles.Contains(x))
					{
						rolesChange.Add(x.Name);
					}
				});

				if (!rolesChange.Any())
					return;

				var embed = Actions.MakeNewEmbed(null, String.Format("**Role{0} Gained:** {1}", rolesChange.Count != 1 ? "s" : "", String.Join(", ", rolesChange)), Constants.UEDT);
				Actions.AddFooter(embed, "Role Gained");
				Actions.AddAuthor(embed, Actions.FormatUser(afterUser), afterUser.GetAvatarUrl());
				await Actions.SendEmbedMessage(serverLog, embed);

				++Variables.LoggedUserChanges;
			}
		}

		public static async Task OnMessageReceived(SocketMessage message)
		{
			var guild = Actions.GetGuildFromMessage(message);
			if (guild == null)
			{
				//Check if the user is trying to become the bot owner by DMing the bot is key
				await Message_Received_Actions.BotOwner(message);
				return;
			}
			else if (message.Author.IsWebhook)
				return;

			if (Variables.Guilds.TryGetValue(guild.Id, out BotGuildInfo guildInfo))
			{
				await Message_Received_Actions.ModifyPreferences(guildInfo, guild, message);
				await Message_Received_Actions.CloseWords(guildInfo, guild, message);
				await Message_Received_Actions.SpamPrevention(guildInfo, guild, message);
				await Message_Received_Actions.VotingOnSpamPrevention(guildInfo, guild, message);
				await Message_Received_Actions.SlowmodeOrBannedPhrases(guildInfo, guild, message);

				var serverLog = guildInfo.ServerLog;
				await Message_Received_Actions.ImageLog(guildInfo, serverLog, message);

				++Variables.LoggedMessages;
			}
		}
		
		public static async Task OnMessageUpdated(Cacheable<IMessage, ulong> beforeMessage, SocketMessage afterMessage, ISocketMessageChannel channel)
		{
			//Get the before message's value
			var beforeMessageValue = beforeMessage.HasValue ? beforeMessage.Value : null;
			//Check if the updated message has any banned phrases and should be deleted
			await Actions.BannedPhrases(afterMessage);

			var guild = Actions.VerifyGuild(afterMessage, LogActions.MessageUpdated);
			if (guild == null)
				return;
			if (!Variables.Guilds.TryGetValue(guild.Id, out BotGuildInfo guildInfo))
				return;
			var serverLog = guildInfo.ServerLog;
			if (serverLog == null)
				return;

			//If the before message is not specified always take that as it should be logged. If the embed counts are greater take that as logging too.
			if (!beforeMessage.HasValue || beforeMessageValue.Embeds.Count() < afterMessage.Embeds.Count())
				await Message_Received_Actions.ImageLog(guildInfo, serverLog, afterMessage);
			
			var beforeMsgContent = Actions.ReplaceMarkdownChars(beforeMessageValue?.Content ?? "");
			var afterMsgContent = Actions.ReplaceMarkdownChars(afterMessage.Content);
			beforeMsgContent = String.IsNullOrWhiteSpace(beforeMsgContent) ? "Empty or unable to be gotten." : beforeMsgContent;
			afterMsgContent = String.IsNullOrWhiteSpace(afterMsgContent) ? "Empty or unable to be gotten." : afterMsgContent;

			if (beforeMsgContent.Equals(afterMsgContent))
				return;
			if (beforeMsgContent.Length + afterMsgContent.Length > Constants.MAX_MESSAGE_LENGTH_LONG)
			{
				beforeMsgContent = beforeMsgContent.Length > 667 ? "LONG MESSAGE" : beforeMsgContent;
				afterMsgContent = afterMsgContent.Length > 667 ? "LONG MESSAGE" : afterMsgContent;
			}

			var embed = Actions.MakeNewEmbed(null, null, Constants.MEDT);
			Actions.AddFooter(embed, "Message Updated");
			Actions.AddField(embed, "Before:", "`" + beforeMsgContent + "`");
			Actions.AddField(embed, "After:", "`" + afterMsgContent + "`", false);
			Actions.AddAuthor(embed, String.Format("{0} in #{1}", Actions.FormatUser(afterMessage.Author), afterMessage.Channel), afterMessage.Author.GetAvatarUrl());
			await Actions.SendEmbedMessage(serverLog, embed);

			++Variables.LoggedEdits;
		}

		public static async Task OnMessageDeleted(Cacheable<IMessage, ulong> message, ISocketMessageChannel channel)
		{
			//Get the message's value
			var messageValue = message.HasValue ? message.Value : null;

			var guild = Actions.VerifyGuild(messageValue, LogActions.MessageDeleted);
			if (guild == null)
				return;
			if (!Variables.Guilds.TryGetValue(guild.Id, out BotGuildInfo guildInfo))
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

		public static async Task OnRoleCreated(SocketRole role)
		{
			var guild = Actions.VerifyGuild(role, LogActions.RoleCreated);
			if (guild == null)
				return;
			if (!Variables.Guilds.TryGetValue(guild.Id, out BotGuildInfo guildInfo))
				return;
			var serverLog = guildInfo.ServerLog;
			if (serverLog == null)
				return;

			var embed = Actions.MakeNewEmbed("Role Created", String.Format("Name: `{0}`\nID: `{1}`", role.Name, role.Id), Constants.CCRE);
			Actions.AddFooter(embed, "Role Created");
			await Actions.SendEmbedMessage(serverLog, embed);
		}

		public static async Task OnRoleUpdated(SocketRole beforeRole, SocketRole afterRole)
		{
			var guild = Actions.VerifyGuild(afterRole, LogActions.RoleUpdated);
			if (guild == null)
				return;
			if (!Variables.Guilds.TryGetValue(guild.Id, out BotGuildInfo guildInfo))
				return;
			var serverLog = guildInfo.ServerLog;
			if (serverLog == null)
				return;

			//Make sure the role's name is not the same
			if (!Actions.CaseInsEquals(beforeRole.Name, afterRole.Name))
			{
				var embed = Actions.MakeNewEmbed("Role Name Changed", null, Constants.REDT);
				Actions.AddFooter(embed, "Role Name Changed");
				Actions.AddField(embed, "Before:", "`" + beforeRole.Name + "`");
				Actions.AddField(embed, "After:", "`" + afterRole.Name + "`", false);
				await Actions.SendEmbedMessage(serverLog, embed);
			}
		}

		public static async Task OnRoleDeleted(SocketRole role)
		{
			//Add this to prevent massive spam fests when a role is deleted
			Variables.DeletedRoles.Add(role.Id);

			var guild = Actions.VerifyGuild(role, LogActions.RoleDeleted);
			if (guild == null)
				return;
			if (!Variables.Guilds.TryGetValue(guild.Id, out BotGuildInfo guildInfo))
				return;
			var serverLog = guildInfo.ServerLog;
			if (serverLog == null)
				return;

			var embed = Actions.MakeNewEmbed("Role Deleted", String.Format("Name: `{0}`\nID: `{1}`", role.Name, role.Id), Constants.CCRE);
			Actions.AddFooter(embed, "Role Deleted");
			Actions.WriteLine("role deleted: " + role.Name);
			await Actions.SendEmbedMessage(serverLog, embed);
		}

		public static async Task OnChannelCreated(SocketChannel channel)
		{
			var chan = channel as IGuildChannel;

			var guild = Actions.VerifyGuild(chan, LogActions.ChannelCreated);
			if (guild == null)
				return;
			if (!Variables.Guilds.TryGetValue(guild.Id, out BotGuildInfo guildInfo))
				return;
			var serverLog = guildInfo.ServerLog;
			if (serverLog == null)
				return;

			var embed = Actions.MakeNewEmbed("Channel Created", String.Format("Name: `{0}`\nID: `{1}`", chan.Name, chan.Id), Constants.CCRE);
			Actions.AddFooter(embed, "Channel Created");
			await Actions.SendEmbedMessage(serverLog, embed);
		}

		public static async Task OnChannelUpdated(SocketChannel beforeChannel, SocketChannel afterChannel)
		{
			var bChan = beforeChannel as IGuildChannel;
			var aChan = afterChannel as IGuildChannel;

			var guild = Actions.VerifyGuild(aChan, LogActions.ChannelUpdated);
			if (guild == null)
				return;
			if (!Variables.Guilds.TryGetValue(guild.Id, out BotGuildInfo guildInfo))
				return;
			var serverLog = guildInfo.ServerLog;
			if (serverLog == null)
				return;

			//Check if the name is the bot channel name
			if (!Actions.CaseInsEquals(aChan.Name, bChan.Name))
			{
				var embed = Actions.MakeNewEmbed("Channel Name Changed", null, Constants.CEDT);
				Actions.AddFooter(embed, "Channel Name Changed");
				Actions.AddField(embed, "Before:", "`" + bChan.Name + "`");
				Actions.AddField(embed, "After:", "`" + aChan.Name + "`", false);
				await Actions.SendEmbedMessage(serverLog, embed);
			}			
		}

		public static async Task OnChannelDeleted(SocketChannel channel)
		{
			var chan = channel as IGuildChannel;

			var guild = Actions.VerifyGuild(chan, LogActions.ChannelDeleted);
			if (guild == null)
				return;
			if (!Variables.Guilds.TryGetValue(guild.Id, out BotGuildInfo guildInfo))
				return;
			var serverLog = guildInfo.ServerLog;
			if (serverLog == null)
				return;

			var embed = Actions.MakeNewEmbed("Channel Deleted", String.Format("Name: `{0}`\nID: `{1}`", chan.Name, chan.Id), Constants.CDEL);
			Actions.AddFooter(embed, "Channel Deleted");
			await Actions.SendEmbedMessage(serverLog, embed);
		}
	}

	public class Mod_Logs : ModuleBase
	{
		public static async Task LogCommand(CommandContext context)
		{
			//Write into the console what the command was and who said it
			Actions.WriteLine(String.Format("'{0}' on {1}: \'{2}\'", Actions.FormatUser(context.User), Actions.FormatGuild(context.Guild), context.Message.Content));

			//Get the guild and log channel
			var guild = Actions.VerifyGuild(context.Message, LogActions.CommandLog);
			if (guild == null)
				return;
			var modLog = Variables.Guilds.ContainsKey(guild.Id) ? Variables.Guilds[guild.Id].ModLog : null;
			if (modLog == null)
				return;

			//Make the embed
			var embed = Actions.MakeNewEmbed(description: context.Message.Content);
			Actions.AddFooter(embed, "Mod Log");
			Actions.AddAuthor(embed, String.Format("{0} in #{1}", Actions.FormatUser(context.User), context.Channel.Name), context.User.GetAvatarUrl());
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
			if (false
				|| logChannel == null
				|| message.Author.IsBot
				|| message.Author.IsWebhook
				|| guildInfo.IgnoredLogChannels.Contains(message.Channel.Id)
				|| !guildInfo.LogActions.Contains(LogActions.ImageLog))
				return;

			if (message.Attachments.Any())
			{
				await Actions.ImageLog(logChannel, message, false);
			}
			if (message.Embeds.Any())
			{
				await Actions.ImageLog(logChannel, message, true);
			}
		}

		public static async Task BotOwner(IMessage message)
		{
			//See if they're on the list to be a potential bot owner
			if (Variables.PotentialBotOwners.Contains(message.Author.Id))
			{
				//If the key they input is the same as the bots key then they become owner
				if (message.Content.Trim().Equals(Properties.Settings.Default.BotKey))
				{
					Properties.Settings.Default.BotOwner = message.Author.Id;
					Properties.Settings.Default.Save();
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
					Variables.ActiveCloseWords.Remove(closeWordList);
					await Actions.SendChannelMessage(message.Channel, remind.Text);
					await Actions.DeleteMessage(message);
				}
				var closeHelpList = Variables.ActiveCloseHelp.FirstOrDefault(x => x.User == message.Author as IGuildUser);
				if (closeHelpList.User != null && closeHelpList.List.Count > number)
				{
					var help = closeHelpList.List[number].Help;
					Variables.ActiveCloseHelp.Remove(closeHelpList);
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

			//Check if the guild has slowmode enabled currently
			if (guildInfo.SlowmodeGuild != null || guildInfo.SlowmodeChannels.Any(x => x.ChannelID == message.Channel.Id))
			{
				await Actions.Slowmode(message);
			}
			//Check if any banned phrases
			else if (guildInfo.BannedPhrases.Strings.Any() || guildInfo.BannedPhrases.Regex.Any())
			{
				await Actions.BannedPhrases(message);
			}
		}

		public static async Task SpamPrevention(BotGuildInfo guildInfo, IGuild guild, IMessage msg)
		{
			var author = msg.Author as IGuildUser;
			if (Actions.GetPosition(guild, author) >= Actions.GetPosition(guild, await guild.GetUserAsync(Variables.Bot_ID)))
				return;

			var global = guildInfo.GlobalSpamPrevention;
			var isSpam = false;

			var message = global.MessageSpamPrevention;
			if (Actions.SpamCheck(message, msg))
			{
				isSpam = isSpam || await Actions.HandleSpamPrevention(global, message, guild, author, msg);
			}
			var longmessage = global.LongMessageSpamPrevention;
			if (Actions.SpamCheck(longmessage, msg))
			{
				isSpam = isSpam || await Actions.HandleSpamPrevention(global, longmessage, guild, author, msg);
			}
			var link = global.LinkSpamPrevention;
			if (Actions.SpamCheck(link, msg))
			{
				isSpam = isSpam || await Actions.HandleSpamPrevention(global, link, guild, author, msg);
			}
			var image = global.ImageSpamPrevention;
			if (Actions.SpamCheck(image, msg))
			{
				isSpam = isSpam || await Actions.HandleSpamPrevention(global, image, guild, author, msg);
			}
			var mention = global.MentionSpamPrevention;
			if (Actions.SpamCheck(mention, msg))
			{
				isSpam = isSpam || await Actions.HandleSpamPrevention(global, mention, guild, author, msg);
			}

			if (!isSpam)
				return;

			await Actions.DeleteMessage(msg);

			var spUser = Variables.Guilds[guild.Id].GlobalSpamPrevention.SpamPreventionUsers.FirstOrDefault(x => x.User == author);
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
				if (Actions.GetPosition(guild, x.User) >= Actions.GetPosition(guild, await guild.GetUserAsync(Variables.Bot_ID)) || x.VotesToKick < x.VotesRequired)
					return;
				//Check if they've already been kicked to determine if they should be banned or kicked
				await (x.AlreadyKicked ? guild.AddBanAsync(x.User, 1) : x.User.KickAsync());
				//Reset their current spam count and the people who have already voted on them so they don't get destroyed instantly if they join back
				x.ResetSpamUser();
			});
		}
	}
}