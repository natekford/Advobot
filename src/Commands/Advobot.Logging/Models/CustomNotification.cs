using Advobot.Logging.ReadOnlyModels;

namespace Advobot.Logging.Models
{
	public class CustomNotification : CustomEmbed, IReadOnlyCustomNotification
	{
		public ulong ChannelId { get; set; }
		public string? Content { get; set; }
		public ulong GuildId { get; set; }
	}
}