using Advobot.Logging.Database;
using Advobot.Logging.Models;

namespace Advobot.Logging.Resetters;

public sealed class WelcomeNotificationResetter(INotificationDatabase db) : NotificationResetter(db)
{
	protected override Notification Event => Notification.Welcome;
}