using System;
using Advobot.Enums;
using Advobot.Interfaces;
using Discord.WebSocket;

namespace Advobot.Services.Logging.LoggingContexts
{
	/// <summary>
	/// Helps with logging user actions.
	/// </summary>
	public sealed class UserLoggingContext : LoggingContext
	{
		/// <inheritdoc />
		public override bool CanLog
		{
			get
			{
				if (!base.CanLog)
				{
					return false;
				}

				switch (LogAction)
				{
					//Only log if it wasn't this bot that left
					case LogAction.UserJoined:
					case LogAction.UserLeft:
						return User.Id != Guild.CurrentUser.Id;
					//Only log if it wasn't any bot that was updated.
					case LogAction.UserUpdated:
						return !(User.IsBot || User.IsWebhook);
					default:
						throw new InvalidOperationException($"Invalid log action supplied: {LogAction}.");
				}
			}
		}
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
