using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Advobot.Core.Classes;
using Advobot.Core.Classes.Results;
using Advobot.Core.Enums;
using Advobot.Core.Interfaces;
using Advobot.Core.Utilities.Formatting;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Advobot.Core.Utilities
{
	/// <summary>
	/// Actions which are done on an <see cref="IRole"/>.
	/// </summary>
	public static class RoleUtils
	{
		/// <summary>
		/// Verifies that the role can be edited in specific ways.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="target"></param>
		/// <param name="checks"></param>
		/// <returns></returns>
		public static VerifiedObjectResult Verify(this IRole target, ICommandContext context, IEnumerable<ObjectVerification> checks)
		{
			if (target == null)
			{
				return new VerifiedObjectResult(target, CommandError.ObjectNotFound, "Unable to find a matching role.");
			}
			if (!(context.User is SocketGuildUser invokingUser && context.Guild.GetBot() is SocketGuildUser bot))
			{
				return new VerifiedObjectResult(target, CommandError.Unsuccessful, "Invalid invoking user or guild or bot.");
			}

			foreach (var check in checks)
			{
				if (!invokingUser.CanModify(target, check))
				{
					return new VerifiedObjectResult(target, CommandError.UnmetPrecondition,
						$"You are unable to make the given changes to the role: `{target.Format()}`.");
				}
				if (!bot.CanModify(target, check))
				{
					return new VerifiedObjectResult(target, CommandError.UnmetPrecondition,
						$"I am unable to make the given changes to the role: `{target.Format()}`.");
				}

				switch (check)
				{
					case ObjectVerification.IsNotEveryone:
						if (context.Guild.EveryoneRole.Id != target.Id)
						{
							return new VerifiedObjectResult(target, CommandError.UnmetPrecondition,
								"The everyone role cannot be modified in that way.");
						}
						break;
					case ObjectVerification.IsManaged:
						if (!target.IsManaged)
						{
							return new VerifiedObjectResult(target, CommandError.UnmetPrecondition,
								"Managed roles cannot be modified in that way.");
						}
						break;
				}
			}

			return new VerifiedObjectResult(target, null, null);
		}
		/// <summary>
		/// Creates a role then says the supplied reason in the audit log.
		/// </summary>
		/// <param name="guild"></param>
		/// <param name="name"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		public static async Task<IRole> CreateRoleAsync(IGuild guild, string name, RequestOptions options)
		{
			return await guild?.CreateRoleAsync(name, new GuildPermissions(0), options: options).CAF();
		}
		/// <summary>
		/// Deletes a a role then says the supplied reason in the audit log.
		/// </summary>
		/// <param name="role"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		public static async Task DeleteRoleAsync(IRole role, RequestOptions options)
		{
			await role?.DeleteAsync(options).CAF();
		}
		/// <summary>
		/// Gives the roles to a user.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="roles"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		public static async Task GiveRolesAsync(IGuildUser user, IEnumerable<IRole> roles, RequestOptions options)
		{
			await user?.AddRolesAsync(roles, options).CAF();
		}
		/// <summary>
		/// Removes the roles from a user.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="roles"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		public static async Task TakeRolesAsync(IGuildUser user, IEnumerable<IRole> roles, RequestOptions options)
		{
			await user?.RemoveRolesAsync(roles, options).CAF();
		}
		/// <summary>
		/// Changes the role's position and says the supplied reason in the audit log.
		/// </summary>
		/// <param name="role"></param>
		/// <param name="position"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		public static async Task<int> ModifyRolePositionAsync(IRole role, int position, RequestOptions options)
		{
			if (!(role != null && role.Guild is SocketGuild guild && guild.GetBot() is SocketGuildUser bot))
			{
				return -1;
			}

			var roles = role.Guild.Roles.Where(x => x.Id != role.Id && x.Position < bot.Hierarchy)
				.OrderBy(x => x.Position).ToArray();
			position = Math.Max(1, Math.Min(position, roles.Length));

			var reorderProperties = new ReorderRoleProperties[roles.Length + 1];
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
				}
			}

			await role.Guild.ReorderRolesAsync(reorderProperties, options).CAF();
			return reorderProperties.FirstOrDefault(x => x.Id == role.Id)?.Position ?? -1;
		}
		/// <summary>
		/// Changes the role's permissions and says the supplied reason in the audit log.
		/// </summary>
		/// <param name="role"></param>
		/// <param name="permissions"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		public static async Task ModifyRolePermissionsAsync(IRole role, ulong permissions, RequestOptions options)
		{
			await role?.ModifyAsync(x => x.Permissions = new GuildPermissions(permissions), options).CAF();
		}
		/// <summary>
		/// Changes the role's name and says the supplied reason in the audit log.
		/// </summary>
		/// <param name="role"></param>
		/// <param name="name"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		public static async Task ModifyRoleNameAsync(IRole role, string name, RequestOptions options)
		{
			await role?.ModifyAsync(x => x.Name = name, options).CAF();
		}
	}
}