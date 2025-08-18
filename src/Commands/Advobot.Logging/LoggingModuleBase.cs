using Advobot.Logging.Database;
using Advobot.Modules;

namespace Advobot.Logging;

public abstract class LoggingModuleBase : AdvobotModuleBase
{
	public required LoggingDatabase Db { get; set; }
}