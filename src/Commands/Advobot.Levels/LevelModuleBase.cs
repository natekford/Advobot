using Advobot.Levels.Database;
using Advobot.Levels.Service;
using Advobot.Modules;

namespace Advobot.Levels;

public abstract class LevelModuleBase : AdvobotModuleBase
{
	public ILevelDatabase Db { get; set; } = null!;
	public ILevelService Service { get; set; } = null!;
}