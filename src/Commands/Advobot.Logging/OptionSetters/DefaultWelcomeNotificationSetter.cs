using Advobot.Logging.Service;

namespace Advobot.Logging.OptionSetters
{
	public sealed class DefaultWelcomeNotificationSetter : DefaultNotificationSetter
	{
		protected override Notification Event => Notification.Welcome;

		public DefaultWelcomeNotificationSetter(INotificationService notifications) : base(notifications)
		{
		}
	}
}