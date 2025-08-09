using Advobot.Logging.Database;
using Advobot.Services;

using Discord.Commands;

namespace Advobot.Logging.Resetters;

public abstract class NotificationResetter(INotificationDatabase db) : IResetter
{
	private readonly INotificationDatabase _Db = db;
	protected abstract Notification Event { get; }

	public async Task ResetAsync(ICommandContext context)
	{
		await _Db.UpsertNotificationContentAsync(Event, context.Guild.Id, null).ConfigureAwait(false);
		await _Db.UpsertNotificationEmbedAsync(Event, context.Guild.Id, null).ConfigureAwait(false);
		await _Db.UpsertNotificationChannelAsync(Event, context.Guild.Id, null).ConfigureAwait(false);
	}
}