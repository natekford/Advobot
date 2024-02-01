using Advobot.Modules;
using Advobot.Settings.Database;

namespace Advobot.Settings;

public abstract class SettingsModuleBase : AdvobotModuleBase
{
	public ISettingsDatabase Db { get; set; } = null!;
}