using System.Diagnostics;

using Advobot.Services.GuildSettings;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Advobot.Modules
{
	/// <summary>
	/// A <see cref="ShardedCommandContext"/> which contains settings and the service provider.
	/// </summary>
	public class AdvobotCommandContext : ShardedCommandContext, IAdvobotCommandContext
	{
		private readonly Stopwatch _Stopwatch = new Stopwatch();

		/// <summary>
		/// The channel this command is executing from.
		/// </summary>
		public new SocketTextChannel Channel { get; }

		/// <summary>
		/// The settings for the guild.
		/// </summary>
		public IGuildSettings Settings { get; }

		/// <summary>
		/// The user this command is executing from.
		/// </summary>
		public new SocketGuildUser User { get; }

		ITextChannel IAdvobotCommandContext.Channel => Channel;

		/// <summary>
		/// The time since starting the command.
		/// </summary>
		public long ElapsedMilliseconds => _Stopwatch.ElapsedMilliseconds;

		IGuildUser IAdvobotCommandContext.User => User;

		/// <summary>
		/// Creates an instance of <see cref="AdvobotCommandContext"/>.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="client"></param>
		/// <param name="msg"></param>
		public AdvobotCommandContext(
			IGuildSettings settings,
			DiscordShardedClient client,
			SocketUserMessage msg)
			: base(client, msg)
		{
			_Stopwatch.Start();
			User = (SocketGuildUser)msg.Author;
			Channel = (SocketTextChannel)msg.Channel;
			Settings = settings;
		}
	}
}