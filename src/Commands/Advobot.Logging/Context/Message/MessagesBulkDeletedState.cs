using Discord;

namespace Advobot.Logging.Context.Message;

public class MessagesBulkDeletedState(IEnumerable<Cacheable<IMessage, ulong>> messages) : MessageDeletedState(messages.First())
{
	public IReadOnlyList<IMessage> Messages { get; } = [.. messages
			.Where(x => x.HasValue)
			.Select(x => x.Value)
			.OrderBy(x => x.Id)];
}