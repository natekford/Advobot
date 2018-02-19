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

		public RemovableMessage() : base(default) { }
		public RemovableMessage(TimeSpan time, ICommandContext context, params IUserMessage[] messages)
			: this(time, context.Guild, context.Channel, context.User, messages) { }
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
