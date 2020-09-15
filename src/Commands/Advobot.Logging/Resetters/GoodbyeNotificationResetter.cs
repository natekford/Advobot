using Advobot.Logging.Database;

namespace Advobot.Logging.OptionSetters
{
	public sealed class GoodbyeNotificationResetter : NotificationResetter
	{
		protected override Notification Event => Notification.Goodbye;

		public GoodbyeNotificationResetter(INotificationDatabase db) : base(db)
		{
		}
	}
}