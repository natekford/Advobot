using System;
using System.Threading.Tasks;
using AdvorangesUtils;
using Discord;
using Discord.Commands;

namespace Advobot.Attributes.Preconditions.Permissions
{
	/// <summary>
	/// Verifies the bot's permissions.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	public sealed class BotPermissionRequirementAttribute : PermissionRequirementAttribute
	{
		/// <summary>
		/// Creates an instance of <see cref="BotPermissionRequirementAttribute"/>.
		/// </summary>
		/// <param name="permissions"></param>
		public BotPermissionRequirementAttribute(params GuildPermission[] permissions)
			: base(permissions) { }

		/// <inheritdoc />
		public override async Task<ulong> GetUserPermissionsAsync(
			ICommandContext context,
			IServiceProvider services)
		{
			var bot = await context.Guild.GetCurrentUserAsync().CAF();
			return bot.GuildPermissions.RawValue;
		}
	}
}
