using System.Threading.Tasks;

using Advobot.Logging.ReadOnlyModels;

namespace Advobot.Logging.Service
{
	public interface INotificationService
	{
		Task DisableAsync(Notification notification, ulong guildId);

		Task<IReadOnlyCustomNotification?> GetAsync(Notification notification, ulong guildId);

		Task SetChannelAsync(Notification notification, ulong guildId, ulong channelId);

		Task SetContentAsync(Notification notification, ulong guildId, string? content);

		Task SetEmbedAsync(Notification notification, ulong guildId, IReadOnlyCustomEmbed? embed);
	}
}