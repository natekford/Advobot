using Advobot.Logging.Database;
using Advobot.Modules;

using YACCS.Commands.Building;

namespace Advobot.Logging;

public abstract class LoggingModuleBase : AdvobotModuleBase
{
	[InjectService]
	public required LoggingDatabase Db { get; set; }
}