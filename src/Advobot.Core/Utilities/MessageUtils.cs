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
			const string INVITE_3 = "discord.com";
			const string SANITIZED_INVITE_3 = INVITE_3 + SPACE;

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
				.CaseInsReplace(INVITE_2, SANITIZED_INVITE_2)
				.CaseInsReplace(INVITE_3, SANITIZED_INVITE_3);
		}

		/// <summary>
		/// Sends a message to the given channel with the given content.
		/// </summary>
		/// <param name="channel"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		public static Task<IUserMessage> SendMessageAsync(
			this IMessageChannel channel,
			MessageArgs? args)
		{
			args ??= new MessageArgs();

			if (args.Content is null && args.Embed is null && args.File is null)
			{
				throw new ArgumentNullException("A sendable value must exist (content, embed, or file).");
			}
			if (channel is null)
			{
				if (args.AllowNullChannel)
				{
					return Task.FromResult<IUserMessage>(null!);
				}
				throw new ArgumentNullException(nameof(channel));
			}

			// Make sure all the information from the embed that didn't fit goes in the text file
			if (args.Embed?.Errors?.Count > 0)
			{
				args.File ??= new TextFileInfo();
				args.File.Name ??= "Embed_Errors";
				args.File.Text += $"Embed Errors:\n{args.Embed}\n\n{args.File.Text}";
			}

			// Make sure the content removes annoying parts
			args.Content = args.Content.Sanitize();
			if (args.Content.Length > 2000)
			{
				args.File ??= new TextFileInfo();
				args.File.Name ??= "Long_Message";
				args.File.Text += $"Message Content:\n{args.Content}\n\n{args.File.Text}";
				args.Content = $"{Constants.ZERO_WIDTH_SPACE}Response is too long; sent as text file instead.";
			}

			// Can clear the content if it's going to only be a zero length space
			// and there's an embed or a file
			// Otherwise there will be unecessary empty space
			if (!args.AllowZeroWidthLengthMessages
				&& args.Content == Constants.ZERO_WIDTH_SPACE
				&& (args.Embed != null || args.File != null))
			{
				args.Content = "";
			}

			try
			{
				var built = args.Embed?.Build();
				// If the file name and text exists, then attempt to send as a file
				if (args.File != null
					&& !string.IsNullOrWhiteSpace(args.File.Name)
					&& !string.IsNullOrWhiteSpace(args.File.Text))
				{
					using var stream = new MemoryStream();
					using var writer = new StreamWriter(stream);

					writer.Write(args.File.Text);
					writer.Flush();
					stream.Seek(0, SeekOrigin.Begin);

					return channel.SendFileAsync(
						stream,
						args.File.Name,
						args.Content,
						args.IsTTS,
						built,
						args.Options,
						args.IsSpoiler,
						args.AllowedMentions
					);
				}

				return channel.SendMessageAsync(
					args.Content,
					args.IsTTS,
					built,
					args.Options,
					args.AllowedMentions
				);
			}
			// If the message fails to send, then return the error
			catch (Exception e)
			{
				return channel.SendMessageAsync(
					e.Message.Sanitize(),
					false,
					null,
					args.Options,
					args.AllowedMentions
				);
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

	/// <summary>
	/// Arguments used for sending a message.
	/// </summary>
	public sealed class MessageArgs
	{
		/// <summary>
		/// The allowed mentions of the message. By default this is None.
		/// </summary>
		public AllowedMentions AllowedMentions { get; set; } = new AllowedMentions();
		/// <summary>
		/// Whether or not to allow null channels. If true, a null message will be returned. If false, an exception will occur.
		/// </summary>
		public bool AllowNullChannel { get; set; }
		/// <summary>
		/// Whether or not to allow zero width space messages.
		/// </summary>
		public bool AllowZeroWidthLengthMessages { get; set; }
		/// <summary>
		/// The content of the message. If this is null, no message will be sent.
		/// </summary>
		public string? Content { get; set; }
		/// <summary>
		/// The embed of the message. If this is null, no embed will be sent.
		/// </summary>
		public EmbedWrapper? Embed { get; set; }
		/// <summary>
		/// The file of the message. If this is null, a file will only be sent if there are errors.
		/// </summary>
		public TextFileInfo? File { get; set; }
		/// <summary>
		/// Whether or not this message should be spoilered.
		/// </summary>
		public bool IsSpoiler { get; set; }
		/// <summary>
		/// Whether or not this message should use text to speech.
		/// </summary>
		public bool IsTTS { get; set; }
		/// <summary>
		/// Request options to use when sending the message.
		/// </summary>
		public RequestOptions? Options { get; set; }
	}
}