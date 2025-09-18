using Advobot.Modules;

using Discord;

using YACCS.Commands.Attributes;
using YACCS.Preconditions;
using YACCS.Results;

namespace Advobot.ParameterPreconditions.Discord.Invites;

/// <summary>
/// Only allows invites from this guild.
/// </summary>
[AttributeUsage(AttributeUtils.PARAMETERS, AllowMultiple = false, Inherited = true)]
public sealed class FromThisGuild : AdvobotParameterPrecondition<IInviteMetadata>
{
	/// <inheritdoc />
	public override string Summary => "From this guild";

	/// <inheritdoc />
	protected override ValueTask<IResult> CheckNotNullAsync(
		CommandMeta meta,
		IGuildContext context,
		IInviteMetadata value)
	{
		if (context.Guild.Id == value.GuildId)
		{
			return new(Result.EmptySuccess);
		}
		return new(Result.Failure($"`{value.Id}` does not belong to this guild."));
	}
}