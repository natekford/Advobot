using Discord.Commands;
using Discord.WebSocket;

using System.Diagnostics;

namespace Advobot.Modules;

/// <summary>
/// A <see cref="ShardedCommandContext"/> which contains settings and the service provider.
/// </summary>
public class AdvobotCommandContext : ShardedCommandContext
{
	private readonly Stopwatch _Stopwatch = new();

	/// <summary>
	/// The channel this command is executing from.
	/// </summary>
	public new SocketTextChannel Channel { get; }
	/// <summary>
	/// The time since starting the command.
	/// </summary>
	public TimeSpan Elapsed => _Stopwatch.Elapsed;
	/// <summary>
	/// The user this command is executing from.
	/// </summary>
	public new SocketGuildUser User { get; }

	/// <summary>
	/// Creates an instance of <see cref="AdvobotCommandContext"/>.
	/// </summary>
	/// <param name="client"></param>
	/// <param name="msg"></param>
	public AdvobotCommandContext(DiscordShardedClient client, SocketUserMessage msg)
		: base(client, msg)
	{
		_Stopwatch.Start();
		User = (SocketGuildUser)msg.Author;
		Channel = (SocketTextChannel)msg.Channel;
	}
}