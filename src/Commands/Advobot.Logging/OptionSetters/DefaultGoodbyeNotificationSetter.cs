using Advobot.Logging.Service;

namespace Advobot.Logging.OptionSetters
{
	public sealed class DefaultGoodbyeNotificationSetter : DefaultNotificationSetter
	{
		protected override Notification Event => Notification.Goodbye;

		public DefaultGoodbyeNotificationSetter(INotificationService notifications) : base(notifications)
		{
		}
	}
}