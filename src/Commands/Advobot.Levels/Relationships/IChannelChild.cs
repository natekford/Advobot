namespace Advobot.Levels.Relationships
{
	public interface IChannelChild : IGuildChild
	{
		ulong ChannelId { get; }
	}
}