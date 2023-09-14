using Advobot.Logging.Database;

namespace Advobot.Logging.OptionSetters;

public sealed class WelcomeNotificationResetter(INotificationDatabase db) : NotificationResetter(db)
{
	protected override Notification Event => Notification.Welcome;
}