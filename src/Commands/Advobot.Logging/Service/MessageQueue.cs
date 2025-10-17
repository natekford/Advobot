using Advobot.Embeds;
using Advobot.Logging.Utilities;
using Advobot.Services;
using Advobot.Utilities;

using Discord;

using Microsoft.Extensions.Logging;

using System.Collections.Concurrent;
using System.Text;
using System.Threading.Channels;

using YACCS.Preconditions.Locked;

using static Advobot.Resources.Responses;

namespace Advobot.Logging.Service;

public sealed class MessageQueue(
	ILogger<LoggingService> logger,
	IDiscordClient client
) : StartableService, IConfigurableService
{
	private readonly Channel<(IMessageChannel, SendMessageArgs)> _ToSend
		= Channel.CreateUnbounded<(IMessageChannel, SendMessageArgs)>();
	// This isn't a channel so the messages can get grouped together
	private ConcurrentDictionary<ulong, ConcurrentBag<IMessage>> _ToLog = new();

	public void EnqueueDeleted(IMessageChannel channel, IMessage message)
		=> _ToLog.GetOrAdd(channel.Id, _ => []).Add(message);

	public void EnqueueSend(IMessageChannel channel, SendMessageArgs message)
		=> _ToSend.Writer.TryWrite((channel, message));

	protected override Task StartAsyncImpl()
	{
		RepeatInBackground(async () =>
		{
			var (channel, args) = await _ToSend.Reader.ReadAsync(CancellationToken).ConfigureAwait(false);
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
		});
		RepeatInBackground(async () =>
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

			await Task.Delay(TimeSpan.FromSeconds(5), CancellationToken).ConfigureAwait(false);
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

		var sb = new StringBuilder();
		foreach (var message in ordered)
		{
			sb.AppendLine(message.Format().Sanitize(keepMarkdown: true));
		}

		var embed = new EmbedWrapper
		{
			Color = EmbedWrapper.MessageDelete,
			Title = TitleDeletedMessages,
			Footer = new()
			{
				Text = TitleDeletedMessages,
			},
		};
		if (!embed.TrySetDescription(sb.ToString(), out _))
		{
			embed.TrySetDescription(VariableSeeAttachedFile, out _);
		}

		EnqueueSend(channel, embed.ToMessageArgs());
	}
}