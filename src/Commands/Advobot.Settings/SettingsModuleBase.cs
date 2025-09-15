using Advobot.Modules;
using Advobot.Settings.Database;

using YACCS.Commands.Building;

namespace Advobot.Settings;

public abstract class SettingsModuleBase : AdvobotModuleBase
{
	[InjectService]
	public required SettingsDatabase Db { get; set; }
}