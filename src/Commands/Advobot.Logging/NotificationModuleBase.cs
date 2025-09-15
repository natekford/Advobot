using Advobot.Logging.Database;
using Advobot.Modules;

using YACCS.Commands.Building;

namespace Advobot.Logging;

public abstract class NotificationModuleBase : AdvobotModuleBase
{
	[InjectService]
	public required NotificationDatabase Db { get; set; }
}