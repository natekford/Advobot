using Advobot.Logging.Models;

namespace Advobot.Logging.Database;

public interface INotificationDatabase
{
	Task<CustomNotification?> GetAsync(Notification notification, ulong guildId);

	Task<int> UpsertNotificationChannelAsync(Notification notification, ulong guildId, ulong? channelId);

	Task<int> UpsertNotificationContentAsync(Notification notification, ulong guildId, string? content);

	Task<int> UpsertNotificationEmbedAsync(Notification notification, ulong guildId, CustomEmbed? embed);
}