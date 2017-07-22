using Advobot.Enums;
using Advobot.Interfaces;
using Advobot.RemovablePunishments;
using Advobot.SavedClasses;
using Advobot.Structs;
using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Advobot
{
	namespace Actions
	{
		public static class MessageActions
		{
			public static async Task<IUserMessage> SendEmbedMessage(IMessageChannel channel, EmbedBuilder embed, string content = null)
			{
				var guild = channel.GetGuild();
				if (guild == null)
					return null;

				//Embeds have a global limit of 6000 characters
				var totalChars = 0
					+ embed?.Author?.Name?.Length
					+ embed?.Title?.Length
					+ embed?.Footer?.Text?.Length;

				//Descriptions can only be 2048 characters max and mobile can only show up to 20 line breaks
				string badDesc = null;
				if (embed.Description?.Length > Constants.MAX_DESCRIPTION_LENGTH)
				{
					badDesc = embed.Description;
					embed.WithDescription(String.Format("The description is over `{0}` characters and will be sent as a text file instead.", Constants.MAX_DESCRIPTION_LENGTH));
				}
				else if (embed.Description.GetLineBreaks() > Constants.MAX_DESCRIPTION_LINES)
				{
					badDesc = embed.Description;
					embed.WithDescription(String.Format("The description is over `{0}` lines and will be sent as a text file instead.", Constants.MAX_DESCRIPTION_LINES));
				}
				totalChars += embed.Description?.Length ?? 0;

				//Embeds can only be 1024 characters max and mobile can only show up to 5 line breaks
				var badFields = new List<Tuple<int, string>>();
				for (int i = 0; i < embed.Fields.Count; ++i)
				{
					var field = embed.Fields[i];
					var value = field.Value.ToString();
					if (totalChars > Constants.MAX_EMBED_TOTAL_LENGTH - 1500)
					{
						badFields.Add(new Tuple<int, string>(i, value));
						field.WithName(i.ToString());
						field.WithValue(String.Format("`{0}` char limit close.", Constants.MAX_EMBED_TOTAL_LENGTH));
					}
					else if (value?.Length > Constants.MAX_FIELD_VALUE_LENGTH)
					{
						badFields.Add(new Tuple<int, string>(i, value));
						field.WithValue(String.Format("This field is over `{0}` characters and will be sent as a text file instead.", Constants.MAX_FIELD_VALUE_LENGTH));
					}
					else if (value.GetLineBreaks() > Constants.MAX_FIELD_LINES)
					{
						badFields.Add(new Tuple<int, string>(i, value));
						field.WithValue(String.Format("This field is over `{0}` lines and will be sent as a text file instead.", Constants.MAX_FIELD_LINES));
					}
					totalChars += value?.Length ?? 0;
					totalChars += field.Name?.Length ?? 0;
				}

				IUserMessage msg;
				try
				{
					if (content != null)
					{
						content = content.CaseInsReplace(guild.EveryoneRole.Mention, Constants.FAKE_EVERYONE);
						content = content.CaseInsReplace("@everyone", Constants.FAKE_EVERYONE);
						content = content.CaseInsReplace("\tts", Constants.FAKE_TTS);
					}

					msg = await channel.SendMessageAsync(Constants.ZERO_LENGTH_CHAR + content ?? "", false, embed.WithCurrentTimestamp());
				}
				catch (Exception e)
				{
					ConsoleActions.ExceptionToConsole(e);
					msg = await channel.SendMessageAsync(Constants.ZERO_LENGTH_CHAR + FormattingActions.ERROR(e.Message));
					return null;
				}

				//Go send the description/fields that had an error
				if (badDesc != null)
				{
					await UploadActions.WriteAndUploadTextFile(guild, channel, badDesc, "Description_");
				}
				foreach (var tuple in badFields)
				{
					var num = tuple.Item1;
					var val = tuple.Item2;
					await UploadActions.WriteAndUploadTextFile(guild, channel, val, String.Format("Field_{0}_", num));
				}

				return msg;
			}
			public static async Task<IUserMessage> SendChannelMessage(ICommandContext context, string content)
			{
				return await SendChannelMessage(context.Channel, content);
			}
			public static async Task<IUserMessage> SendChannelMessage(IMessageChannel channel, string content)
			{
				var guild = (channel as ITextChannel)?.Guild;
				if (guild == null)
					return null;

				content = content.CaseInsReplace(guild.EveryoneRole.Mention, Constants.FAKE_EVERYONE);
				content = content.CaseInsReplace("@everyone", Constants.FAKE_EVERYONE);
				content = content.CaseInsReplace("\tts", Constants.FAKE_TTS);

				IUserMessage msg = null;
				if (content.Length >= Constants.MAX_MESSAGE_LENGTH_LONG)
				{
					msg = await UploadActions.WriteAndUploadTextFile(guild, channel, content, "Long_Message_", "The response is a long message and was sent as a text file instead");
				}
				else
				{
					msg = await channel.SendMessageAsync(Constants.ZERO_LENGTH_CHAR + content);
				}
				return msg;
			}
			public static async Task<IUserMessage> SendDMMessage(IDMChannel channel, string message)
			{
				if (channel == null)
					return null;

				return await channel.SendMessageAsync(Constants.ZERO_LENGTH_CHAR + message);
			}

			public static async Task<int> RemoveMessages(IMessageChannel channel, IMessage fromMessage, int requestCount)
			{
				var guildChannel = channel as ITextChannel;
				if (guildChannel == null)
					return 0;

				var messages = await channel.GetMessagesAsync(fromMessage, Direction.Before, requestCount).Flatten();
				await DeleteMessages(channel, messages);
				return messages.Count();
			}
			public static async Task<int> RemoveMessages(IMessageChannel channel, IMessage fromMessage, int requestCount, IUser user)
			{
				var guildChannel = channel as ITextChannel;
				if (guildChannel == null)
					return 0;

				if (user == null)
				{
					return await RemoveMessages(channel, fromMessage, requestCount);
				}

				var deletedCount = 0;
				while (requestCount > 0)
				{
					//Get the current messages and ones that aren't null
					var messages = await channel.GetMessagesAsync(fromMessage, Direction.Before, 100).Flatten();
					if (!messages.Any())
						break;

					//Set the from message as the last of the currently grabbed ones
					fromMessage = messages.Last();

					//Check for messages of the targetted user
					messages = messages.Where(x => x.Author.Id == user.Id);
					if (!messages.Any())
						break;

					var gatheredForUserAmt = messages.Count();
					messages = messages.ToList().GetUpToAndIncludingMinNum(requestCount, gatheredForUserAmt, 100);

					//Delete them in a try catch due to potential errors
					var msgAmt = messages.Count();
					try
					{
						await DeleteMessages(channel, messages);
						deletedCount += msgAmt;
					}
					catch
					{
						ConsoleActions.WriteLine(String.Format("Unable to delete {0} messages on the guild {1} on channel {2}.", msgAmt, guildChannel.Guild.FormatGuild(), guildChannel.FormatChannel()));
						break;
					}

					//Leave if the message count gathered implies that enough user messages have been deleted 
					if (msgAmt < gatheredForUserAmt)
						break;

					requestCount -= msgAmt;
				}
				return deletedCount;
			}
			public static async Task<List<IMessage>> GetMessages(IMessageChannel channel, int requestCount)
			{
				return (await channel.GetMessagesAsync(++requestCount).Flatten()).ToList();
			}

			public static async Task MakeAndDeleteSecondaryMessage(IMyCommandContext context, string secondStr, int time = -1)
			{
				await MakeAndDeleteSecondaryMessage(context.Channel, context.Message, secondStr, time, context.Timers);
			}
			public static async Task MakeAndDeleteSecondaryMessage(IMessageChannel channel, IUserMessage message, string secondStr, int time = -1, ITimersModule timers = null)
			{
				var secondMsg = await channel.SendMessageAsync(Constants.ZERO_LENGTH_CHAR + secondStr);
				var messages = new List<IMessage> { secondMsg, message };

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
					timers.AddRemovableMessages(new RemovableMessage(messages, time));
				}
			}
			public static void RemoveCommandMessage(IMessage message, int time = 0, ITimersModule timers = null)
			{
				if (time > 0 && timers != null)
				{
					timers.AddRemovableMessages(new RemovableMessage(message, time));
				}
			}

			public static async Task DeleteMessages(IMessageChannel channel, IEnumerable<IMessage> messages)
			{
				if (messages == null || !messages.Any())
					return;

				try
				{
					await channel.DeleteMessagesAsync(messages.Where(x => DateTime.UtcNow.Subtract(x.CreatedAt.UtcDateTime).TotalDays < 14).Distinct());
				}
				catch
				{
					ConsoleActions.WriteLine(String.Format("Unable to delete {0} messages on the guild {1} on channel {2}.", messages.Count(), channel.GetGuild().FormatGuild(), channel.FormatChannel()));
				}
			}
			public static async Task DeleteMessage(IMessage message)
			{
				if (message == null || DateTime.UtcNow.Subtract(message.CreatedAt.UtcDateTime).TotalDays >= 14)
					return;

				try
				{
					await message.DeleteAsync();
				}
				catch
				{
					ConsoleActions.WriteLine(String.Format("Unable to delete the message {0} on channel {1}.", message.Id, message.Channel.FormatChannel()));
				}
			}
			public static async Task SendMessageContainingFormattedDeletedMessages(IGuild guild, ITextChannel channel, List<string> inputList)
			{
				if (!inputList.Any())
				{
					return;
				}

				var characterCount = 0;
				inputList.ForEach(x => characterCount += (x.Length + 100));

				if (inputList.Count <= 5 && characterCount < Constants.MAX_MESSAGE_LENGTH_LONG)
				{
					var embed = EmbedActions.MakeNewEmbed("Deleted Messages", String.Join("\n", inputList), Constants.MDEL);
					EmbedActions.AddFooter(embed, "Deleted Messages");
					await SendEmbedMessage(channel, embed);
				}
				else
				{
					var text = FormattingActions.RemoveMarkdownChars(String.Join("\n-----\n", inputList), true);
					var name = "Deleted_Messages_";
					var content = String.Format("{0} Deleted Messages", inputList.Count);
					await UploadActions.WriteAndUploadTextFile(guild, channel, text, name, content);
				}
			}

			public static async Task SendGuildNotification(IUser user, GuildNotification notification)
			{
				if (notification == null)
					return;

				var content = notification.Content;
				content = content.CaseInsReplace("{UserMention}", user != null ? user.Mention : "Invalid User");
				content = content.CaseInsReplace("{User}", user != null ? user.FormatUser() : "Invalid User");
				//Put a zero length character in between invite links for names so the invite links will no longer embed

				if (notification.Embed != null)
				{
					await SendEmbedMessage(notification.Channel, notification.Embed, content);
				}
				else
				{
					await SendChannelMessage(notification.Channel, content);
				}
			}

			public static async Task HandleObjectGettingErrors<T>(IMyCommandContext context, ReturnedObject<T> returnedObject)
			{
				await MakeAndDeleteSecondaryMessage(context, FormattingActions.FormatErrorString(context.Guild, returnedObject.Reason, returnedObject.Object));
			}
			public static async Task HandleArgsGettingErrors(IMyCommandContext context, ReturnedArguments returnedArgs)
			{
				//TODO: Remove my own arg parsing.
				switch (returnedArgs.Reason)
				{
					case FailureReason.TooMany:
					{
						await MakeAndDeleteSecondaryMessage(context, FormattingActions.ERROR("Too many arguments."));
						return;
					}
					case FailureReason.TooFew:
					{
						await MakeAndDeleteSecondaryMessage(context, FormattingActions.ERROR("Too few arguments."));
						return;
					}
					/*
					case FailureReason.MissingCriticalArgs:
					{
						await MakeAndDeleteSecondaryMessage(context, ERROR("Missing critical arguments."));
						return;
					}
					case FailureReason.MaxLessThanMin:
					{
						await MakeAndDeleteSecondaryMessage(context, ERROR("NOT USER ERROR: Max less than min."));
						return;
					}*/
				}
			}

			public static async Task<Dictionary<IUser, IMessageChannel>> GetAllBotDMs(IDiscordClient client)
			{
				var dict = new Dictionary<IUser, IMessageChannel>();
				foreach (var channel in await client.GetDMChannelsAsync())
				{
					var recep = channel.Recipient;
					if (recep != null)
					{
						dict.Add(recep, channel);
					}
				}
				return dict;
			}
			public static async Task<List<IMessage>> GetBotDMs(IDMChannel channel)
			{
				return (await GetMessages(channel, Constants.AMT_OF_DMS_TO_GATHER)).OrderBy(x => x?.CreatedAt).ToList();
			}
		}
	}
}