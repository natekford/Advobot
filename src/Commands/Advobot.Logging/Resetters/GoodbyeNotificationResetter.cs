using Advobot.Logging.Database;
using Advobot.Logging.Database.Models;

namespace Advobot.Logging.Resetters;

public sealed class GoodbyeNotificationResetter(NotificationDatabase db) : NotificationResetter(db)
{
	protected override Notification Event => Notification.Goodbye;
}