using Discord.Commands;

namespace Advobot.Interfaces
{
	/// <summary>
	/// Abstraction for <see cref="Classes.AdvobotCommandContext"/>.
	/// </summary>
	public interface IAdvobotCommandContext : ICommandContext
	{
		IBotSettings BotSettings { get; }
		IGuildSettings GuildSettings { get; }
		ILogService Logging { get; }
		ITimersService Timers { get; }
	}
}
