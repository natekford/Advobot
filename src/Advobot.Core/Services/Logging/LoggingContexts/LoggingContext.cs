using System;
using Advobot.Enums;
using Advobot.Interfaces;
using Discord.WebSocket;

namespace Advobot.Services.Logging.LoggingContexts
{
	/// <summary>
	/// Helps with logging.
	/// </summary>
	public abstract class LoggingContext
	{
		/// <summary>
		/// The guild this logger is targetting.
		/// </summary>
		public SocketGuild Guild { get; }
		/// <summary>
		/// The action this logger uses.
		/// </summary>
		public LogAction LogAction { get; }
		/// <summary>
		/// The settings this logger is targetting.
		/// </summary>
		public IGuildSettings Settings { get; }
		/// <summary>
		/// Where message/user actions get logged.
		/// </summary>
		public SocketTextChannel? ServerLog { get; }
		/// <summary>
		/// Where images get logged.
		/// </summary>
		public SocketTextChannel? ImageLog { get; }
		/// <summary>
		/// Whether the current context can be logged.
		/// </summary>
		public virtual bool CanLog { get; }

		/// <summary>
		/// Creates an instance of <see cref="LoggingContext"/>.
		/// </summary>
		/// <param name="factory"></param>
		/// <param name="action"></param>
		/// <param name="guild"></param>
		public LoggingContext(IGuildSettingsFactory factory, LogAction action, SocketGuild guild)
		{
			Guild = guild;
			LogAction = action;

			if (!factory.TryGet(guild.Id, out var settings))
			{
				throw new InvalidOperationException($"Unable to get settings for {guild.Id}");
			}

			Settings = settings;
			ServerLog = guild.GetTextChannel(settings.ServerLogId);
			ImageLog = guild.GetTextChannel(settings.ImageLogId);
			CanLog = Settings.LogActions.Contains(LogAction);
		}
	}
}
