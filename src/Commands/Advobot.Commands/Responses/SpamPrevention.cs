using Advobot.Modules;
using Advobot.Services.GuildSettings.Settings;
using Advobot.Utilities;

namespace Advobot.Commands.Responses
{
	public sealed class SpamPrevention : CommandResponses
	{
		private SpamPrevention() { }

		public static AdvobotResult CreatedSpamPrevention(SpamType type)
			=> Success(Default.FormatInterpolated($"Successfully created the {type} spam prevention."));
		public static AdvobotResult NoSpamPrevention(SpamType type)
			=> Success(Default.FormatInterpolated($"Failed to find a {type} spam prevention."));
		public static AdvobotResult ToggledSpamPrevention(SpamType type, bool enabled)
			=> Success(Default.FormatInterpolated($"Successfully {GetEnabled(enabled)} the {type} spam prevention."));
		public static AdvobotResult CreatedRaidPrevention(RaidType type)
			=> Success(Default.FormatInterpolated($"Successfully created the {type} raid prevention."));
		public static AdvobotResult NoRaidPrevention(RaidType type)
			=> Success(Default.FormatInterpolated($"Failed to find a {type} raid prevention."));
		public static AdvobotResult ToggledRaidPrevention(RaidType type, bool enabled)
			=> Success(Default.FormatInterpolated($"Successfully {GetEnabled(enabled)} the {type} raid prevention."));
	}
}
