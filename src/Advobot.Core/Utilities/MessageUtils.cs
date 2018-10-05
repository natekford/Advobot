using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Advobot.Classes;
using Advobot.Interfaces;
using AdvorangesUtils;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Utilities
{
	/// <summary>
	/// Actions which are done on an <see cref="IMessage"/>.
	/// </summary>
	public static class MessageUtils
	{
		/// <summary>
		/// Sends a message to the given channel with the given content.
		/// </summary>
		/// <param name="channel"></param>
		/// <param name="content"></param>
		/// <param name="embedWrapper"></param>
		/// <param name="textFile"></param>
		/// <param name="allowZeroWidthLengthMessages">
		/// If there is no content passed in the content will become only a single zero width space.
		/// This ends up taking up extra space if used with embeds or files.
		/// </param>
		/// <returns></returns>
		[Obsolete]
		public static async Task<IUserMessage> SendMessageAsync(
			IMessageChannel channel,
			string content = null,
			EmbedWrapper embedWrapper = null,
			TextFileInfo textFile = null,
			bool allowZeroWidthLengthMessages = false)
		{
			if (channel == null)
			{
				throw new ArgumentNullException(nameof(channel));
			}
			if (content == null && embedWrapper == null && textFile == null)
			{
				throw new ArgumentNullException($"Input ({nameof(content)}, {nameof(embedWrapper)}, or {nameof(textFile)} must have a value)");
			}

			textFile = textFile ?? new TextFileInfo();

			//Make sure all the information from the embed that didn't fit goes in.
			if (embedWrapper != null && embedWrapper.Errors.Any())
			{
				textFile.Name = textFile.Name ?? "Embed_Errors";
				textFile.Text += $"Embed Errors:\n{embedWrapper}\n\n{textFile.Text}";
			}

			//Make sure none of the content mentions everyone or doesn't have the zero width character
			content = channel.SanitizeContent(content);
			if (content.Length > 2000)
			{
				textFile.Name = textFile.Name ?? "Long_Message";
				textFile.Text += $"Message Content:\n{content}\n\n{textFile.Text}";
				content = $"{Constants.ZERO_LENGTH_CHAR}Response is too long; sent as text file instead.";
			}

			//Can clear the content if it's going to only be a zero length space and there's an embed
			//Otherwise there will be unecessary empty space
			if (!allowZeroWidthLengthMessages && content == Constants.ZERO_LENGTH_CHAR && embedWrapper != null)
			{
				content = "";
			}

			try
			{
				//If the file name and text exists, then attempt to send as a file instead of message
				if (textFile.Name != null && textFile.Text != null)
				{
					using (var stream = new MemoryStream())
					using (var writer = new StreamWriter(stream))
					{
						writer.Write(textFile.Text.Trim());
						writer.Flush();
						stream.Seek(0, SeekOrigin.Begin);
						return await channel.SendFileAsync(stream, textFile.Name, content, embed: embedWrapper).CAF();
					}
				}
				return await channel.SendMessageAsync(content, embed: embedWrapper).CAF();
			}
			//If the message fails to send, then return the error
			catch (Exception e)
			{
				return await channel.SendMessageAsync(channel.SanitizeContent(e.Message));
			}
		}
		/// <summary>
		/// Gets the given count of messages from a channel.
		/// </summary>
		/// <param name="channel"></param>
		/// <param name="requestCount"></param>
		/// <returns></returns>
		public static async Task<IEnumerable<IMessage>> GetMessagesAsync(SocketTextChannel channel, int requestCount)
		{
			//TODO: do something similar to delete messages async to get more than 100 messages?
			return await channel.GetMessagesAsync(requestCount).FlattenAsync().CAF();
		}
		/// <summary>
		/// Removes the given count of messages from a channel.
		/// </summary>
		/// <param name="channel"></param>
		/// <param name="fromMessage"></param>
		/// <param name="count"></param>
		/// <param name="options"></param>
		/// <param name="fromUser"></param>
		/// <returns></returns>
		public static async Task<int> DeleteMessagesAsync(
			SocketTextChannel channel,
			IMessage fromMessage,
			int count,
			RequestOptions options,
			IUser fromUser = null)
		{
			var deletedCount = 0;
			while (count > 0)
			{
				var messages = (await channel.GetMessagesAsync(fromMessage, Direction.Before, 100).FlattenAsync().CAF()).ToList();
				fromMessage = messages.LastOrDefault();

				//Get messages from a targeted user if one is supplied
				var userMessages = fromUser == null ? messages : messages.Where(x => x.Author.Id == fromUser.Id);
				var cutMessages = userMessages.Take(count).ToList();

				//If less messages are deleted than gathered, that means there are some that are too old meaning we can stop
				var deletedThisIteration = await DeleteMessagesAsync(channel, cutMessages, options).CAF();
				deletedCount += deletedThisIteration;
				count -= deletedThisIteration;
				if (deletedThisIteration < cutMessages.Count)
				{
					break;
				}
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
		public static async Task<int> DeleteMessagesAsync(SocketTextChannel channel, IEnumerable<IMessage> messages, RequestOptions options)
		{
			var validMessages = messages.Where(x => x != null && (DateTime.UtcNow - x.CreatedAt.UtcDateTime).TotalDays < 14).ToList();
			if (validMessages.Count == 0)
			{
				return 0;
			}

			try
			{
				if (validMessages.Count == 1)
				{
					await validMessages[0].DeleteAsync(options).CAF();
				}
				else
				{
					await channel.DeleteMessagesAsync(validMessages, options).CAF();
				}
				return validMessages.Count;
			}
			catch
			{
				return 0;
			}
		}

		private static string SanitizeContent(this IMessageChannel channel, string content)
		{
			if (content == null)
			{
				return Constants.ZERO_LENGTH_CHAR;
			}
			if (!content.StartsWith(Constants.ZERO_LENGTH_CHAR))
			{
				content = Constants.ZERO_LENGTH_CHAR + content;
			}
			if (channel is IGuildChannel guildChannel)
			{
				content = content.CaseInsReplace(guildChannel.Guild.EveryoneRole.Mention, $"@{Constants.ZERO_LENGTH_CHAR}everyone"); //Everyone and Here have the same role
			}
			return content
				.CaseInsReplace("@everyone", $"@{Constants.ZERO_LENGTH_CHAR}everyone")
				.CaseInsReplace("@here", $"@{Constants.ZERO_LENGTH_CHAR}here")
				.CaseInsReplace("discord.gg", $"discord{Constants.ZERO_LENGTH_CHAR}.gg")
				.CaseInsReplace("\tts", $"\\{Constants.ZERO_LENGTH_CHAR}tts");
		}
	}
}