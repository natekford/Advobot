namespace Advobot.Levels.Database
{
	public interface ISearchArgs
	{
		ulong? ChannelId { get; }
		ulong? GuildId { get; }
		ulong? UserId { get; }
	}
}