using Advobot.Gacha.Displays;
using Advobot.Modules;

namespace Advobot.Gacha;

public abstract class GachaModuleBase : AdvobotModuleBase
{
	public DisplayManager Displays { get; set; } = null!;
}