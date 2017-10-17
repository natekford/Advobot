using Advobot.Actions;
using Advobot.Actions.Formatting;
using Advobot.Classes;
using Advobot.Enums;
using Advobot.Interfaces;
using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Advobot.Services.Log.Loggers
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
			await handler.HandleBannedPhrasesAsync();
			await handler.HandleChannelSettingsAsync();
			await handler.HandleImageLoggingAsync();
			await handler.HandleSlowmodeAsync();
			await handler.HandleSpamPreventionAsync();
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
			_Logging.MessageEdits.Increment();

			var logInstanceInfo = new LogInstance(_BotSettings, _GuildSettings, message, LogAction.MessageUpdated);
			if (!logInstanceInfo.IsValid)
			{
				return;
			}

			var handler = new MessageHandler(logInstanceInfo, _Timers, _Logging);
			await handler.HandleBannedPhrasesAsync();

			//If the before message is not specified always take that as it should be logged.
			//If the embed counts are greater take that as logging too.
			var beforeMessage = cached.HasValue ? cached.Value : null;
			if (beforeMessage?.Embeds.Count() < message.Embeds.Count())
			{
				await handler.HandleImageLoggingAsync();
			}
			if (logInstanceInfo.HasServerLog)
			{
				var beforeMsgContent = (beforeMessage?.Content ?? "Empty or unable to be gotten.").RemoveAllMarkdown().RemoveDuplicateNewLines();
				var afterMsgContent = (message.Content ?? "Empty or unable to be gotten.").RemoveAllMarkdown().RemoveDuplicateNewLines();
				if (beforeMsgContent.Equals(afterMsgContent))
				{
					return;
				}

				var embed = new AdvobotEmbed(null, null, Colors.MEDT)
					.AddAuthor(message.Author)
					.AddField("Before:", $"`{(beforeMsgContent.Length > 750 ? "Long message" : beforeMsgContent)}`")
					.AddField("After:", $"`{(afterMsgContent.Length > 750 ? "Long message" : afterMsgContent)}`", false)
					.AddFooter("Message Updated");
				await MessageActions.SendEmbedMessageAsync(logInstanceInfo.GuildSettings.ServerLog, embed);
			}
		}
		/// <summary>
		/// Logs the deleted message.
		/// </summary>
		/// <param name="cached"></param>
		/// <param name="channel"></param>
		/// <returns></returns>
		/// <remarks>Very buggy command. Will not work when async. Task.Run in it will not work when awaited.</remarks>
		public Task OnMessageDeleted(Cacheable<IMessage, ulong> cached, ISocketMessageChannel channel)
		{
			_Logging.MessageDeletes.Increment();

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
			}

			//Use a token so the messages do not get sent prematurely
			var cancelToken = msgDeletion.CancelToken;
			if (cancelToken != null)
			{
				cancelToken.Cancel();
			}
			msgDeletion.SetCancelToken(cancelToken = new CancellationTokenSource());

			//I don't know why, but this doesn't run correctly when awaited and 
			//it also doesn't work correctly when this method is made async. (sends messages one by one)
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
				await MessageActions.SendMessageContainingFormattedDeletedMessagesAsync(logInstanceInfo.GuildSettings.ServerLog, formattedMessages);
			});

			return Task.FromResult(0);
		}
	}
}
