using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

using Advobot.Utilities;

using Discord;
using Discord.Commands;

namespace Advobot.Attributes.ParameterPreconditions.DiscordObjectValidation.Channels
{
	/// <summary>
	/// Validates the passed in <see cref="IGuildChannel"/>.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public sealed class CanModifyChannelAttribute : ChannelParameterPreconditionAttribute
	{
		/// <summary>
		/// The permissions to make sure the invoking user has on the channel.
		/// </summary>
		public ImmutableHashSet<ChannelPermission> Permissions { get; }

		/// <inheritdoc />
		public override string Summary { get; }

		/// <summary>
		/// Creates an instance of <see cref="CanModifyChannelAttribute"/>.
		/// </summary>
		/// <param name="permissions"></param>
		public CanModifyChannelAttribute(params ChannelPermission[] permissions)
		{
			Permissions = permissions
				.Select(x => x | ChannelPermission.ViewChannel)
				.DefaultIfEmpty(ChannelPermission.ViewChannel)
				.ToImmutableHashSet();
			Summary = Permissions.FormatPermissions();
		}

		/// <inheritdoc />
		protected override Task<PreconditionResult> SingularCheckChannelAsync(
			ICommandContext context,
			ParameterInfo parameter,
			IGuildUser invoker,
			IGuildChannel channel,
			IServiceProvider services)
			=> invoker.ValidateChannel(channel, Permissions);
	}
}