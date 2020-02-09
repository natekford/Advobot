using Advobot.Logging.Service;

namespace Advobot.Logging.OptionSetters
{
	public sealed class WelcomeNotificationResetter : NotificationResetter
	{
		protected override Notification Event => Notification.Welcome;

		public WelcomeNotificationResetter(INotificationService notifications)
			: base(notifications)
		{
		}
	}
}