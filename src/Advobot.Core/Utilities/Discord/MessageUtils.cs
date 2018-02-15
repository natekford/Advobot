using Advobot.Core.Classes;
using Advobot.Core.Classes.Punishments;
using Advobot.Core.Interfaces;
using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Advobot.Core.Utilities
{
	/// <summary>
	/// Actions which are done on an <see cref="IMessage"/>.
	/// </summary>
	public static class MessageUtils
	{
		public const string ZERO_LENGTH_CHAR = "\u180E";
		private const string LONG = "Response is too long; sent as text file instead.";

		/// <summary>
		/// Sends a message to the given channel with the given content.
		/// </summary>
		/// <param name="channel"></param>
		/// <param name="content"></param>
		/// <returns></returns>
		public static async Task<IUserMessage> SendMessageAsync(IMessageChannel channel, string content)
		{
			if (String.IsNullOrWhiteSpace(content))
			{
				return null;
			}

			content = content.SanitizeContent(channel);
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
			//Catches length errors and nsfw filter errors if an avatar has nsfw content and filtering is enabled
			var messages = new List<IUserMessage>
			{
				await channel.SendMessageAsync((content ?? "").SanitizeContent(channel), embed: embed.Build()).CAF()
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
			fileName = $"{fileName.TrimEnd('_')}_{Formatting.ToSaving()}.txt";
			content = (content == null ? "" : $"**{content}:**").SanitizeContent(channel);

			using (var stream = new MemoryStream())
			using (var writer = new StreamWriter(stream))
			{
				writer.Write(text);
				writer.Flush();
				stream.Seek(0, SeekOrigin.Begin);
				return await channel.SendFileAsync(stream, fileName, content).CAF();
			}
		}
		/// <summary>
		/// Waits a few seconds then deletes the newly created message and the context message.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="secondStr"></param>
		/// <param name="time"></param>
		/// <returns></returns>
		public static async Task<RemovableMessage> MakeAndDeleteSecondaryMessageAsync(AdvobotSocketCommandContext context, string secondStr, TimeSpan time = default)
		{
			return await MakeAndDeleteSecondaryMessageAsync((SocketTextChannel)context.Channel, context.Message, secondStr, context.Timers, time).CAF();
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
		public static async Task<RemovableMessage> MakeAndDeleteSecondaryMessageAsync(SocketTextChannel channel, IUserMessage message, string secondStr, ITimersService timers = null, TimeSpan time = default)
		{
			if (time.Equals(default))
			{
				time = Constants.DEFAULT_WAIT_TIME;
			}

			var secondMessage = await SendMessageAsync(channel, ZERO_LENGTH_CHAR + secondStr).CAF();
			var removableMessage = new RemovableMessage(time, channel, message, secondMessage);
			if (timers != null)
			{
				await timers.AddAsync(removableMessage).CAF();
			}
			return removableMessage;
		}
		/// <summary>
		/// If the guild has verbose errors enabled then this acts just like makeanddeletesecondarymessage.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="error"></param>
		/// <param name="time"></param>
		/// <returns></returns>
		public static async Task<RemovableMessage> SendErrorMessageAsync(AdvobotSocketCommandContext context, Error error, TimeSpan time = default)
		{
			return await SendErrorMessageAsync((SocketTextChannel)context.Channel, context.GuildSettings, context.Message, error, context.Timers, time).CAF();
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
		public static async Task<RemovableMessage> SendErrorMessageAsync(SocketTextChannel channel, IGuildSettings settings, IUserMessage message, Error error, ITimersService timers, TimeSpan time = default)
		{
			return settings.NonVerboseErrors ? default : await MakeAndDeleteSecondaryMessageAsync(channel, message, $"**ERROR:** {error.Reason}", timers, time).CAF();
		}
		/// <summary>
		/// Gets the given count of messages from a channel.
		/// </summary>
		/// <param name="channel"></param>
		/// <param name="requestCount"></param>
		/// <returns></returns>
		public static async Task<IEnumerable<IMessage>> GetMessagesAsync(SocketTextChannel channel, int requestCount)
		{
			return await channel.GetMessagesAsync(requestCount).FlattenAsync().CAF();
		}
		/// <summary>
		/// Removes the given count of messages from a channel.
		/// </summary>
		/// <param name="channel"></param>
		/// <param name="fromMessage"></param>
		/// <param name="requestCount"></param>
		/// <param name="options"></param>
		/// <param name="fromUser"></param>
		/// <returns></returns>
		public static async Task<int> DeleteMessagesAsync(ITextChannel channel, IMessage fromMessage, int requestCount, RequestOptions options, IUser fromUser = null)
		{
			if (fromUser == null)
			{
				var messages = await channel.GetMessagesAsync(fromMessage, Direction.Before, requestCount).FlattenAsync().CAF();
				return await DeleteMessagesAsync(channel, messages, options).CAF();
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
				deletedCount += await DeleteMessagesAsync(channel, userMessages, options).CAF();

				requestCount -= userMessages.Count();
			}
			return deletedCount;
		}
		/// <summary>
		/// Deletes the passed in messages directly. Will only delete messages under 14 days old.
		/// </summary>
		/// <param name="channel"></param>
		/// <param name="messages"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		public static async Task<int> DeleteMessagesAsync(ITextChannel channel, IEnumerable<IMessage> messages, RequestOptions options)
		{
			//13.95 for some buffer in case
			var validMessages = messages.Where(x => x != null && DateTime.UtcNow.Subtract(x.CreatedAt.UtcDateTime).TotalDays < 13.95).ToList();
			try
			{
				await channel.DeleteMessagesAsync(validMessages, options).CAF();
				return validMessages.Count();
			}
			catch
			{
				ConsoleUtils.WriteLine($"Unable to delete {validMessages.Count()} messages on the guild {channel.Guild.Format()} on channel {channel.Format()}.", color: ConsoleColor.Red);
				return 0;
			}
		}
		/// <summary>
		/// Deletes the passed in message directly.
		/// </summary>
		/// <param name="message"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		public static async Task<int> DeleteMessageAsync(IMessage message, RequestOptions options)
		{
			if (message == null || (DateTime.UtcNow - message.CreatedAt.UtcDateTime).TotalDays > 13.95)
			{
				return 0;
			}
			try
			{
				await message.DeleteAsync(options).CAF();
				return 1;
			}
			catch
			{
				ConsoleUtils.WriteLine($"Unable to delete the message {message.Id} on channel {message.Channel.Format()}.", color: ConsoleColor.Red);
				return 0;
			}
		}
		private static string SanitizeContent(this string content, IMessageChannel channel)
		{
			if (channel is SocketGuildChannel guildChannel)
			{
				content = content.CaseInsReplace(guildChannel.Guild.EveryoneRole.Mention, $"@{ZERO_LENGTH_CHAR}everyone"); //Everyone and Here have the same role
			}
			return ZERO_LENGTH_CHAR + content
				.CaseInsReplace("@everyone", $"@{ZERO_LENGTH_CHAR}everyone")
				.CaseInsReplace("@here", $"@{ZERO_LENGTH_CHAR}here")
				.CaseInsReplace("discord.gg", $"discord{ZERO_LENGTH_CHAR}.gg")
				.CaseInsReplace("\tts", $"\\{ZERO_LENGTH_CHAR}tts");
		}
	}
}