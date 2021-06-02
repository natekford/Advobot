using Advobot.SQLite.Relationships;

namespace Advobot.AutoMod.Models
{
	public record ChannelSettings(
		ulong GuildId,
		ulong ChannelId,
		bool IsImageOnly
	) : IGuildChild, IChannelChild
	{
		public ChannelSettings() : this(default, default, default) { }
	}
}