using Advobot.Services.GuildSettings;
using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace Advobot.Modules
{
	public interface IAdvobotCommandContext : ICommandContext
	{
		new IGuildUser User { get; }
		new ITextChannel Channel { get; }
		IGuildSettings Settings { get; }
		long ElapsedMilliseconds { get; }
	}
}
