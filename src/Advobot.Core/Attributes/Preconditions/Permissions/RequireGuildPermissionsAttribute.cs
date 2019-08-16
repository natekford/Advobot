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
	/// Verifies the invoking user's permissions on a guild.
	/// </summary>
	/// <remarks>
	/// Admin will always be added to the list of valid permissions.
	/// </remarks>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public class RequireGuildPermissionsAttribute : RequirePermissionsAttribute
	{
		/// <summary>
		/// Whether this precondition targets the bot rather than the user.
		/// </summary>
		public bool ForBot { get; set; }

		private static readonly Enum _Admin = GuildPermission.Administrator;

		/// <summary>
		/// Creates an instance of <see cref="RequireGuildPermissionsAttribute"/>.
		/// </summary>
		/// <param name="permissions"></param>
		public RequireGuildPermissionsAttribute(params GuildPermission[] permissions)
			: base(permissions.Cast<Enum>().Append(_Admin).ToArray()) { }

		/// <inheritdoc />
		public override async Task<Enum?> GetUserPermissionsAsync(
			ICommandContext context,
			IServiceProvider services)
		{
			if (ForBot)
			{
				var bot = await context.Guild.GetCurrentUserAsync().CAF();
				var bits = bot.GuildPermissions.RawValue;
				return bits == 0 ? null : (Enum)(GuildPermission)bits;
			}

			var guildUser = await context.Guild.GetUserAsync(context.User.Id).CAF();
			var guildBits = guildUser.GuildPermissions.RawValue;

			var settingsFactory = services.GetRequiredService<IGuildSettingsFactory>();
			var settings = await settingsFactory.GetOrCreateAsync(context.Guild).CAF();
			var match = settings.BotUsers.FirstOrDefault(x => x.UserId == context.User.Id);
			var botBits = match?.Permissions ?? 0;
			var allBits = guildBits | botBits;
			return allBits == 0 ? null : (Enum)(GuildPermission)allBits;
		}
	}
}
