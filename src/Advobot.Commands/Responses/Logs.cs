using Advobot.Classes;
using Advobot.Classes.Results;
using Advobot.Enums;
using Advobot.Utilities;
using Discord;
using System;
using System.Collections.Generic;

namespace Advobot.Commands.Responses
{
	public sealed class Logs : CommandResponses
	{
		private Logs() { }

		public static AdvobotResult ModifiedIgnoredLogChannels(IReadOnlyCollection<IGuildChannel> channels, bool ignored)
			=> Success(Default.FormatInterpolated($"Successfully {GetIgnored(ignored)} the channels {channels}."));
		public static AdvobotResult ShowLogActions()
		{
			return Success(new EmbedWrapper
			{
				Title = "Log Actions",
				Description = Default.FormatInterpolated($"{Enum.GetNames(typeof(LogAction))}"),
			});
		}
		public static AdvobotResult DefaultLogActions()
			=> Success("Successfully set the log actions to the default ones.");
		public static AdvobotResult ModifiedAllLogActions(bool enable)
			=> Success(Default.FormatInterpolated($"Successfully {GetEnabled(enable)} every log action."));
		public static AdvobotResult ModifiedLogActions(IReadOnlyCollection<LogAction> logActions, bool enable)
			=> Success(Default.FormatInterpolated($"Successfully {GetEnabled(enable)} the following log actions: {logActions}."));
	}
}
