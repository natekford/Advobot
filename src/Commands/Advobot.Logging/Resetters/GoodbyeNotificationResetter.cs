using Advobot.Logging.Service;

namespace Advobot.Logging.OptionSetters
{
	public sealed class GoodbyeNotificationResetter : NotificationResetter
	{
		protected override Notification Event => Notification.Goodbye;

		public GoodbyeNotificationResetter(INotificationService notifications)
			: base(notifications)
		{
		}
	}
}