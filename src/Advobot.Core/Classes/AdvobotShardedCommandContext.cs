using Advobot.Core.Interfaces;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;

namespace Advobot.Core.Classes
{
	/// <summary>
	/// A mostly copied and pasted implementation to having a sharded versino of <see cref="AdvobotSocketCommandContext"/>.
	/// </summary>
	public class AdvobotShardedCommandContext : AdvobotSocketCommandContext, IAdvobotCommandContext
	{
		public new DiscordShardedClient Client { get; }

		public AdvobotShardedCommandContext(IServiceProvider provider, IGuildSettings settings, DiscordShardedClient client, SocketUserMessage msg)
			: base(provider, settings, client.GetShard(GetShardId(client, (msg.Channel as SocketGuildChannel)?.Guild)), msg)
		{
			Client = client;
		}

		private static int GetShardId(DiscordShardedClient client, IGuild guild)
		{
			return guild == null ? 0 : client.GetShardIdFor(guild);
		}

		IDiscordClient ICommandContext.Client => Client;
	}
}
