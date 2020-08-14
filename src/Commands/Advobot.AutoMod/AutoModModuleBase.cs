using Advobot.AutoMod.Database;
using Advobot.Modules;

namespace Advobot.AutoMod
{
	public abstract class AutoModModuleBase : AdvobotModuleBase
	{
#pragma warning disable CS8618 // Non-nullable field is uninitialized.
		public IAutoModDatabase Db { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized.
	}
}