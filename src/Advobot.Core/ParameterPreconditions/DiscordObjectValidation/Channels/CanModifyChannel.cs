using Advobot.Utilities;

using Discord;
using Discord.Commands;

using System.Collections.Immutable;

namespace Advobot.ParameterPreconditions.DiscordObjectValidation.Channels;

/// <summary>
/// Validates the passed in <see cref="IGuildChannel"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
public sealed class CanModifyChannel : AdvobotParameterPrecondition<IGuildChannel>
{
	/// <summary>
	/// The permissions to make sure the invoking user has on the channel.
	/// </summary>
	public ImmutableHashSet<ChannelPermission> Permissions { get; }

	/// <inheritdoc />
	public override string Summary => "Can be modified by the bot and invoking user";

	/// <summary>
	/// Creates an instance of <see cref="CanModifyChannel"/>.
	/// </summary>
	/// <param name="permissions"></param>
	public CanModifyChannel(params ChannelPermission[] permissions)
	{
		Permissions = permissions
			.Select(x => x | ChannelPermission.ViewChannel)
			.DefaultIfEmpty(ChannelPermission.ViewChannel)
			.ToImmutableHashSet();
	}

	/// <inheritdoc />
	protected override Task<PreconditionResult> CheckPermissionsAsync(
		ICommandContext context,
		ParameterInfo parameter,
		IGuildUser invoker,
		IGuildChannel channel,
		IServiceProvider services)
		=> invoker.ValidateChannel(channel, Permissions);
}