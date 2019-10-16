namespace Advobot.Logging.Relationships
{
	public interface IChannelChild : IGuildChild
	{
		ulong ChannelId { get; }
	}
}