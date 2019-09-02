namespace Advobot.Levels.Relationships
{
	public interface IMessageChild : IChannelChild
	{
		string MessageId { get; }
	}
}