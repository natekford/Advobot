using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Advobot.Classes.Attributes;
using AdvorangesUtils;
using Discord;
using Discord.Commands;

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
		public static async Task<int> ModifyRolePositionAsync(IRole role, int position, RequestOptions options)
		{
			//Make sure it's put at the highest a bot can edit, so no permission exception
			var bot = await role.Guild.GetCurrentUserAsync().CAF();
			var roles = role.Guild.Roles
				.Where(x => x.Id != role.Id && bot.CanModify(x))
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

			await role.Guild.ReorderRolesAsync(reorderProperties, options).CAF();
			return newPosition;
		}
		/// <summary>
		/// Returns every user that has a non null join time in order from least to greatest.
		/// </summary>
		/// <param name="users"></param>
		/// <returns></returns>
		public static IReadOnlyCollection<T> OrderByJoinDate<T>(this IEnumerable<T> users) where T : IGuildUser
			=> users.Where(x => x.JoinedAt.HasValue).OrderBy(x => x.JoinedAt.GetValueOrDefault().Ticks).ToArray();
	}
}
