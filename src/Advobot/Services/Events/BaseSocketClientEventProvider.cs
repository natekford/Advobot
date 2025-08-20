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
		client.MessageUpdated += MessageUpdated.InvokeAsync;
		client.MessageDeleted += MessageDeleted.InvokeAsync;
		client.MessagesBulkDeleted += MessagesBulkDeleted.InvokeAsync;

		client.UserJoined += UserJoined.InvokeAsync;
		client.UserLeft += UserLeft.InvokeAsync;
		client.UserUpdated += UserUpdated.InvokeAsync;

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
		client.MessageUpdated -= MessageUpdated.InvokeAsync;
		client.MessageDeleted -= MessageDeleted.InvokeAsync;
		client.MessagesBulkDeleted -= MessagesBulkDeleted.InvokeAsync;

		client.UserJoined -= UserJoined.InvokeAsync;
		client.UserLeft -= UserLeft.InvokeAsync;
		client.UserUpdated -= UserUpdated.InvokeAsync;

		return base.StopAsyncImpl();
	}
}