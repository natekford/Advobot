using Discord.Commands;

namespace Advobot.Interfaces
{
	/// <summary>
	/// Abstraction for <see cref="Classes.MyCommandContext"/>.
	/// </summary>
	public interface IMyCommandContext : ICommandContext
	{
		IBotSettings BotSettings { get; }
		IGuildSettings GuildSettings { get; }
		ILogModule Logging { get; }
		ITimersModule Timers { get; }
	}
}
