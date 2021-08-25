
using Discord;

namespace Advobot.Logging.Context.Messages
{
	public class MessagesBulkDeletedState : MessageDeletedState
	{
		public IReadOnlyList<IMessage> Messages { get; }

		public MessagesBulkDeletedState(IEnumerable<Cacheable<IMessage, ulong>> messages)
			: base(messages.First())
		{
			Messages = messages
				.Where(x => x.HasValue)
				.Select(x => x.Value)
				.OrderBy(x => x.Id)
				.ToArray();
		}
	}
}