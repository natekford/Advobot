using Advobot.Core.Interfaces;
using Discord;
using Discord.WebSocket;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Advobot.Core.Classes.Punishments
{
	/// <summary>
	/// Messages that will get deleted after the time has passed.
	/// </summary>
	public class RemovableMessage : ITime
	{
		/// <summary>
		/// The id of the object for LiteDB.
		/// </summary>
		public ObjectId Id { get; set; }
		/// <summary>
		/// The time to remove the message(s) at.
		/// </summary>
		public DateTime Time { get; set; }
		/// <summary>
		/// The guild they're located in.
		/// </summary>
		public ulong GuildId { get; set; }
		/// <summary>
		/// The channel they're located in.
		/// </summary>
		public ulong ChannelId { get; set; }
		/// <summary>
		/// The messages to remove.
		/// </summary>
		public List<ulong> MessageIds { get; set; }

		public RemovableMessage() { }
		public RemovableMessage(TimeSpan time, SocketTextChannel channel, params IUserMessage[] messages)
		{
			MessageIds = messages.Select(x => x.Id).ToList();
			ChannelId = channel.Id;
			GuildId = channel.Guild.Id;
			Time = DateTime.UtcNow.Add(time);
		}
	}
}
