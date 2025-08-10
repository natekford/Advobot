using Discord;

namespace Advobot.Logging.Service.Context.Message;

public class MessageEditedState(Cacheable<IMessage, ulong> before, IMessage message)
	: MessageState(message)
{
	public IMessage? Before { get; set; } = before.Value;
}