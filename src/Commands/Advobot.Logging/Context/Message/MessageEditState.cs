using Discord;

namespace Advobot.Logging.Context.Messages;

public class MessageEditState(Cacheable<IMessage, ulong> before, IMessage message) : MessageState(message)
{
	public IMessage? Before { get; set; } = before.Value;
}