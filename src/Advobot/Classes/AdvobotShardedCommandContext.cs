using Advobot.Interfaces;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;

namespace Advobot.Classes
{
	/// <summary>
	/// A mostly copied and pasted implementation to having a sharded versino of <see cref="AdvobotSocketCommandContext"/>.
	/// </summary>
	public class AdvobotShardedCommandContext : AdvobotSocketCommandContext, ICommandContext
	{
		/// <summary>
		/// The client for the command.
		/// </summary>
		public new DiscordShardedClient Client { get; }

		/// <summary>
		/// Creates an instance of <see cref="AdvobotShardedCommandContext"/>.
		/// </summary>
		/// <param name="provider"></param>
		/// <param name="settings"></param>
		/// <param name="client"></param>
		/// <param name="msg"></param>
		public AdvobotShardedCommandContext(IServiceProvider provider, IGuildSettings settings, DiscordShardedClient client, SocketUserMessage msg)
			: base(provider, settings, client.GetShardFor((msg.Channel as SocketGuildChannel)?.Guild), msg)
		{
			Client = client;
		}

		IDiscordClient ICommandContext.Client => Client;
	}
}
