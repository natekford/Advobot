using Advobot.Modules;
using Advobot.Utilities;

using Discord;

using System.Collections.Immutable;

using YACCS.Preconditions;
using YACCS.Results;

namespace Advobot.ParameterPreconditions.Discord.Channels;

/// <summary>
/// Validates the passed in <see cref="IGuildChannel"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
public sealed class CanModifyChannel(params ChannelPermission[] permissions) : AdvobotParameterPrecondition<IGuildChannel>
{
	/// <summary>
	/// The permissions to make sure the invoking user has on the channel.
	/// </summary>
	public ImmutableHashSet<ChannelPermission> Permissions { get; } = [.. permissions
		.Select(x => x | ChannelPermission.ViewChannel)
		.DefaultIfEmpty(ChannelPermission.ViewChannel)
	];

	/// <inheritdoc />
	public override string Summary => "Can be modified by the bot and invoking user";

	/// <inheritdoc />
	public override ValueTask<IResult> CheckAsync(
		CommandMeta meta,
		IGuildContext context,
		IGuildChannel? value)
		=> new(context.User.ValidateChannel(value, Permissions));
}