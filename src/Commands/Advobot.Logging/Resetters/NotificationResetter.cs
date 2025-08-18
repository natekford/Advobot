using Advobot.Logging.Database;
using Advobot.Logging.Database.Models;
using Advobot.Services;

using Discord.Commands;

namespace Advobot.Logging.Resetters;

public abstract class NotificationResetter(NotificationDatabase db) : IResetter
{
	private readonly NotificationDatabase _Db = db;
	protected abstract Notification Event { get; }

	public async Task ResetAsync(ICommandContext context)
	{
		await _Db.UpsertNotificationContentAsync(Event, context.Guild.Id, null).ConfigureAwait(false);
		await _Db.UpsertNotificationEmbedAsync(Event, context.Guild.Id, null).ConfigureAwait(false);
		await _Db.UpsertNotificationChannelAsync(Event, context.Guild.Id, null).ConfigureAwait(false);
	}
}