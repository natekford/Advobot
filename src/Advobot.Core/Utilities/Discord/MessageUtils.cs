using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Advobot.Core.Classes;
using Advobot.Core.Classes.Punishments;
using Advobot.Core.Interfaces;
using Advobot.Core.Utilities.Formatting;
using Discord;
using Discord.Commands;

namespace Advobot.Core.Utilities
{
	/// <summary>
	/// Actions which are done on an <see cref="IMessage"/>.
	/// </summary>
	public static class MessageUtils
	{
		public const string ZERO_LENGTH_CHAR = "\u180E";
		private const string LONG = "Response too long. Sent as text file instead.";

		/// <summary>
		/// Sends a message to the given channel with the given content.
		/// </summary>
		/// <param name="channel"></param>
		/// <param name="content"></param>
		/// <returns></returns>
		public static async Task<IUserMessage> SendMessageAsync(IMessageChannel channel, string content)
		{
			if (String.IsNullOrWhiteSpace(content) || !(channel.GetGuild() is IGuild guild))
			{
				return null;
			}

			content = content.SanitizeContent(guild);
			return content.Length < Constants.MAX_MESSAGE_LENGTH
				? await channel.SendMessageAsync(content).CAF()
				: await SendTextFileAsync(channel, content, "Long_Message_", LONG).CAF();
		}
		/// <summary>
		/// Sends a message to the given channel with the given content and embed.
		/// </summary>
		/// <param name="channel"></param>
		/// <param name="embed"></param>
		/// <param name="content"></param>
		/// <returns></returns>
		public static async Task<IEnumerable<IUserMessage>> SendEmbedMessageAsync(IMessageChannel channel, EmbedWrapper embed, string content = null)
		{
			if (!(channel.GetGuild() is IGuild guild))
			{
				return Enumerable.Empty<IUserMessage>();
			}

			//Catches length errors and nsfw filter errors if an avatar has nsfw content and filtering is enabled
			var messages = new List<IUserMessage>
			{
				await channel.SendMessageAsync((content ?? "").SanitizeContent(guild), embed: embed.Build()).CAF()
			};
			//Upload any errors
			if (embed.FailedValues.Any())
			{
				messages.Add(await SendTextFileAsync(channel, embed.ToString(), "Embed_").CAF());
			}
			return messages;
		}
		/// <summary>
		/// Sends a text file to the given channel with the given content.
		/// </summary>
		/// <param name="channel"></param>
		/// <param name="text"></param>
		/// <param name="fileName"></param>
		/// <param name="content"></param>
		/// <returns></returns>
		public static async Task<IUserMessage> SendTextFileAsync(IMessageChannel channel, string text, string fileName, string content = null)
		{
			if (!(channel.GetGuild() is IGuild guild))
			{
				return null;
			}

			if (!fileName.EndsWith("_"))
			{
				fileName += "_";
			}
			var fullFileName = $"{fileName}{TimeFormatting.ToSaving()}.txt";
			var fileInfo = IOUtils.GetServerDirectoryFile(channel.GetGuild()?.Id ?? 0, fullFileName);
			var c = (String.IsNullOrWhiteSpace(content) ? "" : $"**{content}:**").SanitizeContent(guild);

			IOUtils.OverwriteFile(fileInfo, text.RemoveAllMarkdown());
			var msg = await channel.SendFileAsync(fileInfo.FullName, c).CAF();
			IOUtils.DeleteFile(fileInfo);
			return msg;
		}
		/// <summary>
		/// Waits a few seconds then deletes the newly created message and the context message.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="secondStr"></param>
		/// <param name="time"></param>
		/// <returns></returns>
		public static async Task<RemovableMessage> MakeAndDeleteSecondaryMessageAsync(IAdvobotCommandContext context, string secondStr, TimeSpan time = default)
		{
			return await MakeAndDeleteSecondaryMessageAsync(context.Timers, context.Channel, context.Message, secondStr, time).CAF();
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
		public static async Task<RemovableMessage> MakeAndDeleteSecondaryMessageAsync(ITimersService timers, IMessageChannel channel, IMessage message, string secondStr, TimeSpan time = default)
		{
			if (time.Equals(default))
			{
				time = Constants.DEFAULT_WAIT_TIME;
			}

			var secondMessage = await SendMessageAsync(channel, ZERO_LENGTH_CHAR + secondStr).CAF();
			var removableMessage = new RemovableMessage(time, message, secondMessage);
			timers?.Add(removableMessage);
			return removableMessage;
		}
		/// <summary>
		/// If the guild has verbose errors enabled then this acts just like makeanddeletesecondarymessage.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="error"></param>
		/// <param name="time"></param>
		/// <returns></returns>
		public static async Task<RemovableMessage> SendErrorMessageAsync(IAdvobotCommandContext context, IError error, TimeSpan time = default)
		{
			return await SendErrorMessageAsync(context.Timers, context.GuildSettings, context.Channel, context.Message, error, time).CAF();
		}
		/// <summary>
		/// If the guild has verbose errors enabled then this acts just like makeanddeletesecondarymessage.
		/// </summary>
		/// <param name="timers"></param>
		/// <param name="settings"></param>
		/// <param name="channel"></param>
		/// <param name="message"></param>
		/// <param name="error"></param>
		/// <param name="time"></param>
		/// <returns></returns>
		public static async Task<RemovableMessage> SendErrorMessageAsync(ITimersService timers, IGuildSettings settings, IMessageChannel channel, IMessage message, IError error, TimeSpan time = default)
		{
			return settings.NonVerboseErrors ? default : await MakeAndDeleteSecondaryMessageAsync(timers, channel, message, $"**ERROR:** {error.Reason}", time).CAF();
		}
		/// <summary>
		/// Gets the given count of messages from a channel.
		/// </summary>
		/// <param name="channel"></param>
		/// <param name="requestCount"></param>
		/// <returns></returns>
		public static async Task<IEnumerable<IMessage>> GetMessagesAsync(ITextChannel channel, int requestCount)
		{
			return await channel.GetMessagesAsync(requestCount).FlattenAsync().CAF();
		}
		/// <summary>
		/// Removes the given count of messages from a channel.
		/// </summary>
		/// <param name="channel"></param>
		/// <param name="fromMessage"></param>
		/// <param name="requestCount"></param>
		/// <param name="reason"></param>
		/// <param name="fromUser"></param>
		/// <returns></returns>
		public static async Task<int> DeleteMessagesAsync(ITextChannel channel, IMessage fromMessage, int requestCount, ModerationReason reason, IUser fromUser = null)
		{
			if (fromUser == null)
			{
				var messages = await channel.GetMessagesAsync(fromMessage, Direction.Before, requestCount).FlattenAsync().CAF();
				return await DeleteMessagesAsync(channel, messages, reason).CAF();
			}

			var deletedCount = 0;
			while (requestCount > 0)
			{
				var messages = (await channel.GetMessagesAsync(fromMessage, Direction.Before).FlattenAsync().CAF()).ToList();
				if (!messages.Any())
				{
					break;
				}
				fromMessage = messages.Last();

				//Get messages from a targetted user
				var userMessages = messages.Where(x => x.Author.Id == fromUser.Id).Take(Math.Min(requestCount, 100)).ToList();
				if (!userMessages.Any())
				{
					break;
				}
				deletedCount += await DeleteMessagesAsync(channel, userMessages, reason).CAF();

				requestCount -= userMessages.Count();
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
		public static async Task<int> DeleteMessagesAsync(ITextChannel channel, IEnumerable<IMessage> messages, ModerationReason reason)
		{
			//13.95 for some buffer in case
			var validMessages = messages.Where(x => x != null && DateTime.UtcNow.Subtract(x.CreatedAt.UtcDateTime).TotalDays < 13.95).ToList();
			try
			{
				await channel.DeleteMessagesAsync(validMessages, reason.CreateRequestOptions()).CAF();
				return validMessages.Count();
			}
			catch
			{
				ConsoleUtils.WriteLine($"Unable to delete {validMessages.Count()} messages on the guild {channel.GetGuild().Format()} on channel {channel.Format()}.", color: ConsoleColor.Red);
				return 0;
			}
		}
		/// <summary>
		/// Deletes the passed in message directly.
		/// </summary>
		/// <param name="message"></param>
		/// <param name="reason"></param>
		/// <returns></returns>
		public static async Task<int> DeleteMessageAsync(IMessage message, ModerationReason reason)
		{
			if (message == null || (DateTime.UtcNow - message.CreatedAt.UtcDateTime).TotalDays > 13.95)
			{
				return 0;
			}
			try
			{
				await message.DeleteAsync(reason.CreateRequestOptions()).CAF();
				return 1;
			}
			catch
			{
				ConsoleUtils.WriteLine($"Unable to delete the message {message.Id} on channel {message.Channel.Format()}.", color: ConsoleColor.Red);
				return 0;
			}
		}
		private static string SanitizeContent(this string content, IGuild guild)
		{
			return ZERO_LENGTH_CHAR + content.CaseInsReplace(guild.EveryoneRole.Mention, $"@{ZERO_LENGTH_CHAR}everyone") //Everyone and Here have the same role.
				.CaseInsReplace("@everyone", $"@{ZERO_LENGTH_CHAR}everyone")
				.CaseInsReplace("@here", $"@{ZERO_LENGTH_CHAR}here")
				.CaseInsReplace("discord.gg", $"discord{ZERO_LENGTH_CHAR}.gg")
				.CaseInsReplace("\tts", $"\\{ZERO_LENGTH_CHAR}tts");
		}
	}
}