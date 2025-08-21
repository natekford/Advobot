using Discord;
using Discord.Commands;

using System.Diagnostics;

namespace Advobot.Modules;

/// <summary>
/// A command context which contains the elapsed time.
/// </summary>
/// <param name="client"></param>
/// <param name="msg"></param>
public class GuildCommandContext(IDiscordClient client, IUserMessage msg)
	: CommandContext(client, msg), IGuildCommandContext, IElapsed
{
	private readonly Stopwatch _Stopwatch = Stopwatch.StartNew();

	/// <inheritdoc cref="ICommandContext.Channel" />
	public new ITextChannel Channel { get; } = (msg.Channel as ITextChannel)!;
	/// <summary>
	/// The time since starting the command.
	/// </summary>
	public TimeSpan Elapsed => _Stopwatch.Elapsed;
	/// <inheritdoc cref="ICommandContext.User" />
	public new IGuildUser User { get; } = (msg.Author as IGuildUser)!;
}