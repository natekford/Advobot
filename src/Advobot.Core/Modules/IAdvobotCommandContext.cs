using Advobot.Services.GuildSettings;

using Discord;
using Discord.Commands;

namespace Advobot.Modules
{
	/// <summary>
	/// Holds guild specific user/channel and guild settings.
	/// </summary>
	public interface IAdvobotCommandContext : ICommandContext, IElapsed
	{
		/// <summary>
		/// The guild text channel.
		/// </summary>
		new ITextChannel Channel { get; }

		/// <summary>
		/// The settings used by the guild.
		/// </summary>
		IGuildSettings Settings { get; }

		/// <summary>
		/// The guild user.
		/// </summary>
		new IGuildUser User { get; }
	}
}