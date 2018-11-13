using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Advobot.Classes
{
	/// <summary>
	/// Messages that will get deleted after the time has passed.
	/// </summary>
	public class RemovableMessage : DatabaseEntry
	{
		/// <summary>
		/// Caches request options for messages.
		/// </summary>
		protected static RequestOptions Options { get; } = DiscordUtils.GenerateRequestOptions("Automatic message deletion.");

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

		/// <summary>
		/// Processes the removable messages in a way which is more efficient than deleting them one by one.
		/// </summary>
		/// <param name="client"></param>
		/// <param name="alreadyDeleted"></param>
		/// <param name="removableMessages"></param>
		/// <returns></returns>
		public static async Task ProcessRemovableMessagesAsync(
			BaseSocketClient client,
			ConcurrentBag<ulong> alreadyDeleted,
			IEnumerable<RemovableMessage> removableMessages)
		{
			foreach (var guildGroup in removableMessages.Where(x => x != null).GroupBy(x => x.GuildId))
			{
				if (!(client.GetGuild(guildGroup.Key) is SocketGuild guild))
				{
					continue;
				}
				foreach (var channelGroup in guildGroup.GroupBy(x => x.ChannelId))
				{
					if (!(guild.GetTextChannel(channelGroup.Key) is SocketTextChannel channel))
					{
						continue;
					}
					var messageIds = channelGroup.SelectMany(g => g.MessageIds);
					var messages = await GetValidMessagesAsync(channel, alreadyDeleted, messageIds).CAF();
					await MessageUtils.DeleteMessagesAsync(channel, messages, Options).CAF();
				}
			}
		}
		/// <summary>
		/// Attempts to get every message unless it has already been deleted or the id is 0.
		/// </summary>
		/// <param name="channel"></param>
		/// <param name="alreadyDeleted"></param>
		/// <param name="ids"></param>
		/// <returns></returns>
		private static async Task<IMessage[]> GetValidMessagesAsync(
			SocketTextChannel channel,
			ConcurrentBag<ulong> alreadyDeleted,
			IEnumerable<ulong> ids)
		{
			var invalid = new List<ulong>(alreadyDeleted);
			var tasks = new List<Task<IMessage>>();
			foreach (var id in ids.Where(x => x != 0 && !invalid.Contains(x)))
			{
				alreadyDeleted.Add(id);
				tasks.Add(channel.GetMessageAsync(id));
			}
			return await Task.WhenAll(tasks).CAF();
		}
	}
}
