using Discord.Commands;
using System;

namespace Advobot.Interfaces
{
	/// <summary>
	/// Signifies the object can return a <see cref="DateTime"/>.
	/// </summary>
	public interface ITimeInterface
	{
		DateTime GetTime();
	}

	/// <summary>
	/// Signifies the object has a name and <see cref="ulong"/> value.
	/// </summary>
	public interface IPermission
	{
		string Name { get; }
		ulong Value { get; }
	}

	/// <summary>
	/// Signifies the object has a name and description.
	/// </summary>
	public interface IDescription
	{
		string Name { get; }
		string Description { get; }
	}

	/// <summary>
	/// Abstraction for <see cref="Advobot.Classes.MyCommandContext"/>.
	/// </summary>
	public interface IMyCommandContext : ICommandContext
	{
		IBotSettings BotSettings { get; }
		IGuildSettings GuildSettings { get; }
		ILogModule Logging { get; }
		ITimersModule Timers { get; }
	}
}
