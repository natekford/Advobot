using YACCS.NamedArguments;

namespace Advobot.Levels.Database;

[GenerateNamedArguments]
public sealed class SearchArgs
{
	public ulong? ChannelId { get; set; }
	public ulong? GuildId { get; set; }
	public ulong? UserId { get; set; }

	public SearchArgs()
	{
	}

	public SearchArgs(ulong? userId = null, ulong? guildId = null, ulong? channelId = null)
	{
		UserId = userId;
		GuildId = guildId;
		ChannelId = channelId;
	}
}