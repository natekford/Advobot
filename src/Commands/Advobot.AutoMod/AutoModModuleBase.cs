using Advobot.AutoMod.Database;
using Advobot.Modules;

namespace Advobot.AutoMod;

public abstract class AutoModModuleBase : AdvobotModuleBase
{
	public required IAutoModDatabase Db { get; set; }
}