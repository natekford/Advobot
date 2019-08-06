using System;
using System.Linq;
using System.Threading.Tasks;
using AdvorangesUtils;
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
			: base(permissions.Cast<Enum>().ToArray()) { }

		/// <inheritdoc />
		public override async Task<Enum?> GetUserPermissionsAsync(
			ICommandContext context,
			IServiceProvider services)
		{
			var guildChannel = await context.Guild.GetTextChannelAsync(context.Channel.Id).CAF();
			var guildUser = await context.Guild.GetUserAsync(context.User.Id).CAF();
			var channelBits = guildUser.GetPermissions(guildChannel).RawValue;
			return channelBits == 0 ? null : (Enum)(ChannelPermission)channelBits;
		}
	}
}
