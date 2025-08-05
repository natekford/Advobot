using Advobot.Logging.Database;

namespace Advobot.Logging.Resetters;

public sealed class GoodbyeNotificationResetter(INotificationDatabase db) : NotificationResetter(db)
{
	protected override Notification Event => Notification.Goodbye;
}