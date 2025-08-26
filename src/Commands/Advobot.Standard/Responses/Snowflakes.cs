using Advobot.Modules;
using Advobot.Utilities;

using Discord;

using static Advobot.Resources.Responses;

namespace Advobot.Standard.Responses;

public sealed class Snowflakes : AdvobotResult
{
	public static AdvobotResult Created<T>(IEntity<T> snowflake)
		where T : IEquatable<T>
	{
		return Success(SnowflakesCreated.Format(
			snowflake.Format().WithBlock()
		));
	}

	public static AdvobotResult Deleted<T>(IEntity<T> snowflake)
		where T : IEquatable<T>
	{
		return Success(SnowflakesDeleted.Format(
			snowflake.Format().WithBlock()
		));
	}

	public static AdvobotResult ModifiedName<T>(IEntity<T> snowflake, string name)
		where T : IEquatable<T>
	{
		return Success(SnowflakesModifiedName.Format(
			snowflake.Format().WithBlock(),
			name.WithBlock()
		));
	}

	public static AdvobotResult SoftDeleted<T>(IEntity<T> snowflake)
		where T : IEquatable<T>
	{
		return Success(SnowflakesSoftDeleted.Format(
			snowflake.Format().WithBlock()
		));
	}
}