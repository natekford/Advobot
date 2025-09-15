using Discord;

using YACCS.Commands;

namespace Advobot.Modules;

/// <summary>
/// The context of a command invoked in a guild.
/// </summary>
public interface IGuildContext : IContext
{
	/// <inheritdoc cref="Discord.Commands.ICommandContext.Channel" />
	ITextChannel Channel { get; }
	/// <inheritdoc cref="Discord.Commands.ICommandContext.Client" />
	IDiscordClient Client { get; }
	/// <inheritdoc cref="Discord.Commands.ICommandContext.Guild" />
	IGuild Guild { get; }
	/// <inheritdoc cref="Discord.Commands.ICommandContext.Message" />
	IUserMessage Message { get; }
	/// <inheritdoc cref="Discord.Commands.ICommandContext.User" />
	IGuildUser User { get; }
}