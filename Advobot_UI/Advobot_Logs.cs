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
	public class BotLogs : ModuleBase
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
				//Put the guild into a list
				Variables.Guilds.Add(guild.Id, new BotGuildInfo(guild));
				//Put the invites into a list holding mainly for usage checking
				Task.Run(async () =>
				{
					//Get all of the invites and add their guildID, code, and current uses to the usage check list
					Variables.Guilds[guild.Id].Invites.NewList((await guild.GetInvitesAsync()).ToList().Select(x => new BotInvite(x.GuildId, x.Code, x.Uses)).ToList());
				});

				//Incrementing
				Variables.TotalUsers += guild.MemberCount;
				Variables.TotalGuilds++;

				//Loading everything
				if (Variables.Bot_ID != 0)
				{
					Task.Run(async () =>
					{
						await Actions.LoadGuild(guild);
					});
				}
				else
				{
					Variables.GuildsToBeLoaded.Add(guild);
				}
			}

			//Check if the bot's the only one in the guild
			if (guild.MemberCount == 1)
			{
				//Delete it
				Task.Run(async () =>
				{
					await guild.DeleteAsync();
				});
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

	public class ServerLogs : ModuleBase
	{
		//TODO: Remove most of the events that will get replaced by the audit log
		public static async Task OnUserJoined(SocketGuildUser user)
		{
			//Increment the user total user count
			++Variables.TotalUsers;
			//Get the guild and log channel
			var guild = Actions.VerifyGuild(user, LogActions.UserJoined);
			if (guild == null)
				return;
			var serverLog = Variables.Guilds.ContainsKey(guild.Id) ? Variables.Guilds[guild.Id].ServerLog : null;
			if (serverLog == null)
				return;

			//Check if should add them to a slowmode for channel/guild
			if (Variables.SlowmodeGuilds.ContainsKey(guild.Id) || (await guild.GetTextChannelsAsync()).Select(x => x.Id).Intersect(Variables.SlowmodeChannels.Keys).Any())
			{
				//Add them to the slowmode user list
				await Actions.AddSlowmodeUser(user);
			}
			var antiRaid = Variables.Guilds[guild.Id].AntiRaid;
			if (antiRaid != null)
			{
				//Give them the mute role
				await user.AddRolesAsync(antiRaid.MuteRole);
				//Add them to the list of users who have been muted
				antiRaid.AddUserToMutedList(user);
			}

			//Invite string
			var curInv = await Actions.GetInviteUserJoinedOn(guild);
			var inviteString = curInv != null ? String.Format("\n**Invite:** {0}", curInv.Code) : "";

			//Check if the user is a new account
			var userAccAge = (int)(DateTime.UtcNow - user.CreatedAt.ToUniversalTime()).TotalHours;
			var ageWarningString = userAccAge <= 24 ? String.Format("\n**New Account:** {0} hours old", userAccAge) : "";

			//Make the embed
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

			//Increment the logged joins
			++Variables.LoggedJoins;
		}

		public static async Task OnUserLeft(SocketGuildUser user)
		{
			//Decrease the total users count
			--Variables.TotalUsers;
			//Get the guild and log channel
			var guild = Actions.VerifyGuild(user, LogActions.UserLeft);
			if (guild == null)
				return;
			var serverLog = Variables.Guilds.ContainsKey(guild.Id) ? Variables.Guilds[guild.Id].ServerLog : null;
			if (serverLog == null)
				return;

			//Check if the bot was the one that left
			if (user == user.Guild.GetUser(Variables.Bot_ID))
			{
				Variables.Guilds.Remove(user.Guild.Id);
				return;
			}
			//Check if the bot's the only user left in the guild
			else if (user.Guild.MemberCount == 1)
			{
				//Delete it
				await user.Guild.DeleteAsync();
			}

			//Form the length stayed string
			var lengthStayed = "";
			if (user.JoinedAt.HasValue)
			{
				var time = DateTime.UtcNow.Subtract(user.JoinedAt.Value.UtcDateTime);
				lengthStayed = String.Format("\n**Stayed for:** {0}:{1:00}:{2:00}:{3:00}", time.Days, time.Hours, time.Minutes, time.Seconds);
			}

			//Make the embed
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

			//Increment the leaves count
			++Variables.LoggedLeaves;
		}

		public static async Task OnUserUnbanned(SocketUser user, SocketGuild inputGuild)
		{
			//Get the guild and log channel
			var guild = Actions.VerifyGuild(inputGuild, LogActions.UserUnbanned);
			var serverLog = Variables.Guilds.ContainsKey(guild.Id) ? Variables.Guilds[guild.Id].ServerLog : null;
			if (guild == null || serverLog == null)
				return;

			//Make the embed
			var embed = Actions.MakeNewEmbed(null, "**ID:** " + user.Id, Constants.UNBN);
			Actions.AddFooter(embed, "User Unbanned");
			Actions.AddAuthor(embed, Actions.FormatUser(user), user.GetAvatarUrl());
			await Actions.SendEmbedMessage(serverLog, embed);

			//Increment the unban count
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
			//Get the guild and log channel
			var guild = Actions.VerifyGuild(inputGuild, LogActions.UserBanned);
			if (guild == null)
				return;
			var serverLog = Variables.Guilds.ContainsKey(guild.Id) ? Variables.Guilds[guild.Id].ServerLog : null;
			if (serverLog == null)
				return;

			//Make the embed
			var embed = Actions.MakeNewEmbed(null, "**ID:** " + user.Id, Constants.BANN);
			Actions.AddFooter(embed, "User Banned");
			Actions.AddAuthor(embed, Actions.FormatUser(user), user.GetAvatarUrl());
			await Actions.SendEmbedMessage(serverLog, embed);

			//Increment the ban count
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
					//Get the guild and log channel
					var guild = Actions.VerifyGuild(inputGuild, LogActions.UserUpdated);
					if (guild == null)
						return;
					var serverLog = Variables.Guilds.ContainsKey(guild.Id) ? Variables.Guilds[guild.Id].ServerLog : null;
					if (serverLog == null)
						return;

					//Make the embed
					var embed = Actions.MakeNewEmbed(null, null, Constants.UEDT);
					Actions.AddFooter(embed, "Name Changed");
					Actions.AddField(embed, "Before:", "`" + beforeUser.Username + "`");
					Actions.AddField(embed, "After:", "`" + afterUser.Username + "`", false);
					Actions.AddAuthor(embed, Actions.FormatUser(afterUser), afterUser.GetAvatarUrl());
					await Actions.SendEmbedMessage(serverLog, embed);

					//Increment the logged user changed counter
					++Variables.LoggedUserChanges;
				});
			}
		}

		public static async Task OnGuildMemberUpdated(SocketGuildUser beforeUser, SocketGuildUser afterUser)
		{
			//Get the guild and log channel
			var guild = Actions.VerifyGuild(afterUser, LogActions.GuildMemberUpdated);
			if (guild == null)
				return;
			var serverLog = Variables.Guilds.ContainsKey(guild.Id) ? Variables.Guilds[guild.Id].ServerLog : null;
			if (serverLog == null)
				return;

			//Nickname change
			if (beforeUser.Nickname != afterUser.Nickname)
			{
				//Format the nicknames
				var originalNickname = String.IsNullOrWhiteSpace(beforeUser.Nickname) ? "NO NICKNAME" : beforeUser.Nickname;
				var newNickname = String.IsNullOrWhiteSpace(afterUser.Nickname) ? "NO NICKNAME" : afterUser.Nickname;

				//These ones are across more lines than the previous ones up above because it makes it easier to remember what is doing what
				var embed = Actions.MakeNewEmbed(null, null, Constants.UEDT);
				Actions.AddFooter(embed, "Nickname Changed");
				Actions.AddField(embed, "Before:", "`" + originalNickname + "`");
				Actions.AddField(embed, "After:", "`" + newNickname + "`", false);
				Actions.AddAuthor(embed, Actions.FormatUser(afterUser), afterUser.GetAvatarUrl());
				await Actions.SendEmbedMessage(serverLog, embed);
			}

			//Role change
			var firstNotSecond = beforeUser.Roles.Except(afterUser.Roles).ToList();
			var secondNotFirst = afterUser.Roles.Except(beforeUser.Roles).ToList();
			var rolesChange = new List<string>();
			if (firstNotSecond.Any())
			{
				//Get the info stored for that guild on the bot
				if (!Variables.Guilds.TryGetValue(guild.Id, out BotGuildInfo botInfo))
					return;

				//Get or create the roleloss
				lock (botInfo.RoleLoss)
				{
					botInfo.RoleLoss.AddToList(afterUser);
				}

				//Use a token so the messages do not get sent prematurely
				var cancelToken = botInfo.RoleLoss.CancelToken;
				if (cancelToken != null)
				{
					cancelToken.Cancel();
				}
				cancelToken = new CancellationTokenSource();
				botInfo.RoleLoss.CancelToken = cancelToken;


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
					lock (botInfo.RoleLoss)
					{
						users = new List<IGuildUser>(botInfo.RoleLoss.GetList().Select(x => x as IGuildUser));
						botInfo.RoleLoss.ClearList();
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
				});
			}
			else if (secondNotFirst.Any())
			{
				//Not necessary to have in a separate task like the method above
				secondNotFirst.ForEach(x =>
				{
					rolesChange.Add(x.Name);
				});

				if (!rolesChange.Any())
					return;

				var embed = Actions.MakeNewEmbed(null, String.Format("**Role{0} Gained:** {1}", rolesChange.Count != 1 ? "s" : "", String.Join(", ", rolesChange)), Constants.UEDT);
				Actions.AddFooter(embed, "Role Gained");
				Actions.AddAuthor(embed, Actions.FormatUser(afterUser), afterUser.GetAvatarUrl());
				await Actions.SendEmbedMessage(serverLog, embed);
			}
			//Increment the user changes count
			++Variables.LoggedUserChanges;
		}

		public static async Task OnMessageReceived(SocketMessage message)
		{
			//Get the guild
			var guild = Actions.GetGuildFromMessage(message);
			//Check if the user is trying to become the bot owner by DMing the bot is key
			if (guild == null)
			{
				await MessageReceivedActions.BotOwner(message);
				return;
			}
			//Check if the user should be spam prevented
			await MessageReceivedActions.SpamPrevention(guild, message);
			//Check if the users is voting on a spam prevention
			await MessageReceivedActions.VotingOnSpamPrevention(guild, message);
			//Check if anything to do with deleting/enabling preferences
			await MessageReceivedActions.ModifyPreferences(guild, message);
			//Check if any active closewords
			await MessageReceivedActions.CloseWords(guild, message);
			//Check if slowmode or not banned phrases
			await MessageReceivedActions.SlowmodeOrBannedPhrases(guild, message);

			//Get the guild and log channel
			var serverLog = Variables.Guilds.ContainsKey(guild.Id) ? Variables.Guilds[guild.Id].ServerLog : null;
			//Check if image logging should happen
			await MessageReceivedActions.ImageLog(serverLog, message);

			//Increment the logged messages count
			++Variables.LoggedMessages;
		}
		
		public static async Task OnMessageUpdated(Cacheable<IMessage, ulong> beforeMessage, SocketMessage afterMessage, ISocketMessageChannel channel)
		{
			//Get the before message's value
			var beforeMessageValue = beforeMessage.HasValue ? beforeMessage.Value : null;
			//Check if the updated message has any banned phrases and should be deleted
			await Actions.BannedPhrases(afterMessage);
			//Get the guild and log channel
			var guild = Actions.VerifyGuild(afterMessage, LogActions.MessageUpdated);
			if (guild == null)
				return;
			var serverLog = Variables.Guilds.ContainsKey(guild.Id) ? Variables.Guilds[guild.Id].ServerLog : null;
			if (serverLog == null)
				return;
			//If the before message is not specified always take that as it should be logged. If the embed counts are greater take that as logging too.
			if (!beforeMessage.HasValue || beforeMessageValue.Embeds.Count() < afterMessage.Embeds.Count())
				await MessageReceivedActions.ImageLog(serverLog, afterMessage);

			//Set the content as strings
			var beforeMsgContent = Actions.ReplaceMarkdownChars(beforeMessageValue?.Content ?? "");
			var afterMsgContent = Actions.ReplaceMarkdownChars(afterMessage.Content);
			//Null check
			beforeMsgContent = String.IsNullOrWhiteSpace(beforeMsgContent) ? "Empty or unable to be gotten." : beforeMsgContent;
			afterMsgContent = String.IsNullOrWhiteSpace(afterMsgContent) ? "Empty or unable to be gotten." : afterMsgContent;
			//Return if the messages are the same
			if (beforeMsgContent.Equals(afterMsgContent))
				return;
			//Check lengths
			if (beforeMsgContent.Length + afterMsgContent.Length > Constants.LENGTH_CHECK)
			{
				beforeMsgContent = beforeMsgContent.Length > 667 ? "LONG MESSAGE" : beforeMsgContent;
				afterMsgContent = afterMsgContent.Length > 667 ? "LONG MESSAGE" : afterMsgContent;
			}

			//Make the embed
			var embed = Actions.MakeNewEmbed(null, null, Constants.MEDT);
			Actions.AddFooter(embed, "Message Updated");
			Actions.AddField(embed, "Before:", "`" + beforeMsgContent + "`");
			Actions.AddField(embed, "After:", "`" + afterMsgContent + "`", false);
			Actions.AddAuthor(embed, String.Format("{0} in #{1}", Actions.FormatUser(afterMessage.Author), afterMessage.Channel), afterMessage.Author.GetAvatarUrl());
			await Actions.SendEmbedMessage(serverLog, embed);

			//Increment the edit count
			++Variables.LoggedEdits;
		}

		public static async Task OnMessageDeleted(Cacheable<IMessage, ulong> message, ISocketMessageChannel channel)
		{
			//Get the message's value
			var messageValue = message.HasValue ? message.Value : null;
			//Get the guild and log channel
			var guild = Actions.VerifyGuild(messageValue, LogActions.MessageDeleted);
			if (guild == null)
				return;
			var serverLog = Variables.Guilds.ContainsKey(guild.Id) ? Variables.Guilds[guild.Id].ServerLog : null;
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
			botInfo.MessageDeletion.CancelToken = cancelToken;

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
			//Get the guild and log channel
			var guild = Actions.VerifyGuild(role, LogActions.RoleCreated);
			if (guild == null)
				return;
			var serverLog = Variables.Guilds.ContainsKey(guild.Id) ? Variables.Guilds[guild.Id].ServerLog : null;
			if (serverLog == null)
				return;

			//Make the embed
			var embed = Actions.MakeNewEmbed("Role Created", String.Format("Name: `{0}`\nID: `{1}`", role.Name, role.Id), Constants.CCRE);
			Actions.AddFooter(embed, "Role Created");
			await Actions.SendEmbedMessage(serverLog, embed);
		}

		public static async Task OnRoleUpdated(SocketRole beforeRole, SocketRole afterRole)
		{
			//Get the guild and log channel
			var guild = Actions.VerifyGuild(afterRole, LogActions.RoleUpdated);
			if (guild == null)
				return;
			var serverLog = Variables.Guilds.ContainsKey(guild.Id) ? Variables.Guilds[guild.Id].ServerLog : null;
			if (serverLog == null)
				return;

			//Make sure the role's name is not the same
			if (Actions.CaseInsEquals(beforeRole.Name, afterRole.Name))
				return;

			//Make the embed
			var embed = Actions.MakeNewEmbed("Role Name Changed", null, Constants.REDT);
			Actions.AddFooter(embed, "Role Name Changed");
			Actions.AddField(embed, "Before:", "`" + beforeRole.Name + "`");
			Actions.AddField(embed, "After:", "`" + afterRole.Name + "`", false);
			await Actions.SendEmbedMessage(serverLog, embed);
		}

		public static async Task OnRoleDeleted(SocketRole role)
		{
			//Add this to prevent massive spam fests when a role is deleted
			Variables.DeletedRoles.Add(role.Id);
			//Get the guild and log channel
			var guild = Actions.VerifyGuild(role, LogActions.RoleDeleted);
			if (guild == null)
				return;
			var serverLog = Variables.Guilds.ContainsKey(guild.Id) ? Variables.Guilds[guild.Id].ServerLog : null;
			if (serverLog == null)
				return;

			//Make the embed
			var embed = Actions.MakeNewEmbed("Role Deleted", String.Format("Name: `{0}`\nID: `{1}`", role.Name, role.Id), Constants.CCRE);
			Actions.AddFooter(embed, "Role Deleted");
			Actions.WriteLine("role deleted: " + role.Name);
			await Actions.SendEmbedMessage(serverLog, embed);
		}

		public static async Task OnChannelCreated(SocketChannel channel)
		{
			//Convert the channel to a textchannel
			var chan = channel as IGuildChannel;
			//Get the guild and log channel
			var guild = Actions.VerifyGuild(chan, LogActions.ChannelCreated);
			if (guild == null)
				return;
			var serverLog = Variables.Guilds.ContainsKey(guild.Id) ? Variables.Guilds[guild.Id].ServerLog : null;
			if (serverLog == null)
				return;

			//Make the embed
			var embed = Actions.MakeNewEmbed("Channel Created", String.Format("Name: `{0}`\nID: `{1}`", chan.Name, chan.Id), Constants.CCRE);
			Actions.AddFooter(embed, "Channel Created");
			await Actions.SendEmbedMessage(serverLog, embed);
		}

		public static async Task OnChannelUpdated(SocketChannel beforeChannel, SocketChannel afterChannel)
		{
			//Create a variable of beforechannel and afterchannel as an IGuildChannel for later use
			var bChan = beforeChannel as IGuildChannel;
			var aChan = afterChannel as IGuildChannel;
			//Get the guild and log channel
			var guild = Actions.VerifyGuild(aChan, LogActions.ChannelUpdated);
			if (guild == null)
				return;
			var serverLog = Variables.Guilds.ContainsKey(guild.Id) ? Variables.Guilds[guild.Id].ServerLog : null;
			if (serverLog == null)
				return;

			//Check if the name is the bot channel name
			if (aChan != null && Actions.CaseInsEquals(aChan.Name, Variables.Bot_Channel) && !Actions.CaseInsEquals(aChan.Name, bChan.Name))
			{
				//TODO: Something
			}

			//Check if the before and after name are the same
			if (Actions.CaseInsEquals(aChan.Name, bChan.Name))
				return;

			//Make the embed
			var embed = Actions.MakeNewEmbed("Channel Name Changed", null, Constants.CEDT);
			Actions.AddFooter(embed, "Channel Name Changed");
			Actions.AddField(embed, "Before:", "`" + bChan.Name + "`");
			Actions.AddField(embed, "After:", "`" + aChan.Name + "`", false);
			await Actions.SendEmbedMessage(serverLog, embed);
		}

		public static async Task OnChannelDeleted(SocketChannel channel)
		{
			//Convert the channel to an IGuildChannel
			var chan = channel as IGuildChannel;
			//Get the guild and log channel
			var guild = Actions.VerifyGuild(chan, LogActions.ChannelDeleted);
			if (guild == null)
				return;
			var serverLog = Variables.Guilds.ContainsKey(guild.Id) ? Variables.Guilds[guild.Id].ServerLog : null;
			if (serverLog == null)
				return;

			//Make the embed
			var embed = Actions.MakeNewEmbed("Channel Deleted", String.Format("Name: `{0}`\nID: `{1}`", chan.Name, chan.Id), Constants.CDEL);
			Actions.AddFooter(embed, "Channel Deleted");
			await Actions.SendEmbedMessage(serverLog, embed);
		}
	}

	public class ModLogs : ModuleBase
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

	public class MessageReceivedActions : ModuleBase
	{
		public static async Task ImageLog(ITextChannel channel, IMessage message)
		{
			if (false
				|| channel == null
				|| message.Author.IsBot
				|| Variables.Guilds[channel.GuildId].IgnoredLogChannels.GetList().Contains(channel.Id)
				|| !Variables.Guilds[channel.GuildId].LogActions.GetList().Contains(LogActions.ImageLog))
				return;

			if (message.Attachments.Any())
			{
				await Actions.ImageLog(channel, message, false);
			}
			if (message.Embeds.Any())
			{
				await Actions.ImageLog(channel, message, true);
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

		public static async Task ModifyPreferences(IGuild guild, IMessage message)
		{
			//Check if it's the owner of the guild saying something
			if (message.Author.Id == guild.OwnerId)
			{
				//If the message is only 'yes' then check if they're enabling or deleting preferences
				if (Actions.CaseInsEquals(message.Content, "yes"))
				{
					if (Variables.GuildsEnablingPreferences.Contains(guild))
					{
						//Enable preferences
						await Actions.EnablePreferences(guild, message as IUserMessage);
					}
					if (Variables.GuildsDeletingPreferences.Contains(guild))
					{
						//Delete preferences
						await Actions.DeletePreferences(guild, message as IUserMessage);
					}
				}
			}
		}

		public static async Task CloseWords(IGuild guild, IMessage message)
		{
			if (Constants.CLOSE_WORDS_POSITIONS.Contains(message.Content))
			{
				//Get the number
				var number = Actions.GetInteger(message.Content) - 1;
				var closeWordList = Variables.ActiveCloseWords.FirstOrDefault(x => x.User == message.Author as IGuildUser);
				var closeHelpList = Variables.ActiveCloseHelp.FirstOrDefault(x => x.User == message.Author as IGuildUser);
				if (closeWordList.User != null)
				{
					//Get the remind
					var remind = Variables.Guilds[guild.Id].Reminds.GetList().FirstOrDefault(x => Actions.CaseInsEquals(x.Name, closeWordList.List[number].Name));

					//Send the remind
					await Actions.SendChannelMessage(message.Channel, remind.Text);

					//Remove that list
					Variables.ActiveCloseWords.Remove(closeWordList);
				}
				else if (closeHelpList.User != null)
				{
					//Get the help
					var help = closeHelpList.List[number].Help;

					//Send the remind
					await Actions.SendEmbedMessage(message.Channel, Actions.AddFooter(Actions.MakeNewEmbed(help.Name, Actions.GetHelpString(help)), "Help"));

					//Remove that list
					Variables.ActiveCloseHelp.Remove(closeHelpList);
				}
				else
				{
					return;
				}

				//Delete the message
				await Actions.DeleteMessage(message);
			}
		}

		public static async Task SlowmodeOrBannedPhrases(IGuild guild, IMessage message)
		{
			//Make sure the message is a valid message to do this to
			if (message == null || message.Author.IsBot)
				return;

			//Check if the guild has slowmode enabled currently
			if (Variables.SlowmodeGuilds.ContainsKey(guild.Id) || Variables.SlowmodeChannels.ContainsKey(message.Channel.Id))
			{
				await Actions.Slowmode(message);
			}
			//Check if any banned phrases
			else if (Variables.Guilds[guild.Id].BannedPhrases.GetList().Any() || Variables.Guilds[guild.Id].BannedRegex.GetList().Any())
			{
				await Actions.BannedPhrases(message);
			}
		}

		public static async Task SpamPrevention(IGuild guild, IMessage msg)
		{
			var global = Variables.Guilds[guild.Id].GlobalSpamPrevention;

			//TODO: Add in the checks to determine which spam preventions should go through (like with mentionspam)
			var message = global.MessageSpamPrevention;
			if (Actions.SpamCheck(message, msg) && await Actions.HandleSpamPrevention(global, message, guild, msg))
				return;

			var longmessage = global.LongMessageSpamPrevention;
			if (Actions.SpamCheck(longmessage, msg) && await Actions.HandleSpamPrevention(global, longmessage, guild, msg))
				return;

			var link = global.LinkSpamPrevention;
			if (Actions.SpamCheck(link, msg) && await Actions.HandleSpamPrevention(global, link, guild, msg))
				return;

			var image = global.ImageSpamPrevention;
			if (Actions.SpamCheck(image, msg) && await Actions.HandleSpamPrevention(global, image, guild, msg))
				return;

			var mention = global.MentionSpamPrevention;
			if (Actions.SpamCheck(mention, msg) && await Actions.HandleSpamPrevention(global, mention, guild, msg))
				return;
		}

		public static async Task VotingOnSpamPrevention(IGuild guild, IMessage message)
		{
			//Get the users primed to be kicked/banned by the spam prevention
			var users = Variables.Guilds[guild.Id].GlobalSpamPrevention.SpamPreventionUsers.GetList().Where(x => x.PotentialKick).ToList();
			//Return if it's empty
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