using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Advobot.Core.Classes.Punishments
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
		public List<ulong> MessageIds { get; set; }

		/// <summary>
		/// Initializes the object. Parameterless constructor is used for the database.
		/// </summary>
		public RemovableMessage() : base(default) { }
		/// <summary>
		/// Creates an instance of removable messages with the supplied messages on the guild/channel in the context.
		/// </summary>
		/// <param name="time"></param>
		/// <param name="context"></param>
		/// <param name="messages"></param>
		public RemovableMessage(TimeSpan time, ICommandContext context, params IUserMessage[] messages)
			: this(time, context.Guild, context.Channel, context.User, messages) { }
		/// <summary>
		/// Creates an instance of removable messages with the supplied messages on the guild/channel passed in.
		/// </summary>
		/// <param name="time"></param>
		/// <param name="guild"></param>
		/// <param name="channel"></param>
		/// <param name="user"></param>
		/// <param name="messages"></param>
		public RemovableMessage(TimeSpan time, IGuild guild, IMessageChannel channel, IUser user, params IUserMessage[] messages)
			: base(time)
		{
			GuildId = guild.Id;
			ChannelId = channel.Id;
			UserId = user.Id;
			MessageIds = messages.Select(x => x.Id).ToList();
		}
	}
}
