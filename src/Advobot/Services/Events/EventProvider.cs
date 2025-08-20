using Discord;
using Discord.Commands;

namespace Advobot.Services.Events;

/// <summary>
/// Abstraction around SocketClient and CommandService events.
/// </summary>
public abstract class EventProvider : StartableService
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
	public AsyncEvent<Func<Optional<CommandInfo>, ICommandContext, IResult, Task>> CommandExecuted { get; } = new();
	public AsyncEvent<Func<IGuild, Task>> GuildAvailable { get; } = new();
	public AsyncEvent<Func<IGuild, Task>> GuildJoined { get; } = new();
	public AsyncEvent<Func<IGuild, Task>> GuildLeft { get; } = new();
	public AsyncEvent<Func<IGuild, Task>> GuildUnavailable { get; } = new();
	public AsyncEvent<Func<LogMessage, Task>> Log { get; } = new();
	public AsyncEvent<Func<Cacheable<IMessage, ulong>, Cacheable<IMessageChannel, ulong>, Task>> MessageDeleted { get; } = new();
	public AsyncEvent<Func<IMessage, Task>> MessageReceived { get; } = new();
	public AsyncEvent<Func<IReadOnlyCollection<Cacheable<IMessage, ulong>>, Cacheable<IMessageChannel, ulong>, Task>> MessagesBulkDeleted { get; } = new();
	public AsyncEvent<Func<Cacheable<IMessage, ulong>, IMessage, IMessageChannel, Task>> MessageUpdated { get; } = new();
	public AsyncEvent<Func<IDiscordClient, Task>> Ready { get; } = new();
	public AsyncEvent<Func<IGuildUser, Task>> UserJoined { get; } = new();
	public AsyncEvent<Func<IGuild, IUser, Task>> UserLeft { get; } = new();
	public AsyncEvent<Func<IUser, IUser, Task>> UserUpdated { get; } = new();
}