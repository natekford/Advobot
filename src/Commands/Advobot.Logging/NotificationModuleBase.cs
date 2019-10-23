using Advobot.Logging.Service;
using Advobot.Modules;

namespace Advobot.Logging
{
	public abstract class NotificationModuleBase : AdvobotModuleBase
	{
#pragma warning disable CS8618 // Non-nullable field is uninitialized.
		public INotificationService Notifications { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized.
	}
}