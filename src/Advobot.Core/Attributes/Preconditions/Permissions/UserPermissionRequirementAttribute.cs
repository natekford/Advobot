using System;
using System.Linq;
using System.Threading.Tasks;
using Advobot.Services.GuildSettings;
using AdvorangesUtils;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Attributes.Preconditions.Permissions
{
	/// <summary>
	/// Verifies the invoking user's permissions.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	public sealed class GuildPermissionRequirementAttribute : PermissionRequirementAttribute
	{
		/// <summary>
		/// Creates an instance of <see cref="GuildPermissionRequirementAttribute"/>.
		/// </summary>
		/// <param name="permissions"></param>
		public GuildPermissionRequirementAttribute(params GuildPermission[] permissions)
			: base(permissions) { }

		/// <inheritdoc />
		public override async Task<ulong> GetUserPermissionsAsync(
			ICommandContext context,
			IServiceProvider services)
		{
			var guildUser = await context.Guild.GetUserAsync(context.User.Id).CAF();
			var guildBits = guildUser.GuildPermissions.RawValue;

			var settingsFactory = services.GetRequiredService<IGuildSettingsFactory>();
			var settings = await settingsFactory.GetOrCreateAsync(context.Guild).CAF();
			var match = settings.BotUsers.FirstOrDefault(x => x.UserId == context.User.Id);
			var botBits = match?.Permissions ?? 0;
			return guildBits | botBits;
		}
	}
}
