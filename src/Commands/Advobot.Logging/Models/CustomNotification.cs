using Advobot.Logging.ReadOnlyModels;
using Advobot.Logging.Relationships;
using Advobot.Logging.Utilities;

namespace Advobot.Logging.Models
{
	public class CustomNotification : CustomEmbed, IReadOnlyCustomNotification
	{
		public string ChannelId { get; set; } = null!;
		public string? Content { get; set; }
		public string GuildId { get; set; } = null!;

		ulong IChannelChild.ChannelId => ChannelId.ToId();
		ulong IGuildChild.GuildId => GuildId.ToId();
	}
}