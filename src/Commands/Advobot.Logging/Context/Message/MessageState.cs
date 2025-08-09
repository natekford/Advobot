using Advobot.Logging.Database;

using Discord;

namespace Advobot.Logging.Context.Message;

public class MessageState(IMessage message) : ILogState
{
	public ITextChannel Channel { get; } = (message?.Channel as ITextChannel)!;
	public IGuild Guild => User.Guild;
	public bool IsValid => !(Channel is null || Message is null || User is null);
	public IUserMessage Message { get; } = (message as IUserMessage)!;
	public IGuildUser User { get; } = (message?.Author as IGuildUser)!;

	public virtual async Task<bool> CanLog(ILoggingDatabase db, ILogContext context)
	{
		// Only log message updates and do actions on received messages if they're not a bot and not on an unlogged channel
		if (User.IsBot || User.IsWebhook)
		{
			return false;
		}

		var ignoredChannels = await db.GetIgnoredChannelsAsync(Channel.GuildId).ConfigureAwait(false);
		return !ignoredChannels.Contains(Channel.Id);
	}
}