using Advobot.Classes;
using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace Advobot.Interfaces
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
		/// <param name="markdown"></param>
		/// <param name="equalSpacing"></param>
		/// <returns></returns>
		string FormatLoggedCommands(bool markdown, bool equalSpacing);
		/// <summary>
		/// Returns a string saying how many users actions have happened.
		/// </summary>
		/// <param name="markdown"></param>
		/// <param name="equalSpacing"></param>
		/// <returns></returns>
		string FormatLoggedUserActions(bool markdown, bool equalSpacing);
		/// <summary>
		/// Returns a string saying how many message actions have happened.
		/// </summary>
		/// <param name="markdown"></param>
		/// <param name="equalSpacing"></param>
		/// <returns></returns>
		string FormatLoggedMessageActions(bool markdown, bool equalSpacing);
	}
}