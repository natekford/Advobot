using System.Collections.Generic;

using Advobot.Modules;
using Advobot.Services.GuildSettings.Settings;
using Advobot.Utilities;

using AdvorangesUtils;

using Discord;

using static Advobot.Logging.Resources.Responses;

namespace Advobot.Logging.Responses
{
	public sealed class Logging : AdvobotResult
	{
		private Logging() : base(null, "")
		{
		}

		public static AdvobotResult DefaultLogActions()
			=> Success(LoggingDefaultLogActions);

		public static AdvobotResult ModifiedAllLogActions(bool enable)
		{
			return Success(LoggingModifiedAllLogActions.Format(
				GetEnabled(enable).WithNoMarkdown()
			));
		}

		public static AdvobotResult ModifiedIgnoredLogChannels(
			IReadOnlyCollection<IGuildChannel> channels,
			bool ignored)
		{
			return Success(LoggingModifiedIgnoredLogChannels.Format(
				GetIgnored(ignored).WithNoMarkdown(),
				channels.Join(x => x.Format()).WithBlock()
			));
		}

		public static AdvobotResult ModifiedLogActions(
			IReadOnlyCollection<LogAction> logActions,
			bool enable)
		{
			return Success(LoggingModifiedLogActions.Format(
				GetEnabled(enable).WithNoMarkdown(),
				logActions.Join(x => x.ToString()).WithBlock()
			));
		}

		public static AdvobotResult Removed(string logType)
		{
			return Success(LoggingRemoved.Format(
				logType.WithNoMarkdown()
			));
		}

		public static AdvobotResult SetLog(string logType, ITextChannel channel)
		{
			return Success(LoggingSetLog.Format(
				channel.Format().WithBlock(),
				logType.WithNoMarkdown()
			));
		}

		private static string GetEnabled(bool value)
			=> value ? VariableEnabled : VariableDisabled;

		private static string GetIgnored(bool value)
			=> value ? VariableIgnored : VariableUnignored;
	}
}