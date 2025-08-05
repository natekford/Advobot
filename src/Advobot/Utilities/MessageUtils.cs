using Advobot.Embeds;

using AdvorangesUtils;

using Discord;

namespace Advobot.Utilities;

/// <summary>
/// Actions which are done on an <see cref="IMessage"/>.
/// </summary>
public static class MessageUtils
{
	private const string TOO_LONG = $"{Constants.ZERO_WIDTH_SPACE}Response is too long; sent as text file instead.";
	private static readonly TimeSpan OldestAllowed = TimeSpan.FromDays(13.9);

	/// <summary>
	/// Creates a text file attachment in memory.
	/// </summary>
	/// <param name="fileName"></param>
	/// <param name="content"></param>
	/// <returns></returns>
	public static FileAttachment CreateTextFile(string fileName, string content)
	{
		var sanitized = fileName.Replace(' ', '_').TrimEnd('_');
		var newFileName = $"{sanitized}_{DateTime.UtcNow:yyyyMMdd_hhmmss}.txt";

		// We are not disposing the writer since the created FileAttachment
		// should get disposed, which disposes the base stream
		var writer = new StreamWriter(new MemoryStream());
		writer.Write(content);
		writer.Flush();
		writer.BaseStream.Seek(0, SeekOrigin.Begin);

		return new(writer.BaseStream, newFileName);
	}

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
	/// <exception cref="InvalidOperationException"></exception>
	public static async Task<IUserMessage> SendMessageAsync(
		this IMessageChannel channel,
		SendMessageArgs args)
	{
		if (args.Content is null && args.Embeds is null && args.Files is null)
		{
			throw new InvalidOperationException("A sendable value must exist (content, embed, or file).");
		}
		if (args.Embeds?.Length > 10)
		{
			throw new InvalidOperationException("Cannot have more than 10 embeds.");
		}
		if (args.Files?.Count > 9)
		{
			throw new InvalidOperationException("Cannot have more than 9 files. 1 is always reserved for errors.");
		}
		if (channel is null)
		{
			if (args.AllowNullChannel)
			{
				return null!;
			}
			throw new InvalidOperationException("Cannot send to a null channel.");
		}

		// Make sure the content is sanitized, and any overly long messages are added
		// to the error file
		args.Content = args.Content.Sanitize();
		if (args.Content.Length > 2000)
		{
			args.Errors ??= new(new MemoryStream());
			args.Errors.WriteLine("Message Content:");
			args.Errors.WriteLine(args.Content);
			args.Errors.WriteLine();
			args.Content = TOO_LONG;
		}

		// If there are any errors, add them to the files to be sent
		if (args.Errors is not null)
		{
			args.Files ??= new(1);

			args.Errors.Flush();
			args.Errors.BaseStream.Seek(0, SeekOrigin.Begin);
			args.Files.Add(new FileAttachment(args.Errors.BaseStream, "Errors.txt"));
		}

		// Can clear the content if it's going to only be a zero length space
		// and there's an embed or a file
		// Otherwise there will be unecessary empty space
		if (!args.AllowZeroWidthLengthMessages
			&& args.Content == Constants.ZERO_WIDTH_SPACE
			&& (args.Embeds?.Length > 0 || args.Files?.Count > 0))
		{
			args.Content = "";
		}

		try
		{
			// If the file name and text exists, then attempt to send as a file
			if (args.Files?.Count > 0)
			{
				return await channel.SendFilesAsync(
					args.Files,
					args.Content,
					args.IsTTS,
					null,
					args.Options,
					args.AllowedMentions,
					args.MessageReference,
					args.Components,
					args.Stickers,
					args.Embeds
				).CAF();
			}

			return await channel.SendMessageAsync(
				args.Content,
				args.IsTTS,
				null,
				args.Options,
				args.AllowedMentions,
				args.MessageReference,
				args.Components,
				args.Stickers,
				args.Embeds
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
		finally
		{
			if (args.Files?.Count > 0)
			{
				foreach (var file in args.Files)
				{
					file.Dispose();
				}
			}
			args.Errors?.Dispose();
		}
	}

	private static string Sanitize(this string? content)
	{
		const string SPACE = Constants.ZERO_WIDTH_SPACE;
		const string EVERYONE = "everyone";
		const string EVERYONE_CLEAN = SPACE + EVERYONE;
		const string HERE = "here";
		const string HERE_CLEAN = SPACE + HERE;
		const string INVITE = "discord.gg";
		const string INVITE_CLEAN = INVITE + SPACE;
		const string INVITE_2 = "discordapp.com/invite";
		const string INVITE_2_CLEAN = INVITE_2 + SPACE;
		const string INVITE_3 = "discord.com";
		const string INVITE_3_CLEAN = INVITE_3 + SPACE;

		if (content == null)
		{
			return SPACE;
		}
		else if (!content.StartsWith(SPACE))
		{
			content = SPACE + content;
		}

		return content
			.Replace(EVERYONE, EVERYONE_CLEAN)
			.Replace(HERE, HERE_CLEAN)
			.CaseInsReplace(INVITE, INVITE_CLEAN)
			.CaseInsReplace(INVITE_2, INVITE_2_CLEAN)
			.CaseInsReplace(INVITE_3, INVITE_3_CLEAN);
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
	/// This is used to ensure messages older than 14 days aren't bulk deleted.
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
	public AllowedMentions? AllowedMentions { get; set; } = AllowedMentions.None;
	/// <summary>
	/// Whether or not to allow null channels.
	/// If true, a null message will be returned.
	/// If false, an exception will occur.
	/// </summary>
	public bool AllowNullChannel { get; set; }
	/// <summary>
	/// Whether or not to allow zero width space messages.
	/// </summary>
	public bool AllowZeroWidthLengthMessages { get; set; }
	/// <summary>
	/// The message component to use for interaction.
	/// If this is null, no interaction will be sent.
	/// </summary>
	public MessageComponent? Components { get; set; }
	/// <summary>
	/// The content of the message. If this is null, no message will be sent.
	/// </summary>
	public string? Content { get; set; }
	/// <summary>
	/// The embeds of the message.
	/// If this is null, no embeds will be sent.
	/// There is a maximum of 10 embeds allowed.
	/// </summary>
	public Embed[]? Embeds { get; set; }
	/// <summary>
	/// Any errors which have occurred while validating the message arguments.
	/// </summary>
	public StreamWriter? Errors { get; set; }
	/// <summary>
	/// The files of the message.
	/// If this is null, a file will only be sent if there are errors.
	/// There is a maximum of 9 files allowed,
	/// 1 is always reserved for sending <see cref="Errors"/>.
	/// </summary>
	public List<FileAttachment>? Files { get; set; }
	/// <summary>
	/// Whether or not this message should use text to speech.
	/// </summary>
	public bool IsTTS { get; set; }
	/// <summary>
	/// The message to reply to. If this is null, no message is being replied to.
	/// </summary>
	public MessageReference? MessageReference { get; set; }
	/// <summary>
	/// Request options to use when sending the message.
	/// </summary>
	public RequestOptions? Options { get; set; }
	/// <summary>
	/// The stickers to send with this message.
	/// If this is null, no stickers will be sent.
	/// </summary>
	public ISticker[]? Stickers { get; set; }

	/// <summary>
	/// Creates an instance of <see cref="SendMessageArgs"/>.
	/// </summary>
	public SendMessageArgs()
	{
	}

	/// <summary>
	/// Creates an instance of <see cref="SendMessageArgs"/> and adds all errors from
	/// the passed in embed to <see cref="Errors"/>.
	/// </summary>
	/// <param name="embed"></param>
	public SendMessageArgs(EmbedWrapper? embed)
	{
		if (embed is null)
		{
			return;
		}

		// Make sure any errors get put into the error file
		if (embed.Errors?.Count > 0)
		{
			Errors ??= new(new MemoryStream());
			Errors.WriteLine("Embed Errors:");
			Errors.WriteLine(embed.ToString());
			Errors.WriteLine();
		}
		Embeds = [embed.Build()];
	}
}