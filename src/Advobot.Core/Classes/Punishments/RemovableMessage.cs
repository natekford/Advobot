using Advobot.Core.Interfaces;
using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace Advobot.Core.Classes.Punishments
{
	/// <summary>
	/// Messages that will get deleted after the time is less than <see cref="DateTime.UtcNow"/>.
	/// </summary>
	public struct RemovableMessage : ITime
	{
		public DateTime Time { get; private set; }
		public ImmutableList<ulong> MessageIds { get; private set; }
		public ulong ChannelId { get; private set; }
		public ulong GuildId { get; private set; }

		public RemovableMessage(TimeSpan time, SocketTextChannel channel, params IUserMessage[] messages)
		{
			MessageIds = messages.Select(x => x.Id).ToImmutableList();
			ChannelId = channel.Id;
			GuildId = channel.Guild.Id;
			Time = DateTime.UtcNow.Add(time);
		}
	}
}
