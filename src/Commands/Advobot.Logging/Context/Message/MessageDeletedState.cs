using System;
using System.Linq;
using System.Threading.Tasks;

using Advobot.Logging.Service;

using AdvorangesUtils;

using Discord;

namespace Advobot.Logging.Context.Messages
{
	public class MessageDeletedState : MessageState
	{
		public MessageDeletedState(Cacheable<IMessage, ulong> cached)
			: base(cached.Value)
		{
		}

		public override async Task<bool> CanLog(ILoggingService service, ILogContext context)
		{
			// Log all deleted messages, no matter the source user, unless they're on an unlogged channel
			var ignoredChannels = await service.GetIgnoredChannelsAsync(Channel.GuildId).CAF();
			return !ignoredChannels.Contains(Channel.Id);
		}
	}
}