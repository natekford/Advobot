using Discord;

namespace Advobot.Logging.Service.Context.Message;

public class MessagesBulkDeletedContext(IEnumerable<Cacheable<IMessage, ulong>> messages)
	: MessageContext(messages.First().Value)
{
	public IReadOnlyList<IMessage> Messages { get; } = [.. messages
		.Where(x => x.HasValue)
		.Select(x => x.Value)
		.OrderBy(x => x.Id)
	];
}