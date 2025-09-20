using Advobot.Utilities;

using Discord;

using System.Diagnostics;

namespace Advobot.Modules;

/// <summary>
/// A command context which contains the elapsed time.
/// </summary>
/// <param name="services"></param>
/// <param name="client"></param>
/// <param name="msg"></param>
[DebuggerDisplay(Constants.DEBUGGER_DISPLAY)]
public class GuildContext(
	IServiceProvider services,
	IDiscordClient client,
	IUserMessage msg
) : IGuildContext
{
	private readonly Stopwatch _Stopwatch = Stopwatch.StartNew();

	/// <inheritdoc />
	public ITextChannel Channel { get; } = (msg.Channel as ITextChannel)!;
	/// <inheritdoc />
	public IDiscordClient Client { get; } = client;
	/// <summary>
	/// The time since starting the command.
	/// </summary>
	public TimeSpan Elapsed => _Stopwatch.Elapsed;
	/// <inheritdoc />
	public IGuild Guild { get; } = (msg.Channel as IGuildChannel)?.Guild!;
	/// <inheritdoc />
	public Guid Id { get; } = Guid.NewGuid();
	/// <inheritdoc />
	public IUserMessage Message { get; } = msg;
	/// <inheritdoc />
	public IServiceProvider Services { get; } = services;
	/// <inheritdoc />
	public object Source => Message;
	/// <inheritdoc />
	public IGuildUser User { get; } = (msg.Author as IGuildUser)!;

	private string DebuggerDisplay => $"Guild = {Guild.Format()}, User = {User.Format()}";
}