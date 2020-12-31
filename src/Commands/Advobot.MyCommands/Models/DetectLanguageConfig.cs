using System;

namespace Advobot.MyCommands.Models
{
	public record DetectLanguageConfig(
		string? APIKey,
		float ConfidenceLimit,
		long? CooldownStartTicks
	)
	{
		public DateTime? CooldownStart
			=> CooldownStartTicks is null ? null : new DateTime(CooldownStartTicks.Value);

		public DetectLanguageConfig()
			: this(null, 7.5f, null)
		{
		}
	}
}