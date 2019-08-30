using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Advobot.Classes;

using AdvorangesUtils;

using Discord;

namespace Advobot.Utilities
{
	/// <summary>
	/// Actions which are done on an <see cref="IMessage"/>.
	/// </summary>
	public static class MessageUtils
	{
		/// <summary>
		/// Removes the given count of messages from a channel.
		/// </summary>
		/// <param name="channel"></param>
		/// <param name="from"></param>
		/// <param name="count"></param>
		/// <param name="options"></param>
		/// <param name="predicate"></param>
		/// <returns></returns>
		public static async Task<int> DeleteMessagesAsync(
			ITextChannel channel,
			IMessage from,
			int count,
			RequestOptions options,
			Func<IMessage, bool>? predicate = null)
		{
			var deletedCount = 0;
			while (count > 0)
			{
				var flattened = await channel.GetMessagesAsync(from, Direction.Before, 100).FlattenAsync().CAF();
				var messages = flattened.ToArray();
				if (messages.Length == 0)
				{
					break;
				}
				from = messages.Last();

				var filteredMessages = predicate == null ? messages : messages.Where(predicate);
				var cutMessages = filteredMessages.Take(count).ToArray();

				//If less messages are deleted than gathered, that means there are some that are too old meaning we can stop
				var deletedThisIteration = await DeleteMessagesAsync(channel, cutMessages, options).CAF();
				deletedCount += deletedThisIteration;
				count -= deletedThisIteration;
				if (deletedThisIteration < cutMessages.Length)
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
		public static async Task<int> DeleteMessagesAsync(ITextChannel channel, IEnumerable<IMessage> messages, RequestOptions? options)
		{
			var m = messages.Where(x => x != null && (DateTime.UtcNow - x.CreatedAt.UtcDateTime).TotalDays < 14).ToArray();
			if (m.Length == 0)
			{
				return 0;
			}

			try
			{
				if (m.Length == 1)
				{
					await m[0].DeleteAsync(options).CAF();
				}
				else
				{
					await channel.DeleteMessagesAsync(m, options).CAF();
				}
				return m.Length;
			}
			catch
			{
				return 0;
			}
		}

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
		public static Task<IUserMessage> SendMessageAsync(
			IMessageChannel channel,
			string? content = null,
			EmbedWrapper? embedWrapper = null,
			TextFileInfo? textFile = null,
			bool allowZeroWidthLengthMessages = false)
		{
			if (channel == null)
			{
				throw new ArgumentNullException(nameof(channel));
			}
			if (content == null && embedWrapper == null && textFile == null)
			{
				throw new ArgumentNullException($"{nameof(content)}, {nameof(embedWrapper)}, or {nameof(textFile)} must have a value.");
			}

			textFile ??= new TextFileInfo();

			//Make sure all the information from the embed that didn't fit goes in.
			if (embedWrapper?.Errors.Count > 0)
			{
				textFile.Name ??= "Embed_Errors";
				textFile.Text += $"Embed Errors:\n{embedWrapper}\n\n{textFile.Text}";
			}

			//Make sure none of the content mentions everyone or doesn't have the zero width character
			content = channel.SanitizeContent(content);
			if (content.Length > 2000)
			{
				textFile.Name ??= "Long_Message";
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
				if (!string.IsNullOrWhiteSpace(textFile.Name) && !string.IsNullOrWhiteSpace(textFile.Text))
				{
					using var stream = new MemoryStream();
					using var writer = new StreamWriter(stream);

					writer.Write(textFile.Text.Trim());
					writer.Flush();
					stream.Seek(0, SeekOrigin.Begin);
					return channel.SendFileAsync(stream, textFile.Name, content, embed: embedWrapper?.Build());
				}
				return channel.SendMessageAsync(content, embed: embedWrapper?.Build());
			}
			//If the message fails to send, then return the error
			catch (Exception e)
			{
				return channel.SendMessageAsync(channel.SanitizeContent(e.Message));
			}
		}

		private static string SanitizeContent(this IMessageChannel channel, string? content)
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
				//Everyone and Here have the same role
				content = content.CaseInsReplace(guildChannel.Guild.EveryoneRole.Mention, $"@{Constants.ZERO_LENGTH_CHAR}everyone");
			}
			return content
				.CaseInsReplace("@everyone", $"@{Constants.ZERO_LENGTH_CHAR}everyone")
				.CaseInsReplace("@here", $"@{Constants.ZERO_LENGTH_CHAR}here")
				.CaseInsReplace("discord.gg", $"discord{Constants.ZERO_LENGTH_CHAR}.gg")
				.CaseInsReplace("\\tts", $"\\{Constants.ZERO_LENGTH_CHAR}tts");
		}
	}
}