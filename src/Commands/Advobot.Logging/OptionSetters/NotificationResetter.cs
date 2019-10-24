using System.Threading.Tasks;

using Advobot.Logging.Service;
using Advobot.Services;

using AdvorangesUtils;

using Discord.Commands;

namespace Advobot.Logging.OptionSetters
{
	public abstract class NotificationResetter : IResetter
	{
		private readonly INotificationService _Notifications;
		protected abstract Notification Event { get; }

		protected NotificationResetter(INotificationService notifications)
		{
			_Notifications = notifications;
		}

		public async Task ResetAsync(ICommandContext context)
		{
			await _Notifications.SetContentAsync(Event, context.Guild.Id, null).CAF();
			await _Notifications.SetEmbedAsync(Event, context.Guild.Id, null).CAF();
			await _Notifications.DisableAsync(Event, context.Guild.Id).CAF();
		}
	}
}