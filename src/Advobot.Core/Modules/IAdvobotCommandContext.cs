﻿using Advobot.Services.GuildSettings;

using Discord.Commands;

namespace Advobot.Modules
{
	/// <summary>
	/// Holds guild specific user/channel and guild settings.
	/// </summary>
	public interface IAdvobotCommandContext : IGuildCommandContext, IElapsed
	{
		/// <summary>
		/// The settings used by the guild.
		/// </summary>
		IGuildSettings Settings { get; }
	}
}