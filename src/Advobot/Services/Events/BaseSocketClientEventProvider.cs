using Discord;
using Discord.WebSocket;

namespace Advobot.Services.Events;

/// <summary>
/// Handles events for a socket client.
/// </summary>
/// <param name="client"></param>
public class BaseSocketClientEventProvider(BaseSocketClient client)
	: EventProvider
{
	/// <inheritdoc />
	protected override Task StartAsyncImpl()
	{
		client.Log += Log.InvokeAsync;

		client.GuildAvailable += GuildAvailable.InvokeAsync;
		client.GuildUnavailable += GuildUnavailable.InvokeAsync;
		client.JoinedGuild += GuildJoined.InvokeAsync;
		client.LeftGuild += GuildLeft.InvokeAsync;

		client.MessageReceived += MessageReceived.InvokeAsync;
		client.MessageUpdated += OnMessageUpdated;
		client.MessageDeleted += OnMessageDeleted;
		client.MessagesBulkDeleted += OnMessagesBulkDeleted;

		client.UserJoined += UserJoined.InvokeAsync;
		client.UserLeft += UserLeft.InvokeAsync;
		client.UserUpdated += UserUpdated.InvokeAsync;

		client.GuildMemberUpdated += OnGuildMemberUpdated;

		return Task.CompletedTask;
	}

	/// <inheritdoc />
	protected override Task StopAsyncImpl()
	{
		client.Log -= Log.InvokeAsync;

		client.GuildAvailable -= GuildAvailable.InvokeAsync;
		client.GuildUnavailable -= GuildUnavailable.InvokeAsync;
		client.JoinedGuild -= GuildJoined.InvokeAsync;
		client.LeftGuild -= GuildLeft.InvokeAsync;

		client.MessageReceived -= MessageReceived.InvokeAsync;
		client.MessageUpdated -= OnMessageUpdated;
		client.MessageDeleted -= OnMessageDeleted;
		client.MessagesBulkDeleted -= OnMessagesBulkDeleted;

		client.UserJoined -= UserJoined.InvokeAsync;
		client.UserLeft -= UserLeft.InvokeAsync;
		client.UserUpdated -= UserUpdated.InvokeAsync;

		client.GuildMemberUpdated -= OnGuildMemberUpdated;

		return base.StopAsyncImpl();
	}

	private Task OnGuildMemberUpdated(
		Cacheable<SocketGuildUser, ulong> before,
		SocketGuildUser after
	) => GuildMemberUpdated.InvokeAsync(before.Value, after);

	private Task OnMessageDeleted(
		Cacheable<IMessage, ulong> message,
		Cacheable<IMessageChannel, ulong> _
	) => MessageDeleted.InvokeAsync((message.Value, message.Id));

	private Task OnMessagesBulkDeleted(
		IReadOnlyCollection<Cacheable<IMessage, ulong>> messages,
		Cacheable<IMessageChannel, ulong> _
	) => MessagesBulkDeleted.InvokeAsync(messages.Select(x => (x.Value, x.Id)).ToArray()!);

	private Task OnMessageUpdated(
		Cacheable<IMessage, ulong> before,
		SocketMessage after,
		ISocketMessageChannel channel
	) => MessageUpdated.InvokeAsync(before.Value, after, channel);
}