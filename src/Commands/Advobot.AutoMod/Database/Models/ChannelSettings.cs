using Advobot.SQLite.Relationships;

namespace Advobot.AutoMod.Database.Models;

public record ChannelSettings(
	ulong GuildId,
	ulong ChannelId,
	bool IsImageOnly
) : IGuildChild, IChannelChild
{
	public ChannelSettings() : this(default, default, default) { }
}