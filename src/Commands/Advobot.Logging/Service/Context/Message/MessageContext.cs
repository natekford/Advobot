using Advobot.Logging.Database;

using Discord;

namespace Advobot.Logging.Service.Context.Message;

public class MessageContext(IMessage message) : ILogContext
{
	public ITextChannel Channel { get; } = (message?.Channel as ITextChannel)!;
	public IGuild Guild => User?.Guild!;
	public IUserMessage Message { get; } = (message as IUserMessage)!;
	public IGuildUser User { get; } = (message?.Author as IGuildUser)!;

	public virtual async Task<bool> IsValidAsync(LoggingDatabase db)
	{
		if (Guild is null || Channel is null || Message is null
			|| User?.IsBot != false || User.IsWebhook)
		{
			return false;
		}

		var ignoredChannels = await db.GetIgnoredChannelsAsync(Guild.Id).ConfigureAwait(false);
		return !ignoredChannels.Contains(Channel.Id);
	}
}