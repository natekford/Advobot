using System.Threading.Tasks;

using Advobot.Logging.Service;
using Advobot.Services;

using AdvorangesUtils;

using Discord.Commands;

namespace Advobot.Logging.OptionSetters
{
	public abstract class DefaultNotificationSetter : IDefaultOptionsSetter
	{
		private readonly INotificationService _Notifications;
		protected abstract Notification Event { get; }

		protected DefaultNotificationSetter(INotificationService notifications)
		{
			_Notifications = notifications;
		}

		public async Task SetAsync(ICommandContext context)
		{
			await _Notifications.SetContentAsync(Event, context.Guild.Id, null).CAF();
			await _Notifications.SetEmbedAsync(Event, context.Guild.Id, null).CAF();
			await _Notifications.DisableAsync(Event, context.Guild.Id).CAF();
		}
	}
}