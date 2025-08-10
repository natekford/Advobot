using Advobot.Modules;
using Advobot.Services.Help;
using Advobot.Utilities;

using static Advobot.Resources.Responses;

namespace Advobot.Settings.Responses;

public sealed class Settings : AdvobotResult
{
	public Settings() : base(null, "")
	{
	}

	public static AdvobotResult ClearedCommands(IEnumerable<IHelpModule> entries)
	{
		return Success(SettingsClearedCommands.Format(
			entries.Select(x => x.Name.WithBlock().Value).Join().WithNoMarkdown()
		));
	}

	public static AdvobotResult ModifiedCommands(
		IEnumerable<IHelpModule> entries,
		int priority,
		bool enabled)
	{
		var format = enabled ? SettingsEnabledCommands : SettingsDisabledCommands;
		return Success(format.Format(
			priority.ToString().WithBlock(),
			entries.Select(x => x.Name.WithBlock().Value).Join().WithNoMarkdown()
		));
	}
}