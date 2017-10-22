using Advobot.Core.Actions;
using Advobot.Core.Actions.Formatting;
using Advobot.Core.Classes;
using Advobot.Core.Enums;
using Advobot.Core.Interfaces;
using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Advobot.Core.Services.Log.Loggers
{
	internal sealed class MessageLogger : Logger, IMessageLogger
	{
		internal MessageLogger(ILogService logging, IServiceProvider provider) : base(logging, provider) { }

		/// <summary>
		/// Handles close quotes/help entries, image only channels, spam prevention, slowmode, banned phrases, and image logging.
		/// </summary>
		/// <param name="message"></param>
		/// <returns></returns>
		public async Task OnMessageReceived(SocketMessage message)
		{
			var logInstanceInfo = new LogInstance(_BotSettings, _GuildSettings, message, LogAction.MessageReceived);
			if (!logInstanceInfo.IsValid)
			{
				return;
			}

			var handler = new MessageHandler(logInstanceInfo, _Timers, _Logging);
			await handler.HandleBannedPhrasesAsync().CAF();
			await handler.HandleChannelSettingsAsync().CAF();
			await handler.HandleImageLoggingAsync().CAF();
			await handler.HandleSlowmodeAsync().CAF();
			await handler.HandleSpamPreventionAsync().CAF();
		}
		/// <summary>
		/// Logs the before and after message. Handles banned phrases on the after message.
		/// </summary>
		/// <param name="cached"></param>
		/// <param name="message"></param>
		/// <param name="channel"></param>
		/// <returns></returns>
		public async Task OnMessageUpdated(Cacheable<IMessage, ulong> cached, SocketMessage message, ISocketMessageChannel channel)
		{
			var logInstanceInfo = new LogInstance(_BotSettings, _GuildSettings, message, LogAction.MessageUpdated);
			if (!logInstanceInfo.IsValid)
			{
				return;
			}

			var handler = new MessageHandler(logInstanceInfo, _Timers, _Logging);
			await handler.HandleBannedPhrasesAsync().CAF();

			//If the before message is not specified always take that as it should be logged.
			//If the embed counts are greater take that as logging too.
			var edited = false;
			var beforeMessage = cached.HasValue ? cached.Value : null;
			if (logInstanceInfo.HasImageLog)
			{
				if (beforeMessage?.Embeds.Count() < message.Embeds.Count())
				{
					await handler.HandleImageLoggingAsync().CAF();
					edited = true;
				}
			}
			if (logInstanceInfo.HasServerLog)
			{
				var beforeMsgContent = (beforeMessage?.Content ?? "Empty or unable to be gotten.").RemoveAllMarkdown().RemoveDuplicateNewLines();
				var afterMsgContent = (message.Content ?? "Empty or unable to be gotten.").RemoveAllMarkdown().RemoveDuplicateNewLines();
				if (!beforeMsgContent.Equals(afterMsgContent))
				{
					var embed = new AdvobotEmbed(null, null, Constants.MEDT)
						.AddAuthor(message.Author)
						.AddField("Before:", $"`{(beforeMsgContent.Length > 750 ? "Long message" : beforeMsgContent)}`")
						.AddField("After:", $"`{(afterMsgContent.Length > 750 ? "Long message" : afterMsgContent)}`", false)
						.AddFooter("Message Updated");
					await MessageActions.SendEmbedMessageAsync(logInstanceInfo.GuildSettings.ServerLog, embed).CAF();
					edited = true;
				}
			}

			if (edited)
			{
				_Logging.MessageEdits.Increment();
			}
		}
		/// <summary>
		/// Logs the deleted message.
		/// </summary>
		/// <param name="cached"></param>
		/// <param name="channel"></param>
		/// <returns></returns>
		public Task OnMessageDeleted(Cacheable<IMessage, ulong> cached, ISocketMessageChannel channel)
		{
			//Ignore uncached messages since not much can be done with them
			var message = cached.HasValue ? cached.Value : null;
			if (message == null)
			{
				return Task.FromResult(0);
			}

			var logInstanceInfo = new LogInstance(_BotSettings, _GuildSettings, message, LogAction.MessageUpdated);
			if (!logInstanceInfo.IsValid || !logInstanceInfo.HasServerLog)
			{
				return Task.FromResult(0);
			}
			
			//Get the list of deleted messages it contains
			var msgDeletion = logInstanceInfo.GuildSettings.MessageDeletion;
			lock (msgDeletion)
			{
				msgDeletion.AddToList(message);
				_Logging.MessageDeletes.Increment();
			}

			//Use a token so the messages do not get sent prematurely
			var cancelToken = msgDeletion.CancelToken;
			if (cancelToken != null)
			{
				cancelToken.Cancel();
			}
			msgDeletion.SetCancelToken(cancelToken = new CancellationTokenSource());

			//I don't know why, but this doesn't run correctly when awaited
			//It also doesn't work correctly when this method is made async. (sends messages one by one)
			Task.Run(async () =>
			{
				try
				{
					await Task.Delay(TimeSpan.FromSeconds(Constants.SECONDS_DEFAULT), cancelToken.Token).CAF();
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
				var serverLog = logInstanceInfo.GuildSettings.ServerLog;
				await MessageActions.SendMessageContainingFormattedDeletedMessagesAsync(serverLog, formattedMessages).CAF();
			});

			return Task.FromResult(0);
		}
	}
}
