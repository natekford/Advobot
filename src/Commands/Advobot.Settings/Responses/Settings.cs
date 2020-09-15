using System;
using System.Collections.Generic;
using System.Text;

using Advobot.Modules;
using Advobot.Services.HelpEntries;
using Advobot.Utilities;

using AdvorangesUtils;

using static Advobot.Resources.Responses;

namespace Advobot.Settings.Responses
{
	public sealed class Settings : AdvobotResult
	{
		public Settings() : base(null, "")
		{
		}

		public static AdvobotResult ClearedCommands(IEnumerable<IModuleHelpEntry> entries)
		{
			return Success(SettingsClearedCommands.Format(
				entries.Join(x => x.Name.WithBlock().Value).WithNoMarkdown()
			));
		}

		public static AdvobotResult ModifiedCommands(
			IEnumerable<IModuleHelpEntry> entries,
			int priority,
			bool enabled)
		{
			var format = enabled ? SettingsEnabledCommands : SettingsDisabledCommands;
			return Success(format.Format(
				priority.ToString().WithBlock(),
				entries.Join(x => x.Name.WithBlock().Value).WithNoMarkdown()
			));
		}
	}
}