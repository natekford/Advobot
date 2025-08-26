using Advobot.Modules;
using Advobot.Utilities;

using Discord;

using static Advobot.Resources.Responses;

namespace Advobot.Standard.Responses;

public sealed class Channels : AdvobotResult
{
	public static AdvobotResult ClearedOverwrites(IGuildChannel channel, int count)
	{
		return Success(ChannelsClearedOverwrites.Format(
			count.ToString().WithBlock(),
			channel.Format().WithBlock()
		));
	}

	public static AdvobotResult CopiedOverwrites(
		IGuildChannel input,
		IGuildChannel output,
		ISnowflakeEntity? obj,
		IReadOnlyCollection<Overwrite> overwrites)
	{
		if (overwrites.Count == 0)
		{
			return Success(ChannelsNoCopyableOverwrite);
		}

		return Success(ChannelsCopiedOverwrite.Format(
			(obj?.Format() ?? VariableAll).WithBlock(),
			input.Format().WithBlock(),
			output.Format().WithBlock()
		));
	}

	public static AdvobotResult MismatchType(
		IGuildChannel input,
		IGuildChannel output)
	{
		return Failure(ChannelsFailedPermissionCopy.Format(
			input.Format().WithBlock(),
			output.Format().WithBlock()
		));
	}
}