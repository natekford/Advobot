namespace Advobot.Levels.Relationships
{
	public interface IChannelChild : IGuildChild
	{
		string ChannelId { get; }
	}
}