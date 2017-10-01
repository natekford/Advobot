using Advobot.Actions.Formatting;
using Advobot.Classes;
using Advobot.Classes.Punishments;
using Advobot.Interfaces;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Advobot.Actions
{
	public static class MessageActions
	{
		/// <summary>
		/// Sends a message to the given channel with the given content.
		/// </summary>
		/// <param name="channel"></param>
		/// <param name="content"></param>
		/// <returns></returns>
		public static async Task<IUserMessage> SendMessage(IMessageChannel channel, string content)
		{
			const string LONG = "The response is a long message and was sent as a text file instead";

			var guild = channel.GetGuild();
			if (guild == null)
			{
				return null;
			}

			content = DiscordObjectFormatting.FormatMessageContentForNotBeingAnnoying(guild, content);
			return content.Length < Constants.MAX_MESSAGE_LENGTH_LONG
				? await channel.SendMessageAsync(Constants.ZERO_LENGTH_CHAR + content)
				: await SendTextFile(channel, content, "Long_Message_", LONG);
		}
		/// <summary>
		/// Sends a message to the given channel with the given content and embed.
		/// </summary>
		/// <param name="channel"></param>
		/// <param name="embed"></param>
		/// <param name="content"></param>
		/// <returns></returns>
		public static async Task<IUserMessage> SendEmbedMessage(IMessageChannel channel, EmbedBuilder embed, string content = null)
		{
			var guild = channel.GetGuild();
			if (guild == null)
			{
				return null;
			}

			//Embeds have a global limit of 6000 characters
			var charCount = 0
				+ embed.Author?.Name?.Length
				+ embed.Title?.Length
				+ embed.Footer?.Text?.Length ?? 0;

			//For overflow text
			var overflowText = new StringBuilder();

			//Descriptions can only be 2048 characters max and mobile can only show up to 20 line breaks
			if (!EmbedActions.CheckIfValidDescription(embed, charCount, out string badDescription, out string error))
			{
				embed.WithDescription(error);
				overflowText.AppendLineFeed($"Description: {badDescription}");
			}
			charCount += embed.Description?.Length ?? 0;

			//Fields can only be 1024 characters max and mobile can only show up to 5 line breaks
			for (int i = 0; i < embed.Fields.Count; ++i)
			{
				var field = embed.Fields[i];
				if (!EmbedActions.CheckIfValidField(field, charCount, out string badValue, out string fieldError))
				{
					field.WithName($"Field {i}");
					field.WithValue(fieldError);
					overflowText.AppendLineFeed($"Field {i}: {badValue}");
				}

				charCount += field.Value?.ToString()?.Length ?? 0;
				charCount += field.Name?.Length ?? 0;
			}

			//Catches length errors and nsfw filter errors if an avatar has nsfw content and filtering is enabled
			IUserMessage message;
			try
			{
				message = await channel.SendMessageAsync(Constants.ZERO_LENGTH_CHAR + content ?? "", embed: embed.WithCurrentTimestamp().Build());
			}
			catch (Exception e)
			{
				ConsoleActions.ExceptionToConsole(e);
				message = await SendMessage(channel, new ErrorReason(e.Message).ToString());
			}

			//Upload the overflow
			if (overflowText.Length != 0)
			{
				await SendTextFile(channel as ITextChannel, overflowText.ToString(), "Embed_");
			}
			return message;
		}
		/// <summary>
		/// Sends a text file to the given channel with the given content.
		/// </summary>
		/// <param name="channel"></param>
		/// <param name="text"></param>
		/// <param name="fileName"></param>
		/// <param name="content"></param>
		/// <returns></returns>
		public static async Task<IUserMessage> SendTextFile(IMessageChannel channel, string text, string fileName, string content = null)
		{
			var guild = channel.GetGuild();
			if (guild == null)
			{
				return null;
			}

			if (!fileName.EndsWith("_"))
			{
				fileName += "_";
			}
			var fullFileName = fileName + TimeFormatting.FormatDateTimeForSaving() + Constants.GENERAL_FILE_EXTENSION;
			var fileInfo = GetActions.GetServerDirectoryFile(guild.Id, fullFileName);

			//Create
			SavingAndLoadingActions.OverWriteFile(fileInfo, text.RemoveAllMarkdown());
			//Upload
			var msg = await channel.SendFileAsync(fileInfo.FullName, String.IsNullOrWhiteSpace(content) ? "" : $"**{content}:**");
			//Delete
			SavingAndLoadingActions.DeleteFile(fileInfo);
			return msg;
		}
		/// <summary>
		/// Sends a formatted list of deleted messages to the given channel.
		/// </summary>
		/// <param name="channel"></param>
		/// <param name="inputList"></param>
		/// <returns></returns>
		public static async Task<IUserMessage> SendMessageContainingFormattedDeletedMessages(IMessageChannel channel, IEnumerable<string> inputList)
		{
			var guild = channel.GetGuild();
			if (guild == null)
			{
				return null;
			}

			var text = String.Join("\n", inputList).RemoveDuplicateNewLines();
			if (inputList.Count() <= 5 && text.Length < Constants.MAX_MESSAGE_LENGTH_LONG)
			{
				var embed = EmbedActions.MakeNewEmbed("Deleted Messages", text, Colors.MDEL)
					.MyAddFooter("Deleted Messages");
				return await SendEmbedMessage(channel, embed);
			}
			else
			{
				var name = "Deleted_Messages_";
				var content = $"{inputList.Count()} Deleted Messages";
				return await SendTextFile(channel, text.RemoveAllMarkdown(), name, content);
			}
		}

		/// <summary>
		/// Waits a few seconds then deletes the newly created message and the context message.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="secondStr"></param>
		/// <param name="time"></param>
		/// <returns></returns>
		public static async Task MakeAndDeleteSecondaryMessage(IMyCommandContext context, string secondStr, int time = -1)
		{
			await MakeAndDeleteSecondaryMessage(context.Channel, context.Message, secondStr, time, context.Timers);
		}
		/// <summary>
		/// Waits a few seconds then deletes the newly created message and the given message.
		/// </summary>
		/// <param name="channel"></param>
		/// <param name="message"></param>
		/// <param name="secondStr"></param>
		/// <param name="time"></param>
		/// <param name="timers"></param>
		/// <returns></returns>
		public static async Task MakeAndDeleteSecondaryMessage(IMessageChannel channel, IMessage message, string secondStr, int time = -1, ITimersModule timers = null)
		{
			if (time < 0)
			{
				time = Constants.SECONDS_DEFAULT;
			}

			var secondMessage = await channel.SendMessageAsync(Constants.ZERO_LENGTH_CHAR + secondStr);
			if (time > 0 && timers != null)
			{
				timers.AddRemovableMessages(new RemovableMessage(time, new[] { message, secondMessage }));
			}
		}
		/// <summary>
		/// If the guild has verbose errors enabled then this acts just like <see cref="MakeAndDeleteSecondaryMessage(IMessageChannel, IMessage, string, int, ITimersModule)"/>.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="reason"></param>
		/// <param name="time"></param>
		/// <returns></returns>
		public static async Task SendErrorMessage(IMyCommandContext context, ErrorReason reason, int time = -1)
		{
			if (!context.GuildSettings.VerboseErrors)
			{
				return;
			}

			await MakeAndDeleteSecondaryMessage(context.Channel, context.Message, reason.ToString(), time, context.Timers);
		}

		/// <summary>
		/// Gets the given count of messages from a channel.
		/// </summary>
		/// <param name="channel"></param>
		/// <param name="requestCount"></param>
		/// <returns></returns>
		public static async Task<List<IMessage>> GetMessages(IMessageChannel channel, int requestCount)
		{
			return (await channel.GetMessagesAsync(requestCount).Flatten()).ToList();
		}
		/// <summary>
		/// Removes the given count of messages from a channel.
		/// </summary>
		/// <param name="channel"></param>
		/// <param name="fromMessage"></param>
		/// <param name="requestCount"></param>
		/// <param name="reason"></param>
		/// <returns></returns>
		public static async Task<int> RemoveMessages(IMessageChannel channel, IMessage fromMessage, int requestCount, ModerationReason reason)
		{
			return await DeleteMessages(channel, await channel.GetMessagesAsync(fromMessage, Direction.Before, requestCount).Flatten(), reason);
		}
		/// <summary>
		/// Removes the given count of messages from a channel and a specific user.
		/// </summary>
		/// <param name="channel"></param>
		/// <param name="fromMessage"></param>
		/// <param name="requestCount"></param>
		/// <param name="user"></param>
		/// <param name="reason"></param>
		/// <returns></returns>
		public static async Task<int> RemoveMessagesFromUser(IMessageChannel channel, IMessage fromMessage, int requestCount, IUser user, ModerationReason reason)
		{
			var deletedCount = 0;
			while (requestCount > 0)
			{
				var messages = await channel.GetMessagesAsync(fromMessage, Direction.Before, 100).Flatten();
				if (!messages.Any())
				{
					break;
				}
				fromMessage = messages.Last();

				//Get messages from a targetted user
				var userMessages = messages.Where(x => x.Author.Id == user.Id);
				if (!userMessages.Any())
				{
					break;
				}

				var cutUserMessages = userMessages.ToList().GetUpToAndIncludingMinNum(requestCount, 100);
				deletedCount += await DeleteMessages(channel, cutUserMessages, reason);

				//Leave if the message count gathered implies that enough user messages have been deleted 
				if (cutUserMessages.Count() < userMessages.Count())
				{
					break;
				}

				requestCount -= cutUserMessages.Count();
			}
			return deletedCount;
		}
		/// <summary>
		/// Deletes the passed in messages directly. Will only delete messages under 14 days old.
		/// </summary>
		/// <param name="channel"></param>
		/// <param name="messages"></param>
		/// <param name="reason"></param>
		/// <returns></returns>
		public static async Task<int> DeleteMessages(IMessageChannel channel, IEnumerable<IMessage> messages, ModerationReason reason)
		{
			//13.95 for some buffer in case
			var youngMessages = messages.Where(x => x != null && DateTime.UtcNow.Subtract(x.CreatedAt.UtcDateTime).TotalDays < 13.95);
			try
			{
				await channel.DeleteMessagesAsync(youngMessages, reason.CreateRequestOptions());
				return youngMessages.Count();
			}
			catch
			{
				ConsoleActions.WriteLine($"Unable to delete {youngMessages.Count()} messages on the guild {channel.GetGuild().FormatGuild()} on channel {channel.FormatChannel()}.", color: ConsoleColor.Red);
				return 0;
			}
		}
		/// <summary>
		/// Deletes the passed in message directly.
		/// </summary>
		/// <param name="message"></param>
		/// <returns></returns>
		public static async Task<int> DeleteMessage(IMessage message)
		{
			if (message == null || DateTime.UtcNow.Subtract(message.CreatedAt.UtcDateTime).TotalDays > 13.95)
			{
				return 0;
			}

			try
			{
				await message.DeleteAsync();
				return 1;
			}
			catch
			{
				ConsoleActions.WriteLine($"Unable to delete the message {message.Id} on channel {message.Channel.FormatChannel()}.", color: ConsoleColor.Red);
				return 0;
			}
		}
	}
}