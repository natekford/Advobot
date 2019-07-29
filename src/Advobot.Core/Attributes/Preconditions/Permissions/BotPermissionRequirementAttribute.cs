using System;
using Advobot.Classes.Modules;
using Discord;

namespace Advobot.Classes.Attributes.Preconditions.Permissions
{
	/// <summary>
	/// Verifies the bot's permissions.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	public sealed class BotPermissionRequirementAttribute : PermissionRequirementAttribute
	{
		/// <inheritdoc />
		public override bool Visible => false;

		/// <summary>
		/// Creates an instance of <see cref="BotPermissionRequirementAttribute"/>.
		/// </summary>
		/// <param name="permissions"></param>
		public BotPermissionRequirementAttribute(params GuildPermission[] permissions) : base(permissions) { }

		/// <inheritdoc />
		public override ulong GetUserPermissions(AdvobotCommandContext context)
			=> context.Guild.CurrentUser.GuildPermissions.RawValue;
	}
}
