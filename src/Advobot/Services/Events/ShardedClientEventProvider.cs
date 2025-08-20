using Discord.WebSocket;

namespace Advobot.Services.Events;

/// <summary>
/// Handles events specific to sharded clients.
/// </summary>
/// <param name="client"></param>
public sealed class ShardedClientEventProvider(DiscordShardedClient client)
	: BaseSocketClientEventProvider(client)
{
	private int _ShardsReady;

	/// <inheritdoc />
	protected override Task StartAsyncImpl()
	{
		client.ShardReady += OnShardReady;

		return base.StartAsyncImpl();
	}

	/// <inheritdoc />
	protected override Task StopAsyncImpl()
	{
		client.ShardReady -= OnShardReady;

		return base.StopAsyncImpl();
	}

	private Task OnShardReady(DiscordSocketClient _)
	{
		if (++_ShardsReady < client.Shards.Count)
		{
			return Task.CompletedTask;
		}
		client.ShardReady -= OnShardReady;

		return Ready.InvokeAsync(client);
	}
}