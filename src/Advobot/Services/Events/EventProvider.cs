using Discord;

using YACCS.Commands;

namespace Advobot.Services.Events;

/// <summary>
/// Abstraction around SocketClient and CommandService events.
/// </summary>
public abstract class EventProvider : StartableService
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
	public AsyncEvent<Func<CommandExecutedResult, Task>> CommandExecuted { get; } = new();
	public AsyncEvent<Func<CommandScore, Task>> CommandNotExecuted { get; } = new();
	public AsyncEvent<Func<IGuild, Task>> GuildAvailable { get; } = new();
	public AsyncEvent<Func<IGuild, Task>> GuildJoined { get; } = new();
	public AsyncEvent<Func<IGuild, Task>> GuildLeft { get; } = new();
	public AsyncEvent<Func<IGuildUser?, IGuildUser, Task>> GuildMemberUpdated { get; } = new();
	public AsyncEvent<Func<IGuild, Task>> GuildUnavailable { get; } = new();
	public AsyncEvent<Func<LogMessage, Task>> Log { get; } = new();
	public AsyncEvent<Func<(IMessage? Message, ulong Id), Task>> MessageDeleted { get; } = new();
	public AsyncEvent<Func<IMessage, Task>> MessageReceived { get; } = new();
	public AsyncEvent<Func<IReadOnlyCollection<(IMessage? Message, ulong Id)>, Task>> MessagesBulkDeleted { get; } = new();
	public AsyncEvent<Func<IMessage?, IMessage, IMessageChannel, Task>> MessageUpdated { get; } = new();
	public AsyncEvent<Func<IDiscordClient, Task>> Ready { get; } = new();
	public AsyncEvent<Func<IGuildUser, Task>> UserJoined { get; } = new();
	public AsyncEvent<Func<IGuild, IUser, Task>> UserLeft { get; } = new();
	public AsyncEvent<Func<IUser, IUser, Task>> UserUpdated { get; } = new();
}