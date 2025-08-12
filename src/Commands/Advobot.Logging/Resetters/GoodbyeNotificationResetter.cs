using Advobot.Logging.Database;
using Advobot.Logging.Database.Models;

namespace Advobot.Logging.Resetters;

public sealed class GoodbyeNotificationResetter(INotificationDatabase db) : NotificationResetter(db)
{
	protected override Notification Event => Notification.Goodbye;
}