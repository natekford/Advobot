using Discord;

namespace Advobot.Logging.Context.Messages
{
	public class MessageEditState : MessageState
	{
		public IMessage? Before { get; set; }

		public MessageEditState(Cacheable<IMessage, ulong> before, IMessage message) : base(message)
		{
			Before = before.Value;
		}
	}
}