using Discord;

namespace Advobot.Logging.Service.Context.Message;

public class MessageEditedContext(Cacheable<IMessage, ulong> before, IMessage message)
	: MessageContext(message)
{
	public IMessage? Before { get; set; } = before.Value;
}