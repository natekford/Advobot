using System;
using System.Linq;
using System.Threading.Tasks;
using Advobot.Modules;
using Discord;

namespace Advobot.Attributes.Preconditions.Permissions
{
	/// <summary>
	/// Verifies the invoking user's permissions.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	public sealed class UserPermissionRequirementAttribute : PermissionRequirementAttribute
	{
		/// <inheritdoc />
		public override bool Visible => true;

		/// <summary>
		/// Creates an instance of <see cref="UserPermissionRequirementAttribute"/>.
		/// </summary>
		/// <param name="permissions"></param>
		public UserPermissionRequirementAttribute(params GuildPermission[] permissions) : base(permissions) { }

		/// <inheritdoc />
		public override Task<ulong> GetUserPermissionsAsync(IAdvobotCommandContext context)
		{
			var guildBits = context.User.GuildPermissions.RawValue;
			var botBits = context.Settings.BotUsers.FirstOrDefault(x => x.UserId == context.User.Id)?.Permissions ?? 0;
			return Task.FromResult(guildBits | botBits);
		}
	}
}
