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
		public class LogHolder
		{
			public BotLog BotLog { get; }
			public ServerLog ServerLog { get; }
			public ModLog ModLog { get; }

			public LogHolder()
			{
				BotLog = new BotLog();
				ServerLog = new ServerLog();
				ModLog = new ModLog();
			}

			public void StartLogging(IDiscordClient client, BotGlobalInfo botInfo)
			{
				BotLog.StartLogging(client, botInfo);
				ServerLog.StartLogging(client, botInfo);
				ModLog.StartLogging(client, botInfo);
			}
		}

		public class BaseLog : MyModuleBase
		{
			protected IDiscordClient Client;
			protected BotGlobalInfo BotInfo;

			public void StartLogging(IDiscordClient client, BotGlobalInfo botInfo)
			{
				Client = client;
				BotInfo = botInfo;
			}
		}

		public sealed class BotLog : BaseLog
		{
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
				Messages.WriteLine(String.Format("{0} is now online on shard {1}.", guild.FormatGuild(), Gets.GetShardIdFor((dynamic)Client, guild)));
				Messages.WriteLine(String.Format("Current memory usage is: {0}MB", Gets.GetMemory(BotInfo.Windows).ToString("0.00")));
				Variables.TotalUsers += guild.MemberCount;
				++Variables.TotalGuilds;

				if (!Variables.Guilds.ContainsKey(guild.Id))
				{
					if (Properties.Settings.Default.BotID != 0)
					{
						await SavingAndLoading.CreateOrGetGuildInfo(guild);
					}
					else
					{
						Variables.GuildsToBeLoaded.Add(guild);
					}
				}

				return;
			}

			public Task OnGuildUnavailable(SocketGuild guild)
			{
				Messages.WriteLine(String.Format("Guild is now offline {0}.", guild.FormatGuild()));
				Variables.TotalUsers = Math.Max(0, Variables.TotalUsers - guild.MemberCount);
				Variables.TotalGuilds = Math.Max(0, Variables.TotalGuilds--);

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
				var shards = Gets.GetShardCount((dynamic)Client);
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

				Variables.TotalUsers -= (guild.MemberCount + 1);
				Variables.TotalGuilds--;

				return Task.CompletedTask;
			}
		}

		public sealed class ServerLog : BaseLog
		{
			public async Task OnUserJoined(SocketGuildUser user)
			{
				++Variables.TotalUsers;

				if (LogChannels.VerifyServerLoggingAction(BotInfo, user, LogAction.UserJoined, out VerifiedLoggingAction verified))
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
					if (((List<BannedPhrase>)guildInfo.GetSetting(SettingOnGuild.BannedNamesForJoiningUsers)).Any(x => user.Username.CaseInsContains(x.Phrase)))
					{
						await Users.BotBanUser(guild, user.Id, 1, "banned name");
						return;
					}
					//Welcome message
					else
					{
						await Messages.SendGuildNotification(user, ((GuildNotification)guildInfo.GetSetting(SettingOnGuild.WelcomeMessage)));
					}

					{
						var embed = Embeds.MakeNewEmbed(null, String.Format("**ID:** {0}{1}{2}", user.Id, inviteStr, ageWarningStr), Constants.JOIN);
						Embeds.AddFooter(embed, String.Format("{0} Joined", botOrUserStr));
						Embeds.AddAuthor(embed, user);
						await Messages.SendEmbedMessage(serverLog, embed);
					}

					++Variables.LoggedJoins;
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
				--Variables.TotalUsers;

				//Check if the bot was the one that left
				if (user.Id == Properties.Settings.Default.BotID)
				{
					Variables.Guilds.Remove(user.Guild.Id);
					return;
				}

				if (LogChannels.VerifyServerLoggingAction(BotInfo, user, LogAction.UserLeft, out VerifiedLoggingAction verified))
				{
					var guild = verified.Guild;
					var guildInfo = verified.GuildInfo;
					var serverLog = verified.LoggingChannel;

					//Don't log them to the server if they're someone who was just banned for joining with a banned name
					if (((List<BannedPhrase>)guildInfo.GetSetting(SettingOnGuild.BannedNamesForJoiningUsers)).Any(x => user.Username.CaseInsContains(x.Phrase)))
						return;

					await Messages.SendGuildNotification(user, ((GuildNotification)guildInfo.GetSetting(SettingOnGuild.GoodbyeMessage)));

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

					++Variables.LoggedLeaves;
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

						if (LogChannels.VerifyServerLoggingAction(BotInfo, guild, LogAction.UserLeft, out VerifiedLoggingAction verified))
						{
							var guildInfo = verified.GuildInfo;
							var serverLog = verified.LoggingChannel;

							var embed = Embeds.MakeNewEmbed(null, null, Constants.UEDT);
							Embeds.AddFooter(embed, "Name Changed");
							Embeds.AddField(embed, "Before:", "`" + beforeUser.Username + "`");
							Embeds.AddField(embed, "After:", "`" + afterUser.Username + "`", false);
							Embeds.AddAuthor(embed, afterUser);
							await Messages.SendEmbedMessage(serverLog, embed);

							++Variables.LoggedUserChanges;
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

				var guildInfo = await SavingAndLoading.CreateOrGetGuildInfo(guild);
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

			public async Task OnMessageUpdated(Cacheable<IMessage, ulong> cached, SocketMessage afterMessage, ISocketMessageChannel channel)
			{
				if (LogChannels.VerifyServerLoggingAction(BotInfo, channel, LogAction.MessageUpdated, out VerifiedLoggingAction verified))
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
						++Variables.LoggedEdits;
					}
					var imageLog = ((DiscordObjectWithID<ITextChannel>)guildInfo.GetSetting(SettingOnGuild.ImageLog));
					if (imageLog != null)
					{
						//If the before message is not specified always take that as it should be logged. If the embed counts are greater take that as logging too.
						if (beforeMessage?.Embeds.Count() < afterMessage.Embeds.Count())
						{
							await HandleImageLogging(guildInfo, afterMessage);
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
					if (LogChannels.VerifyMessageShouldBeLogged(guildInfo, afterMessage))
					{
						await Spam.HandleBannedPhrases(guildInfo, guild, afterMessage);
					}
				}
			}

			public Task OnMessageDeleted(Cacheable<IMessage, ulong> cached, ISocketMessageChannel channel)
			{
				if (LogChannels.VerifyServerLoggingAction(BotInfo, channel, LogAction.MessageDeleted, out VerifiedLoggingAction verified))
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

					++Variables.LoggedDeletes;

					//Make a separate task in order to not mess up the other commands
					Misc.DontWaitForResultOfBigUnimportantFunction(null, async () =>
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
				if (message.Content.Equals(Properties.Settings.Default.BotKey) && ((ulong)BotInfo.GetSetting(SettingOnBot.BotOwnerID)) == 0)
				{
					BotInfo.SetSetting(SettingOnBot.BotOwnerID, message.Author.Id);
					await Messages.SendDMMessage(message.Channel as IDMChannel, "Congratulations, you are now the owner of the bot.");
				}
			}

			private async Task HandleChannelSettings(BotGuildInfo guildInfo, IMessage message)
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

			private async Task HandleImageLogging(BotGuildInfo guildInfo, IMessage message)
			{
				var logChannel = ((DiscordObjectWithID<ITextChannel>)guildInfo.GetSetting(SettingOnGuild.ImageLog))?.Object;
				if (logChannel == null || message.Author.Id == Properties.Settings.Default.BotID)
					return;

				if (message.Attachments.Any())
				{
					await LogChannels.LogImage(logChannel, message, false);
				}
				if (message.Embeds.Any())
				{
					await LogChannels.LogImage(logChannel, message, true);
				}
			}

			private async Task HandleCloseWords(BotGuildInfo guildInfo, IMessage message)
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

			private async Task HandleSlowmodeOrBannedPhrases(BotGuildInfo guildInfo, SocketGuild guild, IMessage message)
			{
				var smGuild = ((SlowmodeGuild)guildInfo.GetSetting(SettingOnGuild.SlowmodeGuild));
				var smChan = ((List<SlowmodeChannel>)guildInfo.GetSetting(SettingOnGuild.SlowmodeChannels)).FirstOrDefault(x => x.ChannelID == message.Channel.Id);
				if (smGuild != null || smChan != null)
				{
					await Spam.HandleSlowmode(smGuild, smChan, message);
				}

				var bannedStrings = (List<BannedPhrase>)guildInfo.GetSetting(SettingOnGuild.BannedPhraseStrings);
				var bannedRegex = (List<BannedPhrase>)guildInfo.GetSetting(SettingOnGuild.BannedPhraseRegex);
				if (bannedStrings.Any() || bannedRegex.Any())
				{
					await Spam.HandleBannedPhrases(guildInfo, guild, message);
				}
			}

			private async Task HandleSpamPrevention(BotGuildInfo guildInfo, SocketGuild guild, IMessage message)
			{
				if (Users.GetIfUserCanBeModifiedByUser(Users.GetBot(guild), message.Author))
				{
					await Spam.HandleSpamPrevention(guildInfo, guild, message.Author as IGuildUser, message);
				}
			}

			private async Task HandleSpamPreventionVoting(BotGuildInfo guildInfo, SocketGuild guild, IMessage message)
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
					if (!Users.GetIfUserCanBeModifiedByUser(Users.GetBot(guild), user.User) || user.UsersWhoHaveAlreadyVoted.Count < user.VotesRequired)
						return;

					await user.Punish(guildInfo, guild);

					//Reset their current spam count and the people who have already voted on them so they don't get destroyed instantly if they join back
					user.ResetSpamUser();
				}
			}
		}

		public sealed class ModLog : BaseLog
		{
			public async Task LogCommand(MyCommandContext context)
			{
				Variables.GuildsToldBotDoesntWorkWithoutAdmin.ThreadSafeRemove(context.Guild.Id);
				Messages.WriteLine(new LoggedCommand(context).ToString());
				await Messages.DeleteMessage(context.Message);

				if (LogChannels.VerifyMessageShouldBeLogged(context.GuildInfo, context.Message))
				{
					var modLog = ((DiscordObjectWithID<ITextChannel>)context.GuildInfo.GetSetting(SettingOnGuild.ModLog))?.Object;
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