using System.Threading.Tasks;

using Advobot.Logging.Database;
using Advobot.Logging.ReadOnlyModels;
using Advobot.Logging.Utilities;

using AdvorangesUtils;

using Discord.WebSocket;

namespace Advobot.Logging.Service
{
	public sealed class NotificationService : INotificationService
	{
		private readonly NotificationDatabase _Db;

		public NotificationService(
			NotificationDatabase db,
			BaseSocketClient client)
		{
			_Db = db;

			client.UserJoined += OnUserJoined;
			client.UserLeft += OnUserLeft;
		}

		public Task DisableAsync(Notification notification, ulong guildId)
			=> _Db.UpdateNotificationChannelAsync(notification, guildId, null);

		public Task<IReadOnlyCustomNotification?> GetAsync(Notification notification, ulong guildId)
			=> _Db.GetAsync(notification, guildId);

		public Task SetChannelAsync(Notification notification, ulong guildId, ulong channelId)
			=> _Db.UpdateNotificationChannelAsync(notification, guildId, channelId);

		public Task SetContentAsync(Notification notification, ulong guildId, string? content)
			=> _Db.UpdateNotificationContentAsync(notification, guildId, content);

		public Task SetEmbedAsync(Notification notification, ulong guildId, IReadOnlyCustomEmbed? embed)
			=> _Db.UpdateNotificationEmbedAsync(notification, guildId, embed);

		private async Task OnUserJoined(SocketGuildUser user)
		{
			var notification = await GetAsync(Notification.Welcome, user.Guild.Id).CAF();
			if (notification == null || notification.GuildId == 0)
			{
				return;
			}

			await notification.SendAsync(user.Guild, user).CAF();
		}

		private async Task OnUserLeft(SocketGuildUser user)
		{
			var notification = await GetAsync(Notification.Goodbye, user.Guild.Id).CAF();
			if (notification == null || notification.GuildId == 0)
			{
				return;
			}

			await notification.SendAsync(user.Guild, user).CAF();
		}
	}
}