﻿using System;
using System.Collections.Generic;
using System.Linq;
using Discord;
using Discord.Commands;

namespace Advobot.Classes
{
	/// <summary>
	/// Messages that will get deleted after the time has passed.
	/// </summary>
	public class RemovableMessage : DatabaseEntry
	{
		/// <summary>
		/// The id of the guild from the passed in context.
		/// </summary>
		public ulong GuildId { get; set; }
		/// <summary>
		/// The id of the channel from the passed in context.
		/// </summary>
		public ulong ChannelId { get; set; }
		/// <summary>
		/// The id of the user from the passed in context.
		/// </summary>
		public ulong UserId { get; set; }
		/// <summary>
		/// The ids of the passed in messages.
		/// </summary>
		public List<ulong> MessageIds { get; set; } = new List<ulong>();

		/// <summary>
		/// Creates an instance of <see cref="RemovableMessage"/>. Parameterless constructor is used for the database.
		/// </summary>
		public RemovableMessage() : base() { }
		/// <summary>
		/// Creates an instance of <see cref="RemovableMessage"/>.
		/// </summary>
		/// <param name="time"></param>
		/// <param name="context"></param>
		/// <param name="messages"></param>
		public RemovableMessage(ICommandContext context, IEnumerable<IMessage> messages, TimeSpan time = default)
			: this(context.Guild, context.Channel, context.User, messages, time) { }
		/// <summary>
		/// Creates an instance of removable messages with the supplied messages on the guild/channel passed in.
		/// </summary>
		/// <param name="time"></param>
		/// <param name="guild"></param>
		/// <param name="channel"></param>
		/// <param name="user"></param>
		/// <param name="messages"></param>
		public RemovableMessage(IGuild guild, IMessageChannel channel, IUser user, IEnumerable<IMessage> messages, TimeSpan time = default)
			: base(time)
		{
			GuildId = guild.Id;
			ChannelId = channel.Id;
			UserId = user.Id;
			MessageIds = messages.Select(x => x.Id).ToList();
		}
	}
}
