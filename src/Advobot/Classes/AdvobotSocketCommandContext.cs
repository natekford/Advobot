using Advobot.Interfaces;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;

namespace Advobot.Classes
{
	/// <summary>
	/// A <see cref="SocketCommandContext"/> which contains <see cref="IBotSettings"/>, <see cref="IGuildSettings"/>, <see cref="ILogService"/>, and <see cref="ITimersService"/>.
	/// </summary>
	public class AdvobotSocketCommandContext : SocketCommandContext
	{
		/// <summary>
		/// The settings for the bot.
		/// </summary>
		public IBotSettings BotSettings { get; }
		/// <summary>
		/// The logging for the bot.
		/// </summary>
		public ILogService Logging { get; }
		/// <summary>
		/// Holds timed objects, like removable messages.
		/// </summary>
		public ITimersService Timers { get; }
		/// <summary>
		/// The invite list for the bot.
		/// </summary>
		public IInviteListService InviteList { get; }
		/// <summary>
		/// The settings for the guild.
		/// </summary>
		public IGuildSettings GuildSettings { get; }
		/// <summary>
		/// The help entries for all the commands.
		/// </summary>
		public HelpEntryHolder HelpEntries { get; }
		/// <summary>
		/// The time since starting the command.
		/// </summary>
		public long ElapsedMilliseconds => _Stopwatch.ElapsedMilliseconds;

		private Stopwatch _Stopwatch = new Stopwatch();

		/// <summary>
		/// Creates an instance of <see cref="AdvobotSocketCommandContext"/>.
		/// </summary>
		/// <param name="provider"></param>
		/// <param name="settings"></param>
		/// <param name="client"></param>
		/// <param name="msg"></param>
		public AdvobotSocketCommandContext(IServiceProvider provider, IGuildSettings settings, DiscordSocketClient client, SocketUserMessage msg) : base(client, msg)
		{
			_Stopwatch.Start();
			BotSettings = provider.GetRequiredService<IBotSettings>();
			Logging = provider.GetRequiredService<ILogService>();
			Timers = provider.GetRequiredService<ITimersService>();
			InviteList = provider.GetRequiredService<IInviteListService>();
			HelpEntries = provider.GetRequiredService<HelpEntryHolder>();
			GuildSettings = settings;
		}

		/// <summary>
		/// Gets the prefix to use.
		/// </summary>
		/// <returns>Returns the guild prefix if not null, otherwise returns the bot prefix.</returns>
		public string GetPrefix()
		{
			return String.IsNullOrWhiteSpace(GuildSettings.Prefix) ? BotSettings.Prefix : GuildSettings.Prefix;
		}
	}
}