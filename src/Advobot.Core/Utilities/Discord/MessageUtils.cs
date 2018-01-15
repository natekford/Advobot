using Advobot.Core.Utilities.Formatting;
using Advobot.Core.Classes;
using Advobot.Core.Classes.Punishments;
using Advobot.Core.Interfaces;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using System.Net;

namespace Advobot.Core.Utilities
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
		/// <returns></returns>
		public static async Task<IUserMessage> SendMessageAsync(IMessageChannel channel, string content)
		{
			const string LONG = "The response is a long message and was sent as a text file instead";

			var guild = channel.GetGuild();
			if (guild == null)
			{
				return null;
			}

			content = DiscordObjectFormatting.FormatMessageContentForNotBeingAnnoying(guild, content);
			return content.Length < Constants.MAX_MESSAGE_LENGTH_LONG
				? await channel.SendMessageAsync(Constants.ZERO_LENGTH_CHAR + content).CAF()
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
			//Embeds have a global limit of 6000 characters
			var charCount = 0
				+ embed.Author?.Name?.Length
				+ embed.Title?.Length
				+ embed.Footer?.Text?.Length ?? 0;

			//For overflow text
			var overflowText = new StringBuilder();

			//Descriptions can only be 2048 characters max and mobile can only show up to 20 line breaks
			if (!embed.CheckIfValidDescription(charCount, out string error))
			{
				overflowText.AppendLineFeed($"Description:\n{embed.Description}");
				embed.WithDescription(error);
			}
			charCount += embed.Description?.Length ?? 0;

			//Fields can only be 1024 characters max and mobile can only show up to 5 line breaks
			for (int i = 0; i < embed.Fields.Count; ++i)
			{
				var field = embed.Fields[i];
				if (!embed.CheckIfValidField(field, charCount, out string fieldError))
				{
					overflowText.AppendLineFeed($"Field {i}; {field.Name}\n{field.Value}");
					field.WithName($"Field {i}");
					field.WithValue(fieldError);
				}

				charCount += field.Value?.ToString()?.Length ?? 0;
				charCount += field.Name?.Length ?? 0;
			}

			//Catches length errors and nsfw filter errors if an avatar has nsfw content and filtering is enabled
			var messages = new List<IUserMessage>();
			try
			{
				content = Constants.ZERO_LENGTH_CHAR + (content ?? "");
				messages.Add(await channel.SendMessageAsync(content, embed: embed.WithCurrentTimestamp().Build()).CAF());
			}
			catch (Exception e)
			{
				e.Write();
				messages.Add(await SendMessageAsync(channel, new ErrorReason(e.Message).ToString()).CAF());
			}

			//Add in the errors from the embed
			foreach (var e in embed.Errors)
			{
				overflowText.Append($"{e.Property}:\n{e.Text}{Environment.NewLine + Environment.NewLine}{e.Exception}");
			}
			//Upload the overflow
			if (overflowText.Length != 0)
			{
				messages.Add(await SendTextFileAsync(channel as ITextChannel, overflowText.ToString(), "Embed_").CAF());
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
			if (!fileName.EndsWith("_"))
			{
				fileName += "_";
			}
			var fullFileName = fileName + TimeFormatting.FormatDateTimeForSaving() + Constants.GENERAL_FILE_EXTENSION;
			var fileInfo = IOUtils.GetServerDirectoryFile(channel.GetGuild()?.Id ?? 0, fullFileName);

			IOUtils.OverWriteFile(fileInfo, text.RemoveAllMarkdown());
			var msg = await channel.SendFileAsync(fileInfo.FullName, String.IsNullOrWhiteSpace(content) ? "" : $"**{content}:**").CAF();
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
		public static async Task MakeAndDeleteSecondaryMessageAsync(IAdvobotCommandContext context, string secondStr, int time = -1)
		{
			await MakeAndDeleteSecondaryMessageAsync(context.Channel, context.Message, secondStr, time, context.Timers).CAF();
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
		public static async Task MakeAndDeleteSecondaryMessageAsync(IMessageChannel channel, IMessage message, string secondStr, int time = -1, ITimersService timers = null)
		{
			if (time < 0)
			{
				time = Constants.SECONDS_DEFAULT;
			}

			var secondMessage = await channel.SendMessageAsync(Constants.ZERO_LENGTH_CHAR + secondStr).CAF();
			if (time > 0 && timers != null)
			{
				timers.AddRemovableMessage(new RemovableMessage(time, new[] { message, secondMessage }));
			}
		}
		/// <summary>
		/// If the guild has verbose errors enabled then this acts just like <see cref="MakeAndDeleteSecondaryMessage(IMessageChannel, IMessage, string, int, ITimersService)"/>.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="reason"></param>
		/// <param name="time"></param>
		/// <returns></returns>
		public static async Task SendErrorMessageAsync(IAdvobotCommandContext context, ErrorReason reason, int time = -1)
		{
			if (context.GuildSettings.NonVerboseErrors)
			{
				return;
			}

			await MakeAndDeleteSecondaryMessageAsync(context.Channel, context.Message, reason.ToString(), time, context.Timers).CAF();
		}

		/// <summary>
		/// Returns true if no error occur.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="text"></param>
		/// <param name="url"></param>
		/// <param name="error"></param>
		/// <returns></returns>
		public static bool GetImageUrl(ICommandContext context, string text, out Uri url, out ErrorReason error)
		{
			url = null;
			error = default;

			if (text != null && !Uri.TryCreate(text, UriKind.Absolute, out url))
			{
				error = new ErrorReason("Invalid Url provided.");
			}

			if (url == null)
			{
				var attach = context.Message.Attachments.Where(x => x.Width != null && x.Height != null).Select(x => x.Url);
				var embeds = context.Message.Embeds.Where(x => x.Image.HasValue).Select(x => x.Image?.Url);
				var imageUrls = attach.Concat(embeds);
				if (imageUrls.Count() == 1)
				{
					url = new Uri(imageUrls.First());
				}
				else if (imageUrls.Count() > 1)
				{
					error = new ErrorReason("Too many attached or embedded images.");
				}
			}

			if (url != null)
			{
				var req = WebRequest.Create(url);
				req.Method = WebRequestMethods.Http.Head;
				using (var resp = req.GetResponse())
				{
					if (!Constants.VALID_IMAGE_EXTENSIONS.Contains("." + resp.Headers.Get("Content-Type").Split('/').Last()))
					{
						error = new ErrorReason("Image must be a png or jpg.");
					}
					else if (!int.TryParse(resp.Headers.Get("Content-Length"), out int ContentLength))
					{
						error = new ErrorReason("Unable to get the image's file size.");
					}
					else if (ContentLength > Constants.MAX_ICON_FILE_SIZE)
					{
						var maxSize = (double)Constants.MAX_ICON_FILE_SIZE / 1000 * 1000;
						error = new ErrorReason($"Image is bigger than {maxSize:0.0}MB. Manually upload instead.");
					}
				}
			}

			return error.Reason == null;
		}

		/// <summary>
		/// Gets the given count of messages from a channel.
		/// </summary>
		/// <param name="channel"></param>
		/// <param name="requestCount"></param>
		/// <returns></returns>
		public static async Task<List<IMessage>> GetMessagesAsync(IMessageChannel channel, int requestCount)
		{
			return (await channel.GetMessagesAsync(requestCount).FlattenAsync().CAF()).ToList();
		}

		/// <summary>
		/// Removes the given count of messages from a channel.
		/// </summary>
		/// <param name="channel"></param>
		/// <param name="fromMessage"></param>
		/// <param name="requestCount"></param>
		/// <param name="reason"></param>
		/// <returns></returns>
		public static async Task<int> RemoveMessagesAsync(ITextChannel channel, IMessage fromMessage, int requestCount, ModerationReason reason)
		{
			var messages = await channel.GetMessagesAsync(fromMessage, Direction.Before, requestCount).FlattenAsync().CAF();
			return await DeleteMessagesAsync(channel, messages, reason).CAF();
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
		public static async Task<int> RemoveMessagesFromUserAsync(ITextChannel channel, IMessage fromMessage, int requestCount, IUser user, ModerationReason reason)
		{
			var deletedCount = 0;
			while (requestCount > 0)
			{
				var messages = await channel.GetMessagesAsync(fromMessage, Direction.Before, 100).FlattenAsync().CAF();
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

				var cutUserMessages = userMessages.ToList().TakeMin(requestCount, 100);
				deletedCount += await DeleteMessagesAsync(channel, cutUserMessages, reason).CAF();

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
		public static async Task<int> DeleteMessagesAsync(ITextChannel channel, IEnumerable<IMessage> messages, ModerationReason reason)
		{
			//13.95 for some buffer in case
			var youngMessages = messages.Where(x => x != null && DateTime.UtcNow.Subtract(x.CreatedAt.UtcDateTime).TotalDays < 13.95);
			try
			{
				await channel.DeleteMessagesAsync(youngMessages, reason.CreateRequestOptions()).CAF();
				return youngMessages.Count();
			}
			catch
			{
				ConsoleUtils.WriteLine($"Unable to delete {youngMessages.Count()} messages on the guild {channel.GetGuild().FormatGuild()} on channel {channel.FormatChannel()}.", color: ConsoleColor.Red);
				return 0;
			}
		}
		/// <summary>
		/// Deletes the passed in message directly.
		/// </summary>
		/// <param name="message"></param>
		/// <returns></returns>
		public static async Task<int> DeleteMessageAsync(IMessage message, ModerationReason reason)
		{
			if (message == null || DateTime.UtcNow.Subtract(message.CreatedAt.UtcDateTime).TotalDays > 13.95)
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
				ConsoleUtils.WriteLine($"Unable to delete the message {message.Id} on channel {message.Channel.FormatChannel()}.", color: ConsoleColor.Red);
				return 0;
			}
		}
	}
}