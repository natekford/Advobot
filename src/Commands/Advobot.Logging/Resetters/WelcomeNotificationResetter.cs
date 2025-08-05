using Advobot.Logging.Database;

namespace Advobot.Logging.Resetters;

public sealed class WelcomeNotificationResetter(INotificationDatabase db) : NotificationResetter(db)
{
	protected override Notification Event => Notification.Welcome;
}