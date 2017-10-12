using Advobot.Classes;
using Discord;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Advobot.Interfaces
{
	/// <summary>
	/// Abstraction for a log module. Handles counts of actions, and which commands have been ran. 
	/// </summary>
	public interface ILogService
	{
		List<LoggedCommand> RanCommands { get; }
		LogCounter TotalUsers { get; }
		LogCounter TotalGuilds { get; }
		LogCounter AttemptedCommands { get; }
		LogCounter SuccessfulCommands { get; }
		LogCounter FailedCommands { get; }
		LogCounter UserJoins { get; }
		LogCounter UserLeaves { get; }
		LogCounter UserChanges { get; }
		LogCounter MessageEdits { get; }
		LogCounter MessageDeletes { get; }
		LogCounter Messages { get; }
		LogCounter Images { get; }
		LogCounter Gifs { get; }
		LogCounter Files { get; }

		IBotLogger BotLogger { get; }
		IGuildLogger GuildLogger { get; }
		IUserLogger UserLogger { get; }
		IMessageLogger MessageLogger { get; }

		string FormatLoggedCommands();
		string FormatLoggedActions();
	}

	public interface IBotLogger
	{
		Task OnLogMessageSent(LogMessage message);
	}

	public interface IGuildLogger
	{
		Task OnGuildAvailable(SocketGuild guild);
		Task OnGuildUnavailable(SocketGuild guild);
		Task OnJoinedGuild(SocketGuild guild);
		Task OnLeftGuild(SocketGuild guild);
	}

	public interface IUserLogger
	{
		Task OnUserJoined(SocketGuildUser user);
		Task OnUserLeft(SocketGuildUser user);
		Task OnUserUpdated(SocketUser beforeUser, SocketUser afterUser);
	}

	public interface IMessageLogger
	{
		Task OnMessageReceived(SocketMessage message);
		Task OnMessageUpdated(Cacheable<IMessage, ulong> cached, SocketMessage message, ISocketMessageChannel channel);
		Task OnMessageDeleted(Cacheable<IMessage, ulong> cached, ISocketMessageChannel channel);
	}
}
