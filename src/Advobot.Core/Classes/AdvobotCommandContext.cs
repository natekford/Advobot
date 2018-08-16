using System;
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
	/// A <see cref="ShardedCommandContext"/> which contains settings and the service provider.
	/// </summary>
	public class AdvobotCommandContext : ShardedCommandContext
	{
		private static readonly string _Joiner = "\n" + new string(' ', 28);

		/// <summary>
		/// The settings for the guild.
		/// </summary>
		public IGuildSettings GuildSettings { get; }
		/// <summary>
		/// The settings for the bot.
		/// </summary>
		public IBotSettings BotSettings { get; }
		/// <summary>
		/// The services that are available.
		/// </summary>
		public IServiceProvider Provider { get; }
		/// <summary>
		/// The time since starting the command.
		/// </summary>
		public long ElapsedMilliseconds => _Stopwatch.ElapsedMilliseconds;

		private Stopwatch _Stopwatch = new Stopwatch();

		/// <summary>
		/// Creates an instance of <see cref="AdvobotCommandContext"/>.
		/// </summary>
		/// <param name="provider"></param>
		/// <param name="settings"></param>
		/// <param name="client"></param>
		/// <param name="msg"></param>
		public AdvobotCommandContext(IServiceProvider provider, IGuildSettings settings, DiscordShardedClient client, SocketUserMessage msg) : base(client, msg)
		{
			_Stopwatch.Start();
			GuildSettings = settings;
			BotSettings = provider.GetRequiredService<IBotSettings>();
			Provider = provider;
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
				$"{_Joiner}Time: {Message.CreatedAt.UtcDateTime.ToReadable()} ({ElapsedMilliseconds}ms)" +
				$"{_Joiner}Text: {Message.Content}";
			resp += result.ErrorReason == null ? "" : $"{_Joiner}Error: {result.ErrorReason}";
			return resp;
		}
	}
}