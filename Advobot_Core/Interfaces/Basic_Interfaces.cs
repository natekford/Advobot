using Discord.Commands;
using System;

namespace Advobot.Interfaces
{
	public interface ITimeInterface
	{
		DateTime GetTime();
	}

	public interface IPermission
	{
		string Name { get; }
		ulong Value { get; }
	}

	public interface INameAndText
	{
		string Name { get; }
		string Text { get; }
	}

	public interface IMyCommandContext : ICommandContext
	{
		IBotSettings BotSettings { get; }
		IGuildSettings GuildSettings { get; }
		ILogModule Logging { get; }
		ITimersModule Timers { get; }
	}
}
