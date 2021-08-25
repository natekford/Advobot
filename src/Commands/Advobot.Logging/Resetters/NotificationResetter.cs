
using Advobot.Logging.Database;
using Advobot.Services;

using AdvorangesUtils;

using Discord.Commands;

namespace Advobot.Logging.OptionSetters
{
	public abstract class NotificationResetter : IResetter
	{
		private readonly INotificationDatabase _Db;
		protected abstract Notification Event { get; }

		protected NotificationResetter(INotificationDatabase db)
		{
			_Db = db;
		}

		public async Task ResetAsync(ICommandContext context)
		{
			await _Db.UpsertNotificationContentAsync(Event, context.Guild.Id, null).CAF();
			await _Db.UpsertNotificationEmbedAsync(Event, context.Guild.Id, null).CAF();
			await _Db.UpsertNotificationChannelAsync(Event, context.Guild.Id, null).CAF();
		}
	}
}