using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Advobot.Utilities;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Advobot.Attributes.ParameterPreconditions.DiscordObjectValidation.Channels
{
	/// <summary>
	/// Base for validating any <see cref="SocketGuildChannel"/>.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public class ChannelAttribute : DiscordObjectParameterPreconditionAttribute
	{
		/// <summary>
		/// The permissions to make sure the invoking user has on the channel.
		/// </summary>
		public ImmutableHashSet<ChannelPermission> Permissions { get; }

		private readonly string _PermissionsText;

		/// <summary>
		/// Creates an instance of <see cref="ChannelAttribute"/>.
		/// </summary>
		/// <param name="permissions"></param>
		public ChannelAttribute(params ChannelPermission[] permissions)
		{
			Permissions = permissions
				.Append(ChannelPermission.ViewChannel)
				.ToImmutableHashSet();

			_PermissionsText = Permissions.FormatPermissions();
		}

		/// <inheritdoc />
		protected override Task<PreconditionResult> SingularCheckPermissionsAsync(
			ICommandContext context,
			ParameterInfo parameter,
			ISnowflakeEntity value,
			IServiceProvider services)
		{
			if (!(context.User is IGuildUser invoker))
			{
				return this.FromErrorAsync("Invalid invoker.");
			}
			if (!(value is IGuildChannel channel))
			{
				return this.FromErrorAsync("Invalid channel.");
			}
			return invoker.ValidateChannel(channel, Permissions, GetPreconditions());
		}
		/// <summary>
		/// Extra checks to use in validation.
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerable<Precondition<IGuildChannel>> GetPreconditions()
			=> Array.Empty<Precondition<IGuildChannel>>();
		/// <inheritdoc />
		public override string ToString()
			=> _PermissionsText;
	}
}