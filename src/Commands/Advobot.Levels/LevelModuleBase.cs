using Advobot.Levels.Database;
using Advobot.Levels.Service;
using Advobot.Modules;

using YACCS.Commands.Building;

namespace Advobot.Levels;

public abstract class LevelModuleBase : AdvobotModuleBase
{
	[InjectService]
	public required LevelDatabase Db { get; set; }
	[InjectService]
	public required LevelService Service { get; set; }
}