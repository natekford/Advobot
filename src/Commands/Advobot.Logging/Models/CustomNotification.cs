using Advobot.SQLite.Relationships;

namespace Advobot.Logging.Models
{
	public record CustomNotification(
		ulong ChannelId,
		string? Content,
		ulong GuildId
	) : CustomEmbed, IChannelChild
	{
		public CustomNotification() : this(default, default, default) { }
	}
}