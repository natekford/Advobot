using System;
using System.Collections.Generic;
using System.IO;
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
		private static readonly TimeSpan OldestAllowed = TimeSpan.FromDays(13.9);

		/// <summary>
		/// Removes the given count of messages from a channel.
		/// </summary>
		/// <param name="channel"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		public static async Task<int> DeleteMessagesAsync(
			this ITextChannel channel,
			DeleteMessageArgs args)
		{
			var deleteCount = args.DeleteCount;
			var from = args.FromMessage;
			var messages = new List<IMessage>();

			// Early exit of the method
			// Delete any messages still in the list
			// Return starting request count - remaining + list count
			static async Task<int> DoneAsync(
				ITextChannel channel,
				List<IMessage> messages,
				int remaining,
				DeleteMessageArgs args)
			{
				if (messages.Count == 1)
				{
					await messages[0].DeleteAsync(args.Options).CAF();
				}
				else if (messages.Count > 1)
				{
					await channel.DeleteMessagesAsync(messages, args.Options).CAF();
				}
				return args.DeleteCount - remaining + messages.Count;
			}

			// Intermediary step of the method
			// Delete all messages in the list and clear it
			// Return the amount of messages deleted
			static async Task<int> DeleteBatchAsync(
				ITextChannel channel,
				List<IMessage> messages,
				DeleteMessageArgs args)
			{
				await channel.DeleteMessagesAsync(messages, args.Options).CAF();
				var count = messages.Count;
				messages.Clear();
				return count;
			}

			while (deleteCount > 0)
			{
				var startCount = messages.Count;
				// We can't pass in a null message/id so if/else to get the right method
				var request = from == null
					? channel.GetMessagesAsync(options: args.Options)
					: channel.GetMessagesAsync(from, Direction.Before, options: args.Options);
				await foreach (var batch in request)
				{
					foreach (var message in batch)
					{
						from = message;

						// Messages are too old to bulk delete, stop looking
						if (args.Now - message.CreatedAt.UtcDateTime > OldestAllowed)
						{
							return await DoneAsync(channel, messages, deleteCount, args).CAF();
						}
						if (args.Predicate == null || args.Predicate(message))
						{
							messages.Add(message);
						}

						// We have the requested amount of message to delete
						if (messages.Count == deleteCount)
						{
							return await DoneAsync(channel, messages, deleteCount, args).CAF();
						}
						// We have reached the max count of messages we can delete in one batch
						if (messages.Count == DiscordConfig.MaxMessagesPerBatch)
						{
							deleteCount -= await DeleteBatchAsync(channel, messages, args).CAF();
							startCount = int.MaxValue;
						}
					}
				}

				// Haven't found any matching messages in the past batch, don't waste time
				// endlessly searching
				if (startCount == messages.Count)
				{
					return await DoneAsync(channel, messages, deleteCount, args).CAF();
				}
			}
			return args.DeleteCount;
		}

		/// <summary>
		/// Sends a message to the given channel with the given content.
		/// </summary>
		/// <param name="channel"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		public static async Task<IUserMessage> SendMessageAsync(
			this IMessageChannel channel,
			SendMessageArgs args)
		{
			if (args.Content is null && args.Embed is null && args.File is null)
			{
				throw new ArgumentNullException("A sendable value must exist (content, embed, or file).");
			}
			if (channel is null)
			{
				if (args.AllowNullChannel)
				{
					return null!;
				}
				throw new ArgumentNullException(nameof(channel));
			}

			// Make sure all the information from the embed that didn't fit goes in the text file
			if (args.Embed?.Errors?.Count > 0)
			{
				args.File ??= new();
				args.File.Name ??= "Embed_Errors";
				args.File.Text += $"Embed Errors:\n{args.Embed}\n\n{args.File.Text}";
			}

			// Make sure the content removes annoying parts
			args.Content = args.Content.Sanitize();
			if (args.Content.Length > 2000)
			{
				args.File ??= new();
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

					return await channel.SendFileAsync(
						stream,
						args.File.Name,
						args.Content,
						args.IsTTS,
						built,
						args.Options,
						args.IsSpoiler,
						args.AllowedMentions
					).CAF();
				}

				return await channel.SendMessageAsync(
					args.Content,
					args.IsTTS,
					built,
					args.Options,
					args.AllowedMentions
				).CAF();
			}
			// If the message fails to send, then return the error
			catch (Exception e)
			{
				return await channel.SendMessageAsync(
					e.Message.Sanitize(),
					false,
					null,
					args.Options,
					args.AllowedMentions
				).CAF();
			}
		}

		private static string Sanitize(this string? content)
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
	}

	/// <summary>
	/// Arguments used for deleting a message.
	/// </summary>
	public sealed class DeleteMessageArgs
	{
		/// <summary>
		/// The amount of messages to delete.
		/// </summary>
		public int DeleteCount { get; set; }
		/// <summary>
		/// The message to start deleting at. This message will also be deleted.
		/// </summary>
		public IMessage? FromMessage { get; set; }
		/// <summary>
		/// The current time.
		/// </summary>
		public DateTimeOffset Now { get; set; } = DateTimeOffset.UtcNow;
		/// <summary>
		/// Request options to use when deleting messages.
		/// </summary>
		public RequestOptions? Options { get; set; }
		/// <summary>
		/// Determines matching messages.
		/// </summary>
		public Func<IMessage, bool>? Predicate { get; set; }
	}

	/// <summary>
	/// Arguments used for sending a message.
	/// </summary>
	public sealed class SendMessageArgs
	{
		/// <summary>
		/// The allowed mentions of the message. By default this is None.
		/// </summary>
		public AllowedMentions? AllowedMentions { get; set; } = new();
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