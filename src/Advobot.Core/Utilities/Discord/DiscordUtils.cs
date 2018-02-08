using Advobot.Core.Classes;
using Advobot.Core.Classes.Results;
using Advobot.Core.Enums;
using Advobot.Core.Interfaces;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Advobot.Core.Utilities
{
	public static class DiscordUtils
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
			return InternalUtils.InternalVerify(target, context, checks, check =>
			{
				switch (check)
				{
					case ObjectVerification.IsNotEveryone:
						if (context.Guild.EveryoneRole.Id == target.Id)
						{
							return new VerifiedObjectResult(target, CommandError.UnmetPrecondition,
								"The everyone role cannot be modified in that way.");
						}
						return null;
					case ObjectVerification.IsManaged:
						if (!target.IsManaged)
						{
							return new VerifiedObjectResult(target, CommandError.UnmetPrecondition,
								"Managed roles cannot be modified in that way.");
						}
						return null;
				}
				return null;
			});
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
			var bot = role.Guild.CurrentUser;
			var roles = role.Guild.Roles
				.Where(x => x.Id != role.Id && x.Position < bot.Hierarchy)
				.OrderBy(x => x.Position)
				.ToArray();
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
		/// Verifies that the channel can be edited in specific ways.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="target"></param>
		/// <param name="checks"></param>
		/// <returns></returns>
		public static VerifiedObjectResult Verify(this IGuildChannel target, ICommandContext context, IEnumerable<ObjectVerification> checks)
		{
			return InternalUtils.InternalVerify(target, context, checks);
		}
		/// <summary>
		/// Gets the permission overwrite for a specific role or user, or null if one does not exist.
		/// </summary>
		/// <param name="channel"></param>
		/// <param name="obj"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentException"></exception>
		public static OverwritePermissions? GetPermissionOverwrite<T>(this IGuildChannel channel, T obj) where T : ISnowflakeEntity
		{
			switch (obj)
			{
				case IRole role:
					return channel.GetPermissionOverwrite(role);
				case IUser user:
					return channel.GetPermissionOverwrite(user);
				default:
					throw new ArgumentException("invalid type", nameof(obj));
			}
		}
		/// <summary>
		/// Sets the overwrite on a channel for the given object.
		/// </summary>
		/// <param name="channel"></param>
		/// <param name="obj"></param>
		/// <param name="allowBits"></param>
		/// <param name="denyBits"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentException"></exception>
		public static async Task AddPermissionOverwriteAsync<T>(this IGuildChannel channel, T obj, ulong allowBits, ulong denyBits, RequestOptions options) where T : ISnowflakeEntity
		{
			var permissions = new OverwritePermissions(allowBits, denyBits);
			switch (obj)
			{
				case IRole role:
					await channel.AddPermissionOverwriteAsync(role, permissions, options).CAF();
					return;
				case IUser user:
					await channel.AddPermissionOverwriteAsync(user, permissions, options).CAF();
					return;
				default:
					throw new ArgumentException("invalid type", nameof(obj));
			}
		}

		/// <summary>
		/// Verifies that the user can be edited in specific ways.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="target"></param>
		/// <param name="checks"></param>
		/// <returns></returns>
		public static VerifiedObjectResult Verify(this IGuildUser target, ICommandContext context, IEnumerable<ObjectVerification> checks)
		{
			return InternalUtils.InternalVerify(target, context, checks);
		}
		/// <summary>
		/// Returns true if the invoking user's position is greater than the target user's position.
		/// </summary>
		/// <param name="invoker"></param>
		/// <param name="target"></param>
		/// <returns></returns>
		public static bool HasHigherPosition(this IGuildUser invoker, IGuildUser target)
		{
			//User is the bot
			if (target.Id == invoker.Id && target.Id.ToString() == Config.Configuration[Config.ConfigDict.ConfigKey.BotId])
			{
				return true;
			}
			var invokerPosition = invoker is SocketGuildUser socketInvoker ? socketInvoker.Hierarchy : -1;
			var targetPosition = target is SocketGuildUser socketTarget ? socketTarget.Hierarchy : -1;
			return invokerPosition > targetPosition;
		}
		/// <summary>
		/// Returns true if the user can edit the user in the specified way.
		/// </summary>
		/// <param name="invoker"></param>
		/// <param name="target"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public static bool CanModify(this IGuildUser invoker, IGuildUser target, ObjectVerification type)
		{
			return InternalUtils.InternalCanModify(invoker, target, type);
		}
		/// <summary>
		/// Returns true if the user can edit the channel in the specified way.
		/// </summary>
		/// <param name="invoker"></param>
		/// <param name="target"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public static bool CanModify(this IGuildUser invoker, IGuildChannel target, ObjectVerification type)
		{
			return InternalUtils.InternalCanModify(invoker, target, type);
		}
		/// <summary>
		/// Returns true if the user can edit the role in the specified way.
		/// </summary>
		/// <param name="invoker"></param>
		/// <param name="target"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public static bool CanModify(this IGuildUser invoker, IRole target, ObjectVerification type)
		{
			return InternalUtils.InternalCanModify(invoker, target, type);
		}

		/// <summary>
		/// Returns every user that has a non null join time in order from least to greatest.
		/// </summary>
		/// <param name="guild"></param>
		/// <returns></returns>
		public static IEnumerable<SocketGuildUser> GetUsersByJoinDate(this SocketGuild guild)
		{
			return guild.Users.OrderBy(x => x.JoinedAt?.Ticks ?? 0);
		}
		/// <summary>
		/// Returns every user that can be modified by both <paramref name="invokingUser"/> and the bot.
		/// </summary>
		/// <param name="guild"></param>
		/// <param name="invokingUser"></param>
		/// <returns></returns>
		public static IEnumerable<SocketGuildUser> GetEditableUsers(this SocketGuild guild, SocketGuildUser invokingUser)
		{
			return guild.Users.Where(x => invokingUser.HasHigherPosition(x) && guild.CurrentUser.HasHigherPosition(x));
		}

		/// <summary>
		/// Checks if the bot can get invites before trying to get invites.
		/// </summary>
		/// <param name="guild"></param>
		/// <returns></returns>
		public static async Task<IEnumerable<RestInviteMetadata>> GetInvitesAsync(SocketGuild guild)
		{
			return guild.CurrentUser.GuildPermissions.ManageGuild ? await guild.GetInvitesAsync().CAF() : new List<RestInviteMetadata>();
		}
		/// <summary>
		/// Tries to find the invite a user joined on.
		/// </summary>
		/// <param name="guildSettings"></param>
		/// <param name="user"></param>
		/// <returns></returns>
		public static async Task<CachedInvite> GetInviteUserJoinedOnAsync(IGuildSettings guildSettings, SocketGuildUser user)
		{
			//Bots join by being invited by admin, not through invites.
			if (user.IsBot)
			{
				return new CachedInvite("Invited by admin", 0);
			}
			var currentInvites = (await GetInvitesAsync(user.Guild).CAF()).ToList();
			if (!currentInvites.Any())
			{
				return user.Guild.Features.CaseInsContains(Constants.VANITY_URL) ? new CachedInvite("Vanity Url", 0) : null;
			}

			//Find invites where the cached invite uses are not the same as the current ones.
			var updatedInvites = guildSettings.Invites.Where(cached =>
			{
				return currentInvites.Any(current => cached.Code == current.Code && cached.Uses != current.Uses);
			}).ToList();
			//If only one then treat it as the joining invite
			CachedInvite joinInv;
			if (updatedInvites.Count() == 1)
			{
				joinInv = updatedInvites.First();
				joinInv.IncrementUses();
				return joinInv;
			}

			//Get the new invites on the guild by finding which guild invites aren't on the bot invites list
			var newInvs = currentInvites.Where(current =>
			{
				return !guildSettings.Invites.Select(cached => cached.Code).Contains(current.Code);
			}).ToList();
			//If no new invites then assume it was the vanity url
			if ((!newInvs.Any() || newInvs.All(x => x.Uses == 0)) && user.Guild.Features.CaseInsContains(Constants.VANITY_URL))
			{
				joinInv = new CachedInvite("Vanity Url", 0);
			}
			//If one then assume it's the new one
			else if (newInvs.Count(x => x.Uses != 0) == 1)
			{
				var invite = newInvs.First(x => x.Uses != 0);
				joinInv = new CachedInvite(invite);
			}
			//No way to tell if more than one
			guildSettings.Invites.AddRange(newInvs.Select(x => new CachedInvite(x)));
			return null;
		}
	}
}
