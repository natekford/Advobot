using Advobot.AutoMod.Database;
using Advobot.Modules;

using YACCS.Commands.Building;

namespace Advobot.AutoMod;

public abstract class AutoModModuleBase : AdvobotModuleBase
{
	[InjectService]
	public required AutoModDatabase Db { get; set; }
}