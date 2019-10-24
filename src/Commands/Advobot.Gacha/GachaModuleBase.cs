using Advobot.Gacha.Displays;
using Advobot.Gacha.Trading;
using Advobot.Modules;

namespace Advobot.Gacha
{
	public abstract class GachaModuleBase : AdvobotModuleBase
	{
#pragma warning disable CS8618 // Non-nullable field is uninitialized.
		public DisplayManager Displays { get; set; }
		public ITradeService Trading { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized.
	}
}