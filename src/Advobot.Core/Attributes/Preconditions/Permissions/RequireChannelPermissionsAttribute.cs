using System;
using System.Linq;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;

namespace Advobot.Attributes.Preconditions.Permissions
{
	/// <summary>
	/// Verifies the invoking user's permissions on the context channel.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public class RequireChannelPermissionsAttribute : RequirePermissionsAttribute
	{
		/// <summary>
		/// Creates an instance of <see cref="RequireGuildPermissionsAttribute"/>.
		/// </summary>
		/// <param name="permissions"></param>
		public RequireChannelPermissionsAttribute(params ChannelPermission[] permissions)
			: base(permissions.Cast<Enum>()) { }

		/// <inheritdoc />
		public override Task<Enum?> GetUserPermissionsAsync(
			ICommandContext context,
			IGuildUser user,
			IServiceProvider services)
		{
			if (!(context.Channel is ITextChannel channel))
			{
				return Task.FromResult<Enum?>(null);
			}

			var bits = user.GetPermissions(channel).RawValue;
			var e = bits == 0 ? null : (Enum)(ChannelPermission)bits;
			return Task.FromResult(e);
		}
	}
}