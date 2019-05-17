using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Advobot.Classes;
using Advobot.Classes.Attributes;
using AdvorangesUtils;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Advobot.Utilities
{
	/// <summary>
	/// Actions done on discord objects.
	/// </summary>
	public static class DiscordUtils
	{
		/// <summary>
		/// Generates a default request options explaining who invoked the command for the audit log.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="reason"></param>
		/// <returns></returns>
		public static RequestOptions GenerateRequestOptions(this ICommandContext context, string? reason = null)
			=> context.User.GenerateRequestOptions(reason);
		/// <summary>
		/// Generates a default request options explaining who invoked the command for the audit log.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="reason"></param>
		/// <returns></returns>
		public static RequestOptions GenerateRequestOptions(this IUser user, string? reason = null)
		{
			var r = reason == null ? "" : $" Reason: {reason}.";
			return GenerateRequestOptions($"Action by {user.Format()}.{r}");
		}
		/// <summary>
		/// Returns request options, with <paramref name="reason"/> as the audit log reason.
		/// </summary>
		/// <param name="reason"></param>
		/// <returns></returns>
		public static RequestOptions GenerateRequestOptions(string? reason = null)
		{
			return new RequestOptions
			{
				AuditLogReason = reason,
				RetryMode = RetryMode.RetryRatelimit,
			};
		}
		/// <summary>
		/// Changes the role's position and says the supplied reason in the audit log.
		/// Not sure why, but IRole.ModifyAsync cannot set the position of a role to 1.
		/// </summary>
		/// <param name="role"></param>
		/// <param name="position"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		public static async Task<int> ModifyRolePositionAsync(SocketRole role, int position, RequestOptions options)
		{
			//Make sure it's put at the highest a bot can edit, so no permission exception
			var roles = role.Guild.Roles
				.Where(x => x.Id != role.Id && x.Position < role.Guild.CurrentUser.Hierarchy)
				.OrderBy(x => x.Position)
				.ToArray();
			position = Math.Max(1, Math.Min(position, roles.Length));

			var reorderProperties = new ReorderRoleProperties[roles.Length + 1];
			var newPosition = -1;
			for (var i = 0; i < reorderProperties.Length; ++i)
			{
				if (i > position)
				{
					reorderProperties[i] = new ReorderRoleProperties(roles[i - 1].Id, i);
				}
				else if (i < position)
				{
					reorderProperties[i] = new ReorderRoleProperties(roles[i].Id, i);
				}
				else
				{
					reorderProperties[i] = new ReorderRoleProperties(role.Id, i);
					newPosition = i;
				}
			}

			await role.Guild.ReorderRolesAsync(reorderProperties.Where(x => x != null), options).CAF();
			return newPosition;
		}
		/// <summary>
		/// Returns all the assemblies in the base directory which have the <see cref="CommandAssemblyAttribute"/>.
		/// This loads assemblies with a matching name so this can be a risk to use if bad files are in the folder.
		/// </summary>
		/// <returns></returns>
		public static IReadOnlyCollection<Assembly> GetCommandAssemblies()
		{
			var unloadedAssemblies = Directory.EnumerateFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll", SearchOption.TopDirectoryOnly)
				.Where(x => Path.GetFileName(x).CaseInsContains("Commands"))
				.Select(x => Assembly.LoadFrom(x));
			var assemblies = AppDomain.CurrentDomain.GetAssemblies().Concat(unloadedAssemblies)
				.Where(x => x.GetCustomAttribute<CommandAssemblyAttribute>() != null).ToArray();
			if (assemblies.Length > 0)
			{
				return assemblies;
			}
			throw new DllNotFoundException("Unable to find any command assemblies.");
		}
		/// <summary>
		/// Returns every user that has a non null join time in order from least to greatest.
		/// </summary>
		/// <param name="users"></param>
		/// <returns></returns>
		public static IReadOnlyCollection<T> OrderByJoinDate<T>(this IEnumerable<T> users) where T : IGuildUser
			=> users.Where(x => x.JoinedAt.HasValue).OrderBy(x => x.JoinedAt.GetValueOrDefault().Ticks).ToArray();
		/// <summary>
		/// Returns every user that can be modified by both <paramref name="invokingUser"/> and the bot.
		/// </summary>
		/// <param name="guild"></param>
		/// <param name="invokingUser"></param>
		/// <returns></returns>
		public static IReadOnlyCollection<SocketGuildUser> GetEditableUsers(this SocketGuild guild, SocketGuildUser invokingUser)
			=> guild.Users.Where(x => invokingUser.CanModify(x) && guild.CurrentUser.CanModify(x)).ToArray();
		/// <summary>
		/// If the bot can get invites then returns the invites otherwise returns null.
		/// </summary>
		/// <param name="guild"></param>
		/// <returns></returns>
		public static async Task<IReadOnlyCollection<IInviteMetadata>> SafeGetInvitesAsync(this IGuild guild)
		{
			var currentUser = await guild.GetCurrentUserAsync().CAF();
			if (currentUser.GuildPermissions.ManageGuild)
			{
				return await guild.GetInvitesAsync().CAF();
			}
			return Array.Empty<IInviteMetadata>();
		}
		/// <summary>
		/// Tries to find the invite a user joined on.
		/// </summary>
		/// <param name="invites"></param>
		/// <param name="user"></param>
		/// <returns></returns>
		public static async Task<CachedInvite?> GetInviteUserJoinedOnAsync(this IList<CachedInvite> invites, IGuildUser user)
		{
			//Bots join by being invited by admin, not through invites.
			if (user.IsBot)
			{
				return new CachedInvite("Bot invited by admin.", 0);
			}
			var current = await user.Guild.SafeGetInvitesAsync().CAF();
			//If the bot can't get invites then determining the correct invite is not possible with any accuracy
			if (current == null)
			{
				return null;
			}
			//No invites means vanity url, linked twitch, or something I don't 
			if (!current.Any())
			{
				return new CachedInvite("Single use invite, vanity url, or linked Twitch account.", 0);
			}
			//Find invites where the cached invite uses are not the same as the current ones.
			var updated = invites.Where(c => current.Any(x => c.Code == x.Code && c.Uses != x.Uses)).ToArray();
			//If only one then treat it as the joining invite
			if (updated.Length == 1)
			{
				var inv = updated[0];
				inv.IncrementUses();
				return inv;
			}
			//Get the new invites on the guild by finding which guild invites aren't on the bot invites list
			var cached = invites.Select(x => x.Code);
			var uncached = current.Where(x => !cached.Contains(x.Code)).ToArray();
			invites.AddRange(uncached.Select(x => new CachedInvite(x)));
			//If no new invites then assume it was the vanity url, linked twitch, or something I don't know
			if ((!uncached.Any() || uncached.All(x => x.Uses == 0)) && user.Guild.Features.CaseInsContains(Constants.VANITY_URL))
			{
				return new CachedInvite("Single use invite, vanity url, or linked Twitch account.", 0);
			}
			//If one then assume it's the new one, if more than one, no way to tell
			var firstUses = uncached.Where(x => x.Uses != 0).ToArray();
			if (firstUses.Length == 1)
			{
				var code = firstUses[0].Code;
				var inv = invites.Single(x => x.Code == code);
				inv.IncrementUses();
				return inv;
			}
			return null;
		}
	}
}
