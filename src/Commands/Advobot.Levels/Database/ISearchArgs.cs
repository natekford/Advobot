namespace Advobot.Levels.Database
{
	public interface ISearchArgs
	{
		string? ChannelId { get; }
		string? GuildId { get; }
		string UserId { get; }
	}
}