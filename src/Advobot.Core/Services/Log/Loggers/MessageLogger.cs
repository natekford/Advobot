﻿using Advobot.Core.Utilities;
using Advobot.Core.Utilities.Formatting;
using Advobot.Core.Classes;
using Advobot.Core.Enums;
using Advobot.Core.Interfaces;
using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
			//For some meme server
			var guild = message.GetGuild();
			if (guild.Id == 294173126697418752)
			{
				var author = message.Author as IGuildUser;
				if (author.Username != "jeff" && author.Nickname != "jeff" && guild.GetBot().GetIfCanModifyUser(author))
				{
					await UserUtils.ChangeNicknameAsync(author, "jeff", new ModerationReason("my nama jeff")).CAF();
				}
			}

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
				var bMsgContent = (beforeMessage?.Content ?? "Empty or unable to be gotten.").RemoveAllMarkdown().RemoveDuplicateNewLines();
				var aMsgContent = (message.Content ?? "Empty or unable to be gotten.").RemoveAllMarkdown().RemoveDuplicateNewLines();
				if (!bMsgContent.Equals(aMsgContent))
				{
					var embed = new EmbedWrapper
					{
						Color = Constants.MEDT,
					};
					embed.TryAddAuthor(message.Author, out var authorErrors);
					embed.TryAddField("Before:", $"`{(bMsgContent.Length > 750 ? "Long message" : bMsgContent)}`", true, out var firstFieldErrors);
					embed.TryAddField("After:", $"`{(aMsgContent.Length > 750 ? "Long message" : aMsgContent)}`", false, out var secondFieldErrors);
					embed.TryAddFooter("Message Updated", null, out var footerErrors);
					await MessageUtils.SendEmbedMessageAsync(logInstanceInfo.GuildSettings.ServerLog, embed).CAF();
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

			_Logging.MessageDeletes.Increment();

			var msgDeletion = logInstanceInfo.GuildSettings.MessageDeletion;
			msgDeletion.Messages.Add(message);
			//The old cancel token gets cancled in the getter of this
			var cancelToken = msgDeletion.CancelToken;

			//Has to run on completely separate thread, else prints early
			Task.Run(async () =>
			{
				//Wait three seconds. If a new message comes in then the token will be canceled and this won't continue.
				//If more than 25 messages just start printing them out so people can't stall the messages forever.
				var inEmbed = msgDeletion.Messages.Count < 10; //Needs very few messages to fit in an embed
				if (msgDeletion.Messages.Count < 25)
				{
					try
					{
						await Task.Delay(TimeSpan.FromSeconds(Constants.SECONDS_DEFAULT), cancelToken.Token).CAF();
					}
					catch (TaskCanceledException)
					{
						return;
					}
				}

				//Give the messages to a new list so they can be removed from the old one
				var messages = new List<IMessage>(msgDeletion.Messages).OrderBy(x => x?.CreatedAt.Ticks);
				msgDeletion.ClearBag();

				var sb = new StringBuilder();
				while (inEmbed)
				{
					foreach (var m in messages)
					{
						sb.AppendLineFeed(m.Format(true));
						//Can only stay in an embed if the description length is less than the max length
						//and if the line numbers are less than 20
						var validDesc = sb.Length < EmbedBuilder.MaxDescriptionLength;
						var validLines = sb.ToString().RemoveDuplicateNewLines().CountLineBreaks() < EmbedWrapper.MaxDescriptionLines;
						inEmbed = validDesc && validLines;
					}
					break;
				}

				var serverLog = logInstanceInfo.GuildSettings.ServerLog;
				if (inEmbed)
				{
					var embed = new EmbedWrapper
					{
						Title = "Deleted Messages",
						Description = sb.ToString().RemoveDuplicateNewLines(),
						Color = Constants.MDEL,
					};
					embed.TryAddFooter("Deleted Messages", null, out var errors);
					await MessageUtils.SendEmbedMessageAsync(serverLog, embed).CAF();
				}
				else
				{
					sb.Clear();
					foreach (var m in messages)
					{
						sb.AppendLineFeed(m.Format(false));
					}

					var text = sb.ToString().RemoveAllMarkdown().RemoveDuplicateNewLines();
					await MessageUtils.SendTextFileAsync(serverLog, text, "Deleted Messages", $"{messages.Count()} {"Deleted Messages"}").CAF();
				}
			});

			return Task.FromResult(0);
		}
	}
}
