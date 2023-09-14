using Advobot.Logging.Database;

namespace Advobot.Logging.OptionSetters;

public sealed class GoodbyeNotificationResetter(INotificationDatabase db) : NotificationResetter(db)
{
	protected override Notification Event => Notification.Goodbye;
}