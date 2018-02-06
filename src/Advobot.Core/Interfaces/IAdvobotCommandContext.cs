using Discord.Commands;

namespace Advobot.Core.Interfaces
{
	/// <summary>
	/// Abstraction for <see cref="Classes.AdvobotSocketCommandContext"/>.
	/// </summary>
	public interface IAdvobotCommandContext : ICommandContext
	{
		IBotSettings BotSettings { get; }
		IGuildSettings GuildSettings { get; }
		ILogService Logging { get; }
		IInviteListService InviteList { get; }
		ITimersService Timers { get; }
		long ElapsedMilliseconds { get; }

		string GetPrefix();
	}
}
