using Advobot.Modules;
using Advobot.Utilities;

namespace Advobot.Settings.Responses
{
	public sealed class GuildSettings : CommandResponses
	{
		private GuildSettings()
		{
		}

		public static AdvobotResult Reset(string name)
			=> Success(Default.FormatInterpolated($"Successfully reset the setting {name}"));

		public static AdvobotResult ResetAll()
			=> Success("Successfully reset all settings.");
	}
}