using Advobot.Embeds;
using Advobot.Logging.Utilities;
using Advobot.Services;
using Advobot.Utilities;

using Discord;
using Discord.WebSocket;

using Microsoft.Extensions.Logging;

using System.Collections.Concurrent;
using System.Text;

namespace Advobot.Logging.Service;

public sealed class MessageQueue(
	ILogger<LoggingService> logger,
	BaseSocketClient client
) : IStartableService, IConfigurableService
{
	private readonly ConcurrentQueue<(IMessageChannel, SendMessageArgs)> _Send = new();

	private ConcurrentDictionary<ulong, ConcurrentBag<IMessage>> _Deleted = new();
	private bool _IsRunning;

	public void EnqueueDeleted(IMessageChannel channel, IMessage message)
		=> _Deleted.GetOrAdd(channel.Id, _ => []).Add(message);

	public void EnqueueSend(IMessageChannel channel, SendMessageArgs message)
		=> _Send.Enqueue((channel, message));

	public void Start()
	{
		if (_IsRunning)
		{
			return;
		}
		_IsRunning = true;

		_ = Task.Run(async () =>
		{
			while (_IsRunning)
			{
				while (_Send.TryDequeue(out var item))
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
			while (_IsRunning)
			{
				foreach (var (channelId, messages) in Interlocked.Exchange(ref _Deleted, []))
				{
					try
					{
						if (client.GetChannel(channelId) is not IMessageChannel channel)
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

						QueueDeletedMessages(channel, messages);
					}
					catch (Exception e)
					{
						logger.LogWarning(
							exception: e,
							message: "Exception occurred while queuing deleted messages. {@Info}",
							args: new
							{
								Channel = channelId,
								Messages = messages.Select(x => x.Id).ToList(),
							}
						);
					}
				}

				await Task.Delay(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
			}
		});
	}

	public void Stop()
		=> _IsRunning = false;

	Task IConfigurableService.ConfigureAsync()
	{
		Start();
		return Task.CompletedTask;
	}

	private void QueueDeletedMessages(IMessageChannel channel, IEnumerable<IMessage> messages)
	{
		var ordered = messages.OrderBy(x => x.Id).ToList();

		logger.LogInformation(
			message: "Printing {Count} deleted messages {@Info}",
			args: [ordered.Count, new
			{
				Channel = channel.Id,
			}]
		);

		// Needs to not be a lot of messages to fit in an embed
		var inEmbed = ordered.Count < 10;
		var sb = new StringBuilder();

		var lineCount = 0;
		foreach (var message in ordered)
		{
			var text = message.Format(withMentions: true).Sanitize(keepMarkdown: true);
			lineCount += text.Count(c => c is '\n');
			sb.AppendLine(text);

			// Can only stay in an embed if the description is less than 2048
			// and if the line numbers are less than 20
			// Subtract 100 just to give some leeway
			if (sb.Length > (EmbedBuilder.MaxDescriptionLength - 100)
				|| lineCount > EmbedWrapper.MAX_DESCRIPTION_LINES)
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
						fileName: $"{ordered.Count}_Deleted_Messages",
						content: sb.ToString()
					),
				],
			});
		}
	}
}