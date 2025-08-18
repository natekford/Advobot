using Advobot.Logging.Database;
using Advobot.Modules;

namespace Advobot.Logging;

public abstract class NotificationModuleBase : AdvobotModuleBase
{
	public required NotificationDatabase Db { get; set; }
}