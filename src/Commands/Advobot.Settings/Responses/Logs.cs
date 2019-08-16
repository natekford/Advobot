using System.Collections.Generic;
using Advobot.Formatting;
using Advobot.Modules;
using Advobot.Services.GuildSettings.Settings;
using Advobot.Utilities;
using Discord;

namespace Advobot.Settings.Responses
{
	public sealed class Logs : CommandResponses
	{
		private Logs() { }

		public static AdvobotResult SetLog(string logType, ITextChannel channel)
			=> Success(Default.FormatInterpolated($"Successfully set {channel} as the {logType.NoFormatting()} log."));
		public static AdvobotResult Removed(string logType)
			=> Success(Default.FormatInterpolated($"Successfully removed the {logType.NoFormatting()} log."));
		public static AdvobotResult ModifiedIgnoredLogChannels(IReadOnlyCollection<IGuildChannel> channels, bool ignored)
			=> Success(Default.FormatInterpolated($"Successfully {GetIgnored(ignored)} the channels {channels}."));
		public static AdvobotResult DefaultLogActions()
			=> Success("Successfully set the log actions to the default ones.");
		public static AdvobotResult ModifiedAllLogActions(bool enable)
			=> Success(Default.FormatInterpolated($"Successfully {GetEnabled(enable)} every log action."));
		public static AdvobotResult ModifiedLogActions(IReadOnlyCollection<LogAction> logActions, bool enable)
			=> Success(Default.FormatInterpolated($"Successfully {GetEnabled(enable)} the following log actions: {logActions}."));
	}
}