using Advobot.Core.Classes;
using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace Advobot.Core.Interfaces
{
	/// <summary>
	/// Abstraction for a log module. Handles counts of actions, and which commands have been ran. 
	/// </summary>
	public interface ILogService
	{
		/// <summary>
		/// How many users does the bot encompass.
		/// </summary>
		LogCounter TotalUsers { get; }
		/// <summary>
		/// How many guilds does the bot encompass.
		/// </summary>
		LogCounter TotalGuilds { get; }
		/// <summary>
		/// How many commands have been used in total.
		/// </summary>
		LogCounter AttemptedCommands { get; }
		/// <summary>
		/// How many commands were successful.
		/// </summary>
		LogCounter SuccessfulCommands { get; }
		/// <summary>
		/// How many commands failed.
		/// </summary>
		LogCounter FailedCommands { get; }
		/// <summary>
		/// How many users have joined.
		/// </summary>
		LogCounter UserJoins { get; }
		/// <summary>
		/// How many users have left.
		/// </summary>
		LogCounter UserLeaves { get; }
		/// <summary>
		/// How many users have modified themselves.
		/// </summary>
		LogCounter UserChanges { get; }
		/// <summary>
		/// How many messages have been edited.
		/// </summary>
		LogCounter MessageEdits { get; }
		/// <summary>
		/// How many messages have been deleted.
		/// </summary>
		LogCounter MessageDeletes { get; }
		/// <summary>
		/// How many messages have been sent.
		/// </summary>
		LogCounter Messages { get; }
		/// <summary>
		/// How many images have been sent.
		/// </summary>
		LogCounter Images { get; }
		/// <summary>
		/// How many videos/gifs have been sent.
		/// </summary>
		LogCounter Animated { get; }
		/// <summary>
		/// How many files have been sent.
		/// </summary>
		LogCounter Files { get; }

		/// <summary>
		/// Logs things related to the bot.
		/// </summary>
		IBotLogger BotLogger { get; }
		/// <summary>
		/// Logs things related to guilds.
		/// </summary>
		IGuildLogger GuildLogger { get; }
		/// <summary>
		/// Logs things related to users.
		/// </summary>
		IUserLogger UserLogger { get; }
		/// <summary>
		/// Logs things related to messages.
		/// </summary>
		IMessageLogger MessageLogger { get; }

		/// <summary>
		/// Returns a string saying how many commands, successes, and failures.
		/// </summary>
		/// <param name="withMarkDown"></param>
		/// <param name="equalSpacing"></param>
		/// <returns></returns>
		string FormatLoggedCommands(bool withMarkDown, bool equalSpacing);
		/// <summary>
		/// Returns a string saying how many users actions have happened.
		/// </summary>
		/// <param name="withMarkDown"></param>
		/// <param name="equalSpacing"></param>
		/// <returns></returns>
		string FormatLoggedUserActions(bool withMarkDown, bool equalSpacing);
		/// <summary>
		/// Returns a string saying how many message actions have happened.
		/// </summary>
		/// <param name="withMarkDown"></param>
		/// <param name="equalSpacing"></param>
		/// <returns></returns>
		string FormatLoggedMessageActions(bool withMarkDown, bool equalSpacing);
	}

	/// <summary>
	/// Logs actions related to the bot.
	/// </summary>
	public interface IBotLogger
	{
		/// <summary>
		/// When the api wrapper sends a log message.
		/// </summary>
		/// <param name="message"></param>
		/// <returns></returns>
		Task OnLogMessageSent(LogMessage message);
	}

	/// <summary>
	/// Logs actions related to guilds.
	/// </summary>
	public interface IGuildLogger
	{
		/// <summary>
		/// When a guild shows up for the bot.
		/// </summary>
		/// <param name="guild"></param>
		/// <returns></returns>
		Task OnGuildAvailable(SocketGuild guild);
		/// <summary>
		/// When a guild disappears for the bot.
		/// </summary>
		/// <param name="guild"></param>
		/// <returns></returns>
		Task OnGuildUnavailable(SocketGuild guild);
		/// <summary>
		/// When the bot joins a guild.
		/// </summary>
		/// <param name="guild"></param>
		/// <returns></returns>
		Task OnJoinedGuild(SocketGuild guild);
		/// <summary>
		/// When the bot leaves a guild.
		/// </summary>
		/// <param name="guild"></param>
		/// <returns></returns>
		Task OnLeftGuild(SocketGuild guild);
	}

	/// <summary>
	/// Logs actions related to users.
	/// </summary>
	public interface IUserLogger
	{
		/// <summary>
		/// When a user joins a guild.
		/// </summary>
		/// <param name="user"></param>
		/// <returns></returns>
		Task OnUserJoined(SocketGuildUser user);
		/// <summary>
		/// When a user leaves a guild.
		/// </summary>
		/// <param name="user"></param>
		/// <returns></returns>
		Task OnUserLeft(SocketGuildUser user);
		/// <summary>
		/// When a user updates themself. (name, picture, etc)
		/// </summary>
		/// <param name="beforeUser"></param>
		/// <param name="afterUser"></param>
		/// <returns></returns>
		Task OnUserUpdated(SocketUser beforeUser, SocketUser afterUser);
	}

	/// <summary>
	/// Logs actions related to messages.
	/// </summary>
	public interface IMessageLogger
	{
		/// <summary>
		/// When a message is received.
		/// </summary>
		/// <param name="message"></param>
		/// <returns></returns>
		Task OnMessageReceived(SocketMessage message);
		/// <summary>
		/// When a message is edited.
		/// </summary>
		/// <param name="cached"></param>
		/// <param name="message"></param>
		/// <param name="channel"></param>
		/// <returns></returns>
		Task OnMessageUpdated(Cacheable<IMessage, ulong> cached, SocketMessage message, ISocketMessageChannel channel);
		/// <summary>
		/// When a message is deleted.
		/// </summary>
		/// <param name="cached"></param>
		/// <param name="channel"></param>
		/// <returns></returns>
		Task OnMessageDeleted(Cacheable<IMessage, ulong> cached, ISocketMessageChannel channel);
	}
}
