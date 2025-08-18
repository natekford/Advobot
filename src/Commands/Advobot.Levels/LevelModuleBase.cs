using Advobot.Levels.Database;
using Advobot.Levels.Service;
using Advobot.Modules;

namespace Advobot.Levels;

public abstract class LevelModuleBase : AdvobotModuleBase
{
	public required LevelDatabase Db { get; set; }
	public required LevelService Service { get; set; }
}