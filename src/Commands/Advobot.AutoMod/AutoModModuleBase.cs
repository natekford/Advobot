using Advobot.AutoMod.Database;
using Advobot.Modules;

namespace Advobot.AutoMod;

public abstract class AutoModModuleBase : AdvobotModuleBase
{
	public IAutoModDatabase Db { get; set; } = null!;
}