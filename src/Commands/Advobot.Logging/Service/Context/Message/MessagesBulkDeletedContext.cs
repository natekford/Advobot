using Discord;

namespace Advobot.Logging.Service.Context.Message;

public class MessagesBulkDeletedContext(
	IEnumerable<(IMessage? Message, ulong Id)> messages
) : MessageContext(messages.FirstOrDefault(x => x.Message is not null).Message)
{
	public IReadOnlyList<IMessage> Messages { get; } = [.. messages
		.Select(x => x.Message)
		.Where(x => x is not null)
		.OrderBy(x => x!.Id)!
	];
}