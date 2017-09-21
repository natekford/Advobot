using Advobot.Classes;
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
		private static bool CheckIfValidDescription(EmbedBuilder embed, int charCount, out string badDescription, out string error)
		{
			if (charCount > Constants.MAX_EMBED_TOTAL_LENGTH - 1250)
			{
				badDescription = embed.Description;
				error = $"`{Constants.MAX_EMBED_TOTAL_LENGTH}` char limit close.";
			}
			else if (embed.Description?.Length > Constants.MAX_DESCRIPTION_LENGTH)
			{
				badDescription = embed.Description;
				error = $"Over `{Constants.MAX_DESCRIPTION_LENGTH}` chars.";
			}
			else if (embed.Description.CountLineBreaks() > Constants.MAX_DESCRIPTION_LINES)
			{
				badDescription = embed.Description;
				error = $"Over `{Constants.MAX_DESCRIPTION_LINES}` lines.";
			}
			else
			{
				badDescription = null;
				error = null;
			}

			return error == null;
		}
		private static bool CheckIfValidField(EmbedFieldBuilder field, int charCount, out string badValue, out string error)
		{
			var value = field.Value.ToString();
			if (charCount > Constants.MAX_EMBED_TOTAL_LENGTH - 1500)
			{
				badValue = value;
				error = $"`{Constants.MAX_EMBED_TOTAL_LENGTH}` char limit close.";
			}
			else if (value?.Length > Constants.MAX_FIELD_VALUE_LENGTH)
			{
				badValue = value;
				error = $"Over `{Constants.MAX_FIELD_VALUE_LENGTH}` chars.";
			}
			else if (value.CountLineBreaks() > Constants.MAX_FIELD_LINES)
			{
				badValue = value;
				error = $"Over `{Constants.MAX_FIELD_LINES}` lines.";
			}
			else
			{
				badValue = null;
				error = null;
			}

			return error == null;
		}
		private static string FormatMessageContentForSending(IGuild guild, string content)
		{
			return content
				.CaseInsReplace(guild.EveryoneRole.Mention, Constants.FAKE_EVERYONE)
				.CaseInsReplace("@everyone", Constants.FAKE_EVERYONE)
				.CaseInsReplace("\tts", Constants.FAKE_TTS);
		}

		public static async Task<IUserMessage> SendEmbedMessage(IMessageChannel channel, EmbedBuilder embed, string content = null)
		{
			//This method is a clusterfuck.
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
			if (!CheckIfValidDescription(embed, charCount, out string badDescription, out string error))
			{
				embed.WithDescription(error);
				overflowText.AppendLineFeed($"Description: {badDescription}");
			}
			charCount += embed.Description?.Length ?? 0;

			//Fields can only be 1024 characters max and mobile can only show up to 5 line breaks
			for (int i = 0; i < embed.Fields.Count; ++i)
			{
				var field = embed.Fields[i];
				if (!CheckIfValidField(field, charCount, out string badValue, out string fieldError))
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
				message = await channel.SendMessageAsync(Constants.ZERO_LENGTH_CHAR + FormattingActions.ERROR(e.Message));
			}

			//Upload the overflow
			await SendTextFile(guild, channel, overflowText.ToString(), "Embed_");
			return message;
		}
		public static async Task<IUserMessage> SendChannelMessage(IMessageChannel channel, string content)
		{
			const string LONG = "The response is a long message and was sent as a text file instead";

			var guild = channel.GetGuild();
			if (guild == null)
			{
				return null;
			}

			content = FormatMessageContentForSending(guild, content);

			return content.Length < Constants.MAX_MESSAGE_LENGTH_LONG
				? await channel.SendMessageAsync(Constants.ZERO_LENGTH_CHAR + content)
				: await SendTextFile(guild, channel, content, "Long_Message_", LONG);
		}
		public static async Task<IUserMessage> SendTextFile(IGuild guild, IMessageChannel channel, string text, string fileName, string content = null)
		{
			if (!fileName.EndsWith("_"))
			{
				fileName += "_";
			}
			var fullFileName = fileName + FormattingActions.FormatDateTimeForSaving() + Constants.GENERAL_FILE_EXTENSION;
			var fileInfo = GetActions.GetServerDirectoryFile(guild.Id, fullFileName);

			//Create
			SavingAndLoadingActions.OverWriteFile(fileInfo, text.RemoveAllMarkdown());
			//Upload
			var msg = await channel.SendFileAsync(fileInfo.FullName, String.IsNullOrWhiteSpace(content) ? "" : $"**{content}:**");
			//Delete
			SavingAndLoadingActions.DeleteFile(fileInfo);
			return msg;
		}

		public static async Task MakeAndDeleteSecondaryMessage(IMyCommandContext context, string secondStr, int time = -1)
		{
			await MakeAndDeleteSecondaryMessage(context.Channel, context.Message, secondStr, time, context.Timers);
		}
		public static async Task MakeAndDeleteSecondaryMessage(IMessageChannel channel, IUserMessage message, string secondStr, int time = -1, ITimersModule timers = null)
		{
			var secondMsg = await channel.SendMessageAsync(Constants.ZERO_LENGTH_CHAR + secondStr);
			var messages = new[] { secondMsg, message };

			if (time < 0)
			{
				time = Constants.SECONDS_DEFAULT;
			}

			if (message == null)
			{
				RemoveCommandMessage(secondMsg, time, timers);
			}
			else
			{
				RemoveCommandMessages(messages, time, timers);
			}
		}
		public static void RemoveCommandMessages(IEnumerable<IMessage> messages, int time = 0, ITimersModule timers = null)
		{
			if (time > 0 && timers != null)
			{
				timers.AddRemovableMessages(new RemovableMessage(time, messages.ToArray()));
			}
		}
		public static void RemoveCommandMessage(IMessage message, int time = 0, ITimersModule timers = null)
		{
			if (time > 0 && timers != null)
			{
				timers.AddRemovableMessages(new RemovableMessage(time, message));
			}
		}

		public static async Task<List<IMessage>> GetMessages(IMessageChannel channel, int requestCount)
		{
			return (await channel.GetMessagesAsync(++requestCount).Flatten()).ToList();
		}
		public static async Task<int> RemoveMessages(IMessageChannel channel, IMessage fromMessage, int requestCount, string reason)
		{
			var guildChannel = channel as ITextChannel;
			if (guildChannel == null)
			{
				return 0;
			}

			var messages = await channel.GetMessagesAsync(fromMessage, Direction.Before, requestCount).Flatten();
			await DeleteMessages(channel, messages, reason);
			return messages.Count();
		}
		public static async Task<int> RemoveMessages(IMessageChannel channel, IMessage fromMessage, int requestCount, IUser user, string reason)
		{
			var guildChannel = channel as ITextChannel;
			if (guildChannel == null)
			{
				return 0;
			}

			if (user == null)
			{
				return await RemoveMessages(channel, fromMessage, requestCount, reason);
			}

			var deletedCount = 0;
			while (requestCount > 0)
			{
				//Get the current messages and ones that aren't null
				var messages = await channel.GetMessagesAsync(fromMessage, Direction.Before, 100).Flatten();
				if (!messages.Any())
				{
					break;
				}

				//Set the from message as the last of the currently grabbed ones
				fromMessage = messages.Last();

				//Check for messages of the targetted user
				messages = messages.Where(x => x.Author.Id == user.Id);
				if (!messages.Any())
				{
					break;
				}

				var gatheredForUserAmt = messages.Count();
				messages = messages.ToList().GetUpToAndIncludingMinNum(requestCount, gatheredForUserAmt, 100);

				//Delete them in a try catch due to potential errors
				var msgAmt = messages.Count();
				try
				{
					await DeleteMessages(channel, messages, reason);
					deletedCount += msgAmt;
				}
				catch
				{
					ConsoleActions.WriteLine($"Unable to delete {msgAmt} messages on the guild {guildChannel.Guild.FormatGuild()} on channel {guildChannel.FormatChannel()}.", color: ConsoleColor.Red);
					break;
				}

				//Leave if the message count gathered implies that enough user messages have been deleted 
				if (msgAmt < gatheredForUserAmt)
				{
					break;
				}

				requestCount -= msgAmt;
			}
			return deletedCount;
		}
		public static async Task DeleteMessages(IMessageChannel channel, IEnumerable<IMessage> messages, string reason = null)
		{
			if (messages == null || !messages.Any())
			{
				return;
			}

			try
			{
				await channel.DeleteMessagesAsync(messages.Where(x => DateTime.UtcNow.Subtract(x.CreatedAt.UtcDateTime).TotalDays < 14).Distinct(), new RequestOptions { AuditLogReason = reason });
			}
			catch
			{
				ConsoleActions.WriteLine($"Unable to delete {messages.Count()} messages on the guild {channel.GetGuild().FormatGuild()} on channel {channel.FormatChannel()}.", color: ConsoleColor.Red);
			}
		}
		public static async Task DeleteMessage(IMessage message)
		{
			if (message == null || DateTime.UtcNow.Subtract(message.CreatedAt.UtcDateTime).TotalDays >= 14)
			{
				return;
			}

			try
			{
				await message.DeleteAsync();
			}
			catch
			{
				ConsoleActions.WriteLine($"Unable to delete the message {message.Id} on channel {message.Channel.FormatChannel()}.", color: ConsoleColor.Red);
			}
		}
		public static async Task SendMessageContainingFormattedDeletedMessages(IGuild guild, ITextChannel channel, IEnumerable<string> inputList)
		{
			if (!inputList.Any())
			{
				return;
			}

			var text = String.Join("\n", inputList).RemoveDuplicateNewLines();
			if (inputList.Count() <= 5 && text.Length < Constants.MAX_MESSAGE_LENGTH_LONG)
			{
				var embed = EmbedActions.MakeNewEmbed("Deleted Messages", text, Colors.MDEL)
					.MyAddFooter("Deleted Messages");
				await SendEmbedMessage(channel, embed);
			}
			else
			{
				var name = "Deleted_Messages_";
				var content = $"{inputList.Count()} Deleted Messages";
				await SendTextFile(guild, channel, text.RemoveAllMarkdown(), name, content);
			}
		}
	}
}