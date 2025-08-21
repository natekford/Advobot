using Discord;
using Discord.Commands;

namespace Advobot.Modules;

/// <summary>
/// The context of a command invoked in a guild.
/// </summary>
public interface IGuildCommandContext : ICommandContext
{
	/// <inheritdoc cref="ICommandContext.Channel" />
	new ITextChannel Channel { get; }
	/// <inheritdoc cref="ICommandContext.User" />
	new IGuildUser User { get; }
}