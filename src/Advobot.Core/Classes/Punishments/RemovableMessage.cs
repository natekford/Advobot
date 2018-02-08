using Advobot.Core.Interfaces;
using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Immutable;

namespace Advobot.Core.Classes.Punishments
{
	/// <summary>
	/// Messages that will get deleted after the time is less than <see cref="DateTime.UtcNow"/>.
	/// </summary>
	public struct RemovableMessage : ITime
	{
		public ImmutableList<IUserMessage> Messages { get; }
		public SocketTextChannel Channel { get; }
		public DateTime Time { get; }

		public RemovableMessage(TimeSpan time, SocketTextChannel channel, params IUserMessage[] messages)
		{
			Messages = messages.ToImmutableList();
			Channel = channel;
			Time = DateTime.UtcNow.Add(time);
		}
	}
}
