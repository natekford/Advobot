using Advobot.Utilities;

using AdvorangesUtils;

using Discord;

using System.Collections.Concurrent;

namespace Advobot.Logging.Service;

public sealed class MessageSenderQueue
{
	// TODO: switch to batches of embeds when d.net updates to allow multiple per msg
	private readonly ConcurrentQueue<(ITextChannel, SendMessageArgs)> _Messages = new();
	private bool _IsRunning;
	public TimeSpan Delay { get; set; } = TimeSpan.FromSeconds(0.25);

	public void Enqueue((ITextChannel, SendMessageArgs) item)
		=> _Messages.Enqueue(item);

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
				while (_Messages.TryDequeue(out var item))
				{
					var (channel, args) = item;
					await channel.SendMessageAsync(args).CAF();
				}

				await Task.Delay(Delay).CAF();
			}
		});
	}

	public void Stop()
		=> _IsRunning = false;
}