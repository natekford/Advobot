using Advobot.Levels.Database;
using Advobot.Levels.Service;
using Advobot.Modules;

namespace Advobot.Levels;

public abstract class LevelModuleBase : AdvobotModuleBase
{
	public required ILevelDatabase Db { get; set; }
	public required LevelService Service { get; set; }
}