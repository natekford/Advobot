namespace Advobot.Levels.Database
{
	public interface ISearchArgs
	{
		string? ChannelId { get; }
		ulong? ChannelIdValue { get; }
		string? GuildId { get; }
		ulong? GuildIdValue { get; }
		string? UserId { get; }
		ulong? UserIdValue { get; }
	}
}