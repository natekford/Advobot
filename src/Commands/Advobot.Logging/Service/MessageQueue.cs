using Advobot.Embeds;
using Advobot.Logging.Utilities;
using Advobot.Services;
using Advobot.Utilities;

using Discord;

using Microsoft.Extensions.Logging;

using System.Collections.Concurrent;
using System.Text;

namespace Advobot.Logging.Service;

public sealed class MessageQueue(
	ILogger<LoggingService> logger,
	IDiscordClient client
) : StartableService, IConfigurableService
{
	private readonly ConcurrentQueue<(IMessageChannel, SendMessageArgs)> _ToSend = new();

	private ConcurrentDictionary<ulong, ConcurrentBag<IMessage>> _ToLog = new();

	public void EnqueueDeleted(IMessageChannel channel, IMessage message)
		=> _ToLog.GetOrAdd(channel.Id, _ => []).Add(message);

	public void EnqueueSend(IMessageChannel channel, SendMessageArgs message)
		=> _ToSend.Enqueue((channel, message));

	protected override Task StartAsyncImpl()
	{
		_ = Task.Run(async () =>
		{
			while (IsRunning)
			{
				while (_ToSend.TryDequeue(out var item))
				{
					var (channel, args) = item;
					try
					{
						await channel.SendMessageAsync(args).ConfigureAwait(false);
					}
					catch (Exception e)
					{
						logger.LogWarning(
							exception: e,
							message: "Exception occurred while sending message. {@Info}",
							args: new
							{
								Channel = channel.Id
							}
						);
					}
				}

				await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);
			}
		});
		_ = Task.Run(async () =>
		{
			while (IsRunning)
			{
				foreach (var (channelId, messages) in Interlocked.Exchange(ref _ToLog, []))
				{
					try
					{
						var channel = await client.GetChannelAsync(channelId).ConfigureAwait(false);
						if (channel is not IMessageChannel messageChannel)
						{
							logger.LogWarning(
								message: "Channel was null while queuing deleted message. {@Info}",
								args: new
								{
									Channel = channelId
								}
							);
							continue;
						}

						QueueDeletedMessages(messageChannel, messages);
					}
					catch (Exception e)
					{
						logger.LogWarning(
							exception: e,
							message: "Exception occurred while queuing deleted messages. {@Info}",
							args: new
							{
								Channel = channelId,
								Messages = messages.Select(x => x.Id).ToArray(),
							}
						);
					}
				}

				await Task.Delay(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
			}
		});
		return Task.CompletedTask;
	}

	private void QueueDeletedMessages(IMessageChannel channel, IEnumerable<IMessage> messages)
	{
		var ordered = messages.OrderBy(x => x.Id).ToArray();

		logger.LogInformation(
			message: "Printing {Count} deleted messages {@Info}",
			args: [ordered.Length, new
			{
				Channel = channel.Id,
			}]
		);

		// Needs to not be a lot of messages to fit in an embed
		var inEmbed = ordered.Length < 10;
		var sb = new StringBuilder();

		foreach (var message in ordered)
		{
			var text = message.Format(withMentions: true).Sanitize(keepMarkdown: true);
			sb.AppendLine(text);

			// Can only stay in an embed if the description is less than 2048
			// and if the line numbers are less than 20
			// Subtract 100 just to give some leeway
			if (sb.Length > (EmbedBuilder.MaxDescriptionLength - 100))
			{
				inEmbed = false;
				break;
			}
		}

		if (inEmbed)
		{
			EnqueueSend(channel, new EmbedWrapper
			{
				Title = "Deleted Messages",
				Description = sb.ToString(),
				Color = EmbedWrapper.MessageDelete,
				Footer = new() { Text = "Deleted Messages", },
			}.ToMessageArgs());
		}
		else
		{
			sb.Clear();
			foreach (var message in ordered)
			{
				var text = message.Format(withMentions: false).Sanitize(keepMarkdown: false);
				sb.AppendLine(text);
			}

			EnqueueSend(channel, new()
			{
				Files =
				[
					MessageUtils.CreateTextFile(
						fileName: $"{ordered.Length}_Deleted_Messages",
						content: sb.ToString()
					),
				],
			});
		}
	}
}