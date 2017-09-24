using Advobot.Actions;
using Advobot.Actions.Formatting;
using Advobot.Classes;
using Advobot.Interfaces;
using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Advobot.Modules.Log
{
	/// <summary>
	/// Logs certain events.
	/// </summary>
	/// <remarks>
	/// This is probably the second worst part of the bot, right behind the UI. Slightly ahead of saving settings though.
	/// </remarks>
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

		public MyLogModule(IDiscordClient client, IBotSettings botSettings, IGuildSettingsModule guildSettings, ITimersModule timers)
		{
			_Client = client;
			_BotSettings = botSettings;
			_GuildSettings = guildSettings;
			_Timers = timers;

			if (_Client is DiscordSocketClient socketClient)
			{
				HookUpEvents(socketClient);
			}
			else if (_Client is DiscordShardedClient shardedClient)
			{
				HookUpEvents(shardedClient);
			}
			else
			{
				throw new ArgumentException("Invalid client provided. Must be either a DiscordSocketClient or a DiscordShardedClient.");
			}
		}

		private void HookUpEvents(DiscordSocketClient client)
		{
			client.Log						+= OnLogMessageSent;
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
		private void HookUpEvents(DiscordShardedClient client)
		{
			client.Log						+= OnLogMessageSent;
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
			var leftSpacing = new[] { a, s, f }.Max().ToString().Length;

			const string aTitle = "**Attempted:**";
			const string sTitle = "**Successful:**";
			const string fTitle = "**Failed:**";
			var rightSpacing = new[] { aTitle, sTitle, fTitle }.Max(x => x.Length) + 1;

			return new StringBuilder()
				.AppendLineFeed(GeneralFormatting.FormatStringsWithLength(aTitle, a, rightSpacing, leftSpacing))
				.AppendLineFeed(GeneralFormatting.FormatStringsWithLength(sTitle, s, rightSpacing, leftSpacing))
				.AppendLineFeed(GeneralFormatting.FormatStringsWithLength(fTitle, f, rightSpacing, leftSpacing))
				.ToString();
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

			return new StringBuilder()
				.AppendLineFeed(GeneralFormatting.FormatStringsWithLength(jTitle, j, rightSpacing, leftSpacing))
				.AppendLineFeed(GeneralFormatting.FormatStringsWithLength(lTitle, l, rightSpacing, leftSpacing))
				.AppendLineFeed(GeneralFormatting.FormatStringsWithLength(uTitle, u, rightSpacing, leftSpacing))
				.AppendLineFeed(GeneralFormatting.FormatStringsWithLength(eTitle, e, rightSpacing, leftSpacing))
				.AppendLineFeed(GeneralFormatting.FormatStringsWithLength(dTitle, d, rightSpacing, leftSpacing))
				.AppendLineFeed(GeneralFormatting.FormatStringsWithLength(iTitle, i, rightSpacing, leftSpacing))
				.AppendLineFeed(GeneralFormatting.FormatStringsWithLength(gTitle, g, rightSpacing, leftSpacing))
				.AppendLineFeed(GeneralFormatting.FormatStringsWithLength(fTitle, f, rightSpacing, leftSpacing))
				.ToString();
		}

		#region Bot
		/// <summary>
		/// Logs system messages from the Discord .Net library.
		/// </summary>
		/// <param name="msg"></param>
		/// <returns></returns>
		internal Task OnLogMessageSent(LogMessage msg)
		{
			if (!String.IsNullOrWhiteSpace(msg.Message))
			{
				ConsoleActions.WriteLine(msg.Message, msg.Source);
			}
			return Task.CompletedTask;
		}
		/// <summary>
		/// Writes to the console telling that the guild is online. If the guild's settings are not loaded, creates them.
		/// </summary>
		/// <param name="guild"></param>
		/// <returns></returns>
		internal async Task OnGuildAvailable(SocketGuild guild)
		{
			ConsoleActions.WriteLine($"{guild.FormatGuild()} is now online on shard {ClientActions.GetShardIdFor(_Client, guild)}.");
			ConsoleActions.WriteLine($"Current memory usage is: {GetActions.GetMemory().ToString("0.00")}MB.");

			if (!_GuildSettings.ContainsGuild(guild.Id))
			{
				this.AddUsers(guild.MemberCount);
				this.IncrementGuilds();
				await _GuildSettings.GetOrCreateSettings(guild);
			}
		}
		/// <summary>
		/// Writes to the console telling that the guild is offline.
		/// </summary>
		/// <param name="guild"></param>
		/// <returns></returns>
		internal Task OnGuildUnavailable(SocketGuild guild)
		{
			ConsoleActions.WriteLine($"Guild is now offline {guild.FormatGuild()}.");
			return Task.CompletedTask;
		}
		/// <summary>
		/// Writes to the console telling that the guild has added the bot. Leaves if too many bots are in the server. Warns about shard issues.
		/// </summary>
		/// <param name="guild"></param>
		/// <returns></returns>
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
		/// <summary>
		/// Writes to the console telling that the guild has kicked the bot. Removes the guild's settings.
		/// </summary>
		/// <param name="guild"></param>
		/// <returns></returns>
		internal async Task OnLeftGuild(SocketGuild guild)
		{
			ConsoleActions.WriteLine($"Bot has left {guild.FormatGuild()}.");

			this.RemoveUsers(guild.MemberCount);
			this.DecrementGuilds();
			await _GuildSettings.RemoveGuild(guild.Id);
		}
		#endregion

		#region Server
		/// <summary>
		/// Checks for banned names and raid prevention, logs their join to the server log, or says the welcome message.
		/// </summary>
		/// <param name="user"></param>
		/// <returns></returns>
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
					var inviteStr = await DiscordObjectFormatting.FormatInviteJoin(verified.GuildSettings, user);
					var ageWarningStr = DiscordObjectFormatting.FormatAccountAgeWarning(user);
					var embed = EmbedActions.MakeNewEmbed(null, $"**ID:** {user.Id}\n{inviteStr}\n{ageWarningStr}", Colors.JOIN)
						.MyAddAuthor(user)
						.MyAddFooter(user.IsBot ? "Bot Joined" : "User Joined");
					await MessageActions.SendEmbedMessage(verified.GuildSettings.ServerLog, embed);
				}

				//Welcome message
				if (verified.GuildSettings.WelcomeMessage != null)
				{
					await verified.GuildSettings.WelcomeMessage.Send(user);
				}
			}
		}
		/// <summary>
		/// Does nothing if the bot is the user, logs their leave to the server log, or says the goodbye message.
		/// </summary>
		/// <param name="user"></param>
		/// <returns></returns>
		internal async Task OnUserLeft(SocketGuildUser user)
		{
			this.DecrementUsers();
			this.IncrementLeaves();

			//Check if the bot was the one that left
			if (user.Id.ToString() == Config.Configuration[ConfigKeys.Bot_Id])
			{
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
					var embed = EmbedActions.MakeNewEmbed(null, $"**ID:** {user.Id}\n{DiscordObjectFormatting.FormatStayLength(user)}", Colors.LEAV)
						.MyAddAuthor(user)
						.MyAddFooter(user.IsBot ? "Bot Left" : "User Left");
					await MessageActions.SendEmbedMessage(verified.GuildSettings.ServerLog, embed);
				}

				//Goodbye message
				if (verified.GuildSettings.GoodbyeMessage != null)
				{
					await verified.GuildSettings.GoodbyeMessage.Send(user);
				}
			}
		}
		/// <summary>
		/// Logs their name change to every server that has OnUserUpdated enabled.
		/// </summary>
		/// <param name="beforeUser"></param>
		/// <param name="afterUser"></param>
		/// <returns></returns>
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
					var embed = EmbedActions.MakeNewEmbed(null, null, Colors.UEDT)
						.MyAddAuthor(afterUser)
						.MyAddField("Before:", "`" + beforeUser.Username + "`")
						.MyAddField("After:", "`" + afterUser.Username + "`", false)
						.MyAddFooter("Name Changed");
					await MessageActions.SendEmbedMessage(verified.GuildSettings.ServerLog, embed);
				}
			}
		}
		/// <summary>
		/// Handles close quotes/help entries, image only channels, spam prevention, slowmode, banned phrases, and image logging.
		/// </summary>
		/// <param name="message"></param>
		/// <returns></returns>
		internal async Task OnMessageReceived(SocketMessage message)
		{
			if (HelperFunctions.DisallowBots(message) && HelperFunctions.VerifyBotLogging(_BotSettings, _GuildSettings, message, out var verified))
			{
				var guildSettings = verified.GuildSettings;

				//Allow closewords to be handled on an unlogged channel, but don't allow anything else.
				await HelperFunctions.HandleCloseWords(_BotSettings, guildSettings, message, _Timers);
				if (HelperFunctions.VerifyLogAction(guildSettings))
				{
					var user = message.Author as IGuildUser;

					await HelperFunctions.HandleChannelSettings(guildSettings, message);
					await HelperFunctions.HandleSpamPrevention(guildSettings, verified.Guild, message, _Timers);

					//Don't bother doing stuff on the user if they're immune
					var slowmode = guildSettings.Slowmode;
					if (slowmode != null && slowmode.Enabled && !user.RoleIds.Intersect(slowmode.ImmuneRoleIds).Any())
					{
						await slowmode.HandleMessage(message, user);
					}

					await HelperFunctions.HandleBannedPhrases(_Timers, guildSettings, message);
					await HelperFunctions.HandleImageLogging(this, guildSettings.ImageLog, message);
				}
			}
		}
		/// <summary>
		/// Logs the before and after message. Handles banned phrases on the after message.
		/// </summary>
		/// <param name="cached"></param>
		/// <param name="message"></param>
		/// <param name="channel"></param>
		/// <returns></returns>
		internal async Task OnMessageUpdated(Cacheable<IMessage, ulong> cached, SocketMessage message, ISocketMessageChannel channel)
		{
			if (HelperFunctions.DisallowBots(message) && HelperFunctions.VerifyBotLogging(_BotSettings, _GuildSettings, message, out var verified) && HelperFunctions.VerifyLogAction(verified.GuildSettings))
			{
				this.IncrementEdits();
				await HelperFunctions.HandleBannedPhrases(_Timers, verified.GuildSettings, message);

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

					var embed = EmbedActions.MakeNewEmbed(null, null, Colors.MEDT)
						.MyAddAuthor(message.Author)
						.MyAddField("Before:", $"`{beforeMsgContent}`")
						.MyAddField("After:", $"`{afterMsgContent}`", false)
						.MyAddFooter("Message Updated");
					await MessageActions.SendEmbedMessage(verified.GuildSettings.ServerLog, embed);
				}
			}
		}
		/// <summary>
		/// Logs the deleted message.
		/// </summary>
		/// <param name="cached"></param>
		/// <param name="channel"></param>
		/// <returns></returns>
		/// <remarks>Very buggy command. Will not work when async. Task.Run in it will not work when awaited.</remarks>
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
					var formattedMessages = deletedMessages.OrderBy(x => x?.CreatedAt.Ticks).Select(x => x.FormatMessage());
					await MessageActions.SendMessageContainingFormattedDeletedMessages(verified.GuildSettings.ServerLog, formattedMessages);
				});
			}
			return Task.FromResult(0);
		}
		#endregion
	}
}