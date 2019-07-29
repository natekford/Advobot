using Advobot.Modules;
using Advobot.Utilities;
using Discord;

namespace Advobot.Commands.Responses
{
	public sealed class Snowflakes : CommandResponses
	{
		private Snowflakes() { }

		public static AdvobotResult ModifiedName(ISnowflakeEntity snowflake, string name)
			=> Success(Default.FormatInterpolated($"Successfully changed the name of {snowflake} to {name}."));
		public static AdvobotResult Created(ISnowflakeEntity snowflake)
			=> Success(Default.FormatInterpolated($"Successfully created {snowflake}."));
		public static AdvobotResult Deleted(ISnowflakeEntity snowflake)
			=> Success(Default.FormatInterpolated($"Successfully deleted {snowflake}."));
		public static AdvobotResult SoftDeleted(ISnowflakeEntity snowflake)
			=> Success(Default.FormatInterpolated($"Successfully soft deleted {snowflake}."));
		public static AdvobotResult EnqueuedIcon(ISnowflakeEntity snowflake, int position)
			=> Success(Default.FormatInterpolated($"Successfully queued changing the icon for {snowflake} at position {position}."));
		public static AdvobotResult RemovedIcon(ISnowflakeEntity snowflake)
			=> Success(Default.FormatInterpolated($"Successfully removed the icon for {snowflake}."));
	}
}
