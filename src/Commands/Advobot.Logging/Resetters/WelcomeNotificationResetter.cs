using Advobot.Logging.Database;

namespace Advobot.Logging.OptionSetters
{
	public sealed class WelcomeNotificationResetter : NotificationResetter
	{
		protected override Notification Event => Notification.Welcome;

		public WelcomeNotificationResetter(INotificationDatabase db) : base(db)
		{
		}
	}
}