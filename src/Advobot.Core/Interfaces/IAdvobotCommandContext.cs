using Discord.Commands;

namespace Advobot.Core.Interfaces
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
