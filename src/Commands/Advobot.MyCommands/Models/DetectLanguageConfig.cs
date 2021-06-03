using System;

namespace Advobot.MyCommands.Models
{
	public record DetectLanguageConfig(
		string? APIKey,
		float ConfidenceLimit,
		long? CooldownStartTicks
	)
	{
		public DateTime? CooldownStart => CooldownStartTicks is long temp ? new(temp) : null;

		public DetectLanguageConfig()
			: this(null, 7.5f, null)
		{
		}
	}
}