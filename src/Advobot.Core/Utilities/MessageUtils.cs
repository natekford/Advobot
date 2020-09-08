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
		/// The oldest messages are allowed to be when bulk deleting.
		/// </summary>
		public static readonly TimeSpan OldestAllowed = TimeSpan.FromDays(14);
		private static readonly AllowedMentions None = new AllowedMentions();

		/// <summary>
		/// Removes the given count of messages from a channel.
		/// </summary>
		/// <param name="channel"></param>
		/// <param name="from"></param>
		/// <param name="requestCount"></param>
		/// <param name="now"></param>
		/// <param name="options"></param>
		/// <param name="predicate"></param>
		/// <returns></returns>
		public static async Task<int> DeleteMessagesAsync(
			ITextChannel channel,
			IMessage? from,
			int requestCount,
			DateTimeOffset now,
			RequestOptions options,
			Func<IMessage, bool>? predicate = null)
		{
			if (from == null)
			{
				return 0;
			}

			var totalDeletedCount = 0;
			while (requestCount > 0)
			{
				const int MAX_GATHER = 100;

				var get = channel.GetMessagesAsync(from, Direction.Before, MAX_GATHER);
				var retrieved = await get.FirstOrDefaultAsync().CAF();
				if (retrieved.Count == 0)
				{
					break;
				}

				var filtered = FilterMessages(retrieved, requestCount, predicate);
				var deletedCount = await DeleteMessagesAsync(channel, filtered, now, options).CAF();
				totalDeletedCount += deletedCount;
				requestCount -= deletedCount;
				//If less messages are deleted than gathered,
				//that means there are some that are too old meaning we can stop
				if (deletedCount < retrieved.Count)
				{
					break;
				}

				from = retrieved.ElementAt(retrieved.Count - 1);
			}
			return totalDeletedCount;
		}

		/// <summary>
		/// Deletes the passed in messages directly. Will only delete messages under 14 days old.
		/// </summary>
		/// <param name="channel"></param>
		/// <param name="messages"></param>
		/// <param name="now"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		public static async Task<int> DeleteMessagesAsync(
			ITextChannel channel,
			IEnumerable<IMessage> messages,
			DateTimeOffset now,
			RequestOptions? options)
		{
			var m = messages.Where(x => now - x.CreatedAt.UtcDateTime < OldestAllowed).ToArray();
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
				return -1;
			}
		}

		/// <summary>
		/// Sanitizes a message's content so it won't mention everyone or link invites.
		/// </summary>
		/// <param name="content"></param>
		/// <returns></returns>
		public static string Sanitize(this string? content)
		{
			const string SPACE = Constants.ZERO_WIDTH_SPACE;
			const string EVERYONE = "everyone";
			const string SANITIZED_EVERYONE = SPACE + EVERYONE;
			const string HERE = "here";
			const string SANITIZED_HERE = SPACE + HERE;
			const string INVITE = "discord.gg";
			const string SANITIZED_INVITE = INVITE + SPACE;
			const string INVITE_2 = "discordapp.com/invite";
			const string SANITIZED_INVITE_2 = INVITE_2 + SPACE;

			if (content == null)
			{
				return SPACE;
			}
			else if (!content.StartsWith(SPACE))
			{
				content = SPACE + content;
			}

			return content
				.Replace(EVERYONE, SANITIZED_EVERYONE)
				.Replace(HERE, SANITIZED_HERE)
				.CaseInsReplace(INVITE, SANITIZED_INVITE)
				.CaseInsReplace(INVITE_2, SANITIZED_INVITE_2);
		}

		/// <summary>
		/// Sends a message to the given channel with the given content.
		/// </summary>
		/// <param name="channel"></param>
		/// <param name="content"></param>
		/// <param name="embed"></param>
		/// <param name="file"></param>
		/// <param name="mentions"></param>
		/// <param name="allowZeroWidthLengthMessages">
		/// <param name="nullChannelIsException"/>
		/// If there is no content passed in the content will become only a single zero width space.
		/// This ends up taking up extra space if used with embeds or files.
		/// </param>
		/// <returns></returns>
		public static Task<IUserMessage> SendMessageAsync(
			IMessageChannel channel,
			string? content = null,
			EmbedWrapper? embed = null,
			TextFileInfo? file = null,
			AllowedMentions? mentions = null,
			bool allowZeroWidthLengthMessages = false,
			bool nullChannelIsException = true)
		{
			if (channel == null)
			{
				if (nullChannelIsException)
				{
					throw new ArgumentNullException(nameof(channel));
				}
				return Task.FromResult<IUserMessage>(null!);
			}
			if (content == null && embed == null && file == null)
			{
				throw new ArgumentNullException($"{nameof(content)}, {nameof(embed)}, or {nameof(file)} must have a value.");
			}

			file ??= new TextFileInfo();
			mentions ??= None;

			//Make sure all the information from the embed that didn't fit goes in.
			if (embed?.Errors.Count > 0)
			{
				file.Name ??= "Embed_Errors";
				file.Text += $"Embed Errors:\n{embed}\n\n{file.Text}";
			}

			//Make sure none of the content mentions everyone or doesn't have the zero width character
			content = content.Sanitize();
			if (content.Length > 2000)
			{
				file.Name ??= "Long_Message";
				file.Text += $"Message Content:\n{content}\n\n{file.Text}";
				content = $"{Constants.ZERO_WIDTH_SPACE}Response is too long; sent as text file instead.";
			}

			//Can clear the content if it's going to only be a zero length space and there's an embed
			//Otherwise there will be unecessary empty space
			if (!allowZeroWidthLengthMessages && content == Constants.ZERO_WIDTH_SPACE && embed != null)
			{
				content = "";
			}

			try
			{
				var built = embed?.Build();
				//If the file name and text exists, then attempt to send as a file instead of message
				if (!string.IsNullOrWhiteSpace(file.Name) && !string.IsNullOrWhiteSpace(file.Text))
				{
					using var stream = new MemoryStream();
					using var writer = new StreamWriter(stream);

					writer.Write(file.Text.Trim());
					writer.Flush();
					stream.Seek(0, SeekOrigin.Begin);
					return channel.SendFileAsync(stream, file.Name, content, embed: built);
				}
				return channel.SendMessageAsync(content, embed: built, allowedMentions: mentions);
			}
			//If the message fails to send, then return the error
			catch (Exception e)
			{
				return channel.SendMessageAsync(e.Message.Sanitize(), allowedMentions: mentions);
			}
		}

		private static IEnumerable<IMessage> FilterMessages(
			IReadOnlyCollection<IMessage> messages,
			int requestCount,
			Func<IMessage, bool>? predicate)
		{
			IEnumerable<IMessage> filtered = messages;
			if (predicate != null)
			{
				filtered = filtered.Where(predicate);
			}
			if (requestCount < messages.Count)
			{
				filtered = filtered.Take(requestCount);
			}
			return filtered;
		}
	}
}