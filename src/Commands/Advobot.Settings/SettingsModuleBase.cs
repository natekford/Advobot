using Advobot.Modules;
using Advobot.Settings.Database;

namespace Advobot.Settings
{
	public abstract class SettingsModuleBase : AdvobotModuleBase
	{
#pragma warning disable CS8618 // Non-nullable field is uninitialized.
		public ISettingsDatabase Db { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized.
	}
}