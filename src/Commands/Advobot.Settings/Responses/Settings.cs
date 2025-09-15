using Advobot.Modules;
using Advobot.Utilities;

using YACCS.Commands.Models;

using static Advobot.Resources.Responses;

namespace Advobot.Settings.Responses;

public sealed class Settings : AdvobotResult
{
	public static AdvobotResult ClearedCommands(IEnumerable<IImmutableCommand> entries)
	{
		return Success(SettingsClearedCommands.Format(
			entries.Select(x => x.Paths[0].Join(" ").WithBlock().Current).Join().WithNoMarkdown()
		));
	}

	public static AdvobotResult ModifiedCommands(
		IEnumerable<IImmutableCommand> entries,
		int priority,
		bool enabled)
	{
		var format = enabled ? SettingsEnabledCommands : SettingsDisabledCommands;
		return Success(format.Format(
			priority.ToString().WithBlock(),
			entries.Select(x => x.Paths[0].Join(" ").WithBlock().Current).Join().WithNoMarkdown()
		));
	}
}