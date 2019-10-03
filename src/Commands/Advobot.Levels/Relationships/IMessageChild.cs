namespace Advobot.Levels.Relationships
{
	public interface IMessageChild : IChannelChild
	{
		ulong MessageId { get; }
	}
}