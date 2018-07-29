﻿using System;
using System.Diagnostics;
using Advobot.Interfaces;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Classes
{
	/// <summary>
	/// A <see cref="SocketCommandContext"/> which contains <see cref="IBotSettings"/>, <see cref="IGuildSettings"/>, <see cref="ILogService"/>, and <see cref="ITimersService"/>.
	/// </summary>
	public class AdvobotSocketCommandContext : SocketCommandContext
	{
		private static readonly string _Joiner = "\n" + new string(' ', 28);

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
			return BotSettings.InternalGetPrefix(GuildSettings);
		}
		/// <summary>
		/// Returns information about the context and how long it's taken to execute.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return ToString(null);
		}
		/// <summary>
		/// Returns information about the context and how long it's taken to execute, but also includes any errors.
		/// </summary>
		/// <param name="result"></param>
		/// <returns></returns>
		public string ToString(IResult result)
		{
			var resp = $"Guild: {Guild.Format()}" +
				$"{_Joiner}Channel: {Channel.Format()}" +
				$"{_Joiner}User: {User.Format()}" +
				$"{_Joiner}Time: {Message.CreatedAt.UtcDateTime.ToReadable()}" +
				$"{_Joiner}Text: {Message.Content}" +
				$"{_Joiner}Time taken: {ElapsedMilliseconds}ms";
			resp += result.ErrorReason == null ? "" : $"{_Joiner}Error: {result.ErrorReason}";
			return resp;
		}
	}
}