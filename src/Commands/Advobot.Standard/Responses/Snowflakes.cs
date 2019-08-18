using Advobot.Modules;
using Advobot.Utilities;
using Discord;
using static Advobot.Standard.Resources.Responses;

namespace Advobot.Standard.Responses
{
	public sealed class Snowflakes : CommandResponses
	{
		private Snowflakes() { }

		public static AdvobotResult ModifiedName(ISnowflakeEntity snowflake, string name)
			=> Success(Default.Format(SnowflakesModifiedName, snowflake, name));
		public static AdvobotResult Created(ISnowflakeEntity snowflake)
			=> Success(Default.Format(SnowflakesCreated, snowflake));
		public static AdvobotResult Deleted(ISnowflakeEntity snowflake)
			=> Success(Default.Format(SnowflakesDeleted, snowflake));
		public static AdvobotResult SoftDeleted(ISnowflakeEntity snowflake)
			=> Success(Default.Format(SnowflakesSoftDeleted, snowflake));
		public static AdvobotResult EnqueuedIcon(ISnowflakeEntity snowflake, int position)
			=> Success(Default.Format(SnowflakesEnqueuedIcon, snowflake, position));
		public static AdvobotResult RemovedIcon(ISnowflakeEntity snowflake)
			=> Success(Default.Format(SnowflakesRemovedIcon, snowflake));
	}
}
