
using Advobot.Logging.Database;
using Advobot.Logging.Utilities;

using AdvorangesUtils;

using Discord.WebSocket;

namespace Advobot.Logging.Service
{
	public sealed class NotificationService
	{
		private readonly INotificationDatabase _Db;

		public NotificationService(
			INotificationDatabase db,
			BaseSocketClient client)
		{
			_Db = db;

			client.UserJoined += OnUserJoined;
			client.UserLeft += OnUserLeft;
		}

		private async Task OnUserJoined(SocketGuildUser user)
		{
			var notification = await _Db.GetAsync(Notification.Welcome, user.Guild.Id).CAF();
			if (notification == null || notification.GuildId == 0)
			{
				return;
			}

			await notification.SendAsync(user.Guild, user).CAF();
		}

		private async Task OnUserLeft(SocketGuildUser user)
		{
			var notification = await _Db.GetAsync(Notification.Goodbye, user.Guild.Id).CAF();
			if (notification == null || notification.GuildId == 0)
			{
				return;
			}

			await notification.SendAsync(user.Guild, user).CAF();
		}
	}
}