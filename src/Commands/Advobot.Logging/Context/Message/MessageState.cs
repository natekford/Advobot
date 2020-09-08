using System;
using System.Linq;
using System.Threading.Tasks;

using Advobot.Logging.Service;

using AdvorangesUtils;

using Discord;

namespace Advobot.Logging.Context.Messages
{
	public class MessageState : ILoggingState
	{
		public ITextChannel Channel { get; }
		public bool IsValid => !(Channel is null || Message is null || User is null);
		public IUserMessage Message { get; }
		public IGuildUser User { get; }
		public IGuild Guild => User.Guild;

		public MessageState(IMessage message)
		{
			Message = (message as IUserMessage)!;
			User = (message?.Author as IGuildUser)!;
			Channel = (message?.Channel as ITextChannel)!;
		}

		public virtual async Task<bool> CanLog(ILoggingService service, ILoggingContext context)
		{
			// Only log message updates and do actions on received messages if they're not a bot and not on an unlogged channel
			if (User.IsBot || User.IsWebhook)
			{
				return false;
			}

			var ignoredChannels = await service.GetIgnoredChannelsAsync(Channel.GuildId).CAF();
			return !ignoredChannels.Contains(Channel.Id);
		}
	}
}