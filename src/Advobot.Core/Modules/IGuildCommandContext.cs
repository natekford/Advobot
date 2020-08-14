using Discord;
using Discord.Commands;

namespace Advobot.Modules
{
	/// <summary>
	/// Represents a context of a guild command.
	/// </summary>
	public interface IGuildCommandContext : ICommandContext
	{
		/// <summary>
		/// The guild text channel.
		/// </summary>
		new ITextChannel Channel { get; }
		/// <summary>
		/// The guild user.
		/// </summary>
		new IGuildUser User { get; }
	}
}