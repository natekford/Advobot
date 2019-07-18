using System;
using Advobot.Enums;
using Advobot.Services.GuildSettings;
using Discord.WebSocket;

namespace Advobot.Services.Logging.LoggingContexts
{
	/// <summary>
	/// Helps with logging user actions.
	/// </summary>
	public sealed class UserLoggingContext : LoggingContext
	{
		/// <inheritdoc />
		public override bool CanLog => base.CanLog && LogAction switch
		{
			//Only log if it wasn't this bot that left
			LogAction.UserJoined => User.Id != Guild.CurrentUser.Id,
			LogAction.UserLeft => User.Id != Guild.CurrentUser.Id,
			//Only log if it wasn't any bot that was updated.
			LogAction.UserUpdated => !(User.IsBot || User.IsWebhook),
			_ => throw new ArgumentException(nameof(LogAction)),
		};
		/// <summary>
		/// The user this logger is targetting.
		/// </summary>
		public SocketGuildUser User { get; }

		/// <summary>
		/// Creates an instance of <see cref="UserLoggingContext"/>.
		/// </summary>
		/// <param name="factory"></param>
		/// <param name="action"></param>
		/// <param name="user"></param>
		public UserLoggingContext(IGuildSettingsFactory factory, LogAction action, SocketGuildUser user)
			: base(factory, action, user.Guild)
		{
			User = user;
		}
	}
}
