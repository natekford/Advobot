using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Advobot.Classes;
using Advobot.Classes.Attributes;
using Advobot.Classes.Results;
using Advobot.Enums;
using Advobot.Interfaces;
using AdvorangesUtils;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;

namespace Advobot.Utilities
{
	/// <summary>
	/// Actions done on discord objects.
	/// </summary>
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
			return InternalUtils.InternalVerify(target, context, checks, "role", check =>
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
					case ObjectVerification.IsNotManaged:
						if (target.IsManaged)
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
		/// Verifies that the channel can be edited in specific ways.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="target"></param>
		/// <param name="checks"></param>
		/// <returns></returns>
		public static VerifiedObjectResult Verify(this IGuildChannel target, ICommandContext context, IEnumerable<ObjectVerification> checks)
		{
			return InternalUtils.InternalVerify(target, context, checks, "channel");
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
			return InternalUtils.InternalVerify(target, context, checks, "user");
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
					throw new ArgumentException("Invalid type supplied for permission overwrites.", nameof(obj));
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
			switch (obj)
			{
				case IRole role:
					await channel.AddPermissionOverwriteAsync(role, new OverwritePermissions(allowBits, denyBits), options).CAF();
					return;
				case IUser user:
					await channel.AddPermissionOverwriteAsync(user, new OverwritePermissions(allowBits, denyBits), options).CAF();
					return;
				default:
					throw new ArgumentException("Invalid type supplied for permission overwrites.", nameof(obj));
			}
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
			if (target.Id == invoker.Id && target.Id == LowLevelConfig.Config.BotId)
			{
				return true;
			}
			var invokerPosition = invoker is SocketGuildUser socketInvoker ? socketInvoker.Hierarchy : -1;
			var targetPosition = target is SocketGuildUser socketTarget ? socketTarget.Hierarchy : -1;
			return invokerPosition > targetPosition;
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
		/// Counts how many times something has occurred within a given timeframe.
		/// Also modifies the queue by removing instances which are too old to matter (locks the source when doing so).
		/// Returns the listlength if seconds is less than 2 or the listlength is less than 2.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="seconds"></param>
		/// <param name="removeOldInstances"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentException">When <paramref name="source"/> is not in order.</exception>
		public static int CountItemsInTimeFrame(List<ulong> source, int seconds = 0, bool removeOldInstances = false)
		{
			var timeList = new List<ulong>(source);
			//No timeFrame given means that it's a spam prevention that doesn't check against time, like longmessage or mentions
			var listLength = timeList.Count;
			if (seconds < 2 || listLength < 2)
			{
				return listLength;
			}

			//If there is a timeFrame then that means to gather the highest amount of messages that are in the time frame
			var maxCount = 0;
			for (var i = 0; i < listLength; ++i)
			{
				//If the queue is out of order that kinda ruins the method
				if (i > 0 && timeList[i - 1] > timeList[i])
				{
					throw new ArgumentException("The queue must be in order from oldest to newest.", nameof(source));
				}

				var currentIterCount = 1;
				var iTime = SnowflakeUtils.FromSnowflake(timeList[i]).UtcDateTime;
				for (var j = i + 1; j < listLength; ++j)
				{
					var jTime = SnowflakeUtils.FromSnowflake(timeList[j]).UtcDateTime;
					if ((int)(jTime - iTime).TotalSeconds < seconds)
					{
						++currentIterCount;
						continue;
					}
					//Optimization by checking if the time difference between two numbers is too high to bother starting at j - 1
					var jMinOneTime = SnowflakeUtils.FromSnowflake(timeList[j - 1]).UtcDateTime;
					if ((int)(jTime - jMinOneTime).TotalSeconds > seconds)
					{
						i = j + 1;
					}
					break;
				}
				maxCount = Math.Max(maxCount, currentIterCount);
			}

			if (removeOldInstances)
			{
				lock (source)
				{
					//Work the way down
					var now = DateTime.UtcNow;
					for (int i = listLength - 1; i >= 0; --i)
					{
						//if the time is recent enough to still be within the timeframe leave it
						if ((int)(now - SnowflakeUtils.FromSnowflake(timeList[i]).UtcDateTime).TotalSeconds < seconds + 1)
						{
							continue;
						}
						//The first object now found within the timeframe is where objects will be removed up to
						for (int j = 0; j < i; ++j)
						{
							source.Remove(timeList[j]);
						}
						break;
					}
				}
			}
			return maxCount;
		}

		/// <summary>
		/// Returns all the assemblies in the base directory which have the <see cref="CommandAssemblyAttribute"/>.
		/// This loads assemblies with a matching name so this can be a risk to use if bad files are in the folder.
		/// </summary>
		/// <returns></returns>
		public static IEnumerable<Assembly> GetCommandAssemblies()
		{
			var unloadedAssemblies = Directory.EnumerateFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll", SearchOption.TopDirectoryOnly)
				.Where(x => Path.GetFileName(x).CaseInsContains("Commands"))
				.Select(x => Assembly.LoadFrom(x));
			var assemblies = AppDomain.CurrentDomain.GetAssemblies().Concat(unloadedAssemblies)
				.Where(x => x.GetCustomAttribute<CommandAssemblyAttribute>() != null).ToList();
			if (assemblies.Any())
			{
				return assemblies;
			}

			ConsoleUtils.WriteLine("Unable to find any command assemblies.", ConsoleColor.Red);
			Console.Read();
			throw new DllNotFoundException("Unable to find any command assemblies.");
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
			return guild.CurrentUser.GuildPermissions.ManageGuild
				? await guild.GetInvitesAsync().CAF()
				: Enumerable.Empty<RestInviteMetadata>();
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
				return new CachedInvite("Invited by admin.", 0);
			}
			//No invites means vanity url, linked twitch, or something I don't know
			var curInvs = (await GetInvitesAsync(user.Guild).CAF()).ToList();
			if (!curInvs.Any())
			{
				return user.Guild.Features.CaseInsContains(Constants.VANITY_URL) ? new CachedInvite("Vanity url or linked twitch.", 0) : null;
			}
			//Find invites where the cached invite uses are not the same as the current ones.
			var updatedInvs = guildSettings.Invites.Where(cac => curInvs.Any(cur => cac.Code == cur.Code && cac.Uses != cur.Uses)).ToList();
			//If only one then treat it as the joining invite
			if (updatedInvs.Count == 1)
			{
				var inv = updatedInvs.First();
				inv.IncrementUses();
				return inv;
			}
			//Get the new invites on the guild by finding which guild invites aren't on the bot invites list
			var newInvs = curInvs.Where(cur => !guildSettings.Invites.Select(cac => cac.Code).Contains(cur.Code)).ToList();
			guildSettings.Invites.AddRange(newInvs.Select(x => new CachedInvite(x)));
			//If no new invites then assume it was the vanity url, linked twitch, or something I don't know
			if ((!newInvs.Any() || newInvs.All(x => x.Uses == 0)) && user.Guild.Features.CaseInsContains(Constants.VANITY_URL))
			{
				return new CachedInvite("Vanity url or linked twitch.", 0);
			}
			//If one then assume it's the new one, if more than one, no way to tell
			return newInvs.Count(x => x.Uses != 0) == 1
				? guildSettings.Invites.First(i => i.Code == newInvs.First(n => n.Uses != 0).Code)
				: null;
		}
		/// <summary>
		/// Returns objects where the function does not return null and is either equal to, less than, or greater than a specified number.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="objects"></param>
		/// <param name="target"></param>
		/// <param name="count"></param>
		/// <param name="f"></param>
		/// <returns></returns>
		public static IEnumerable<T> GetInvitesFromCount<T>(this IEnumerable<T> objects, CountTarget target, uint count, Func<T, uint> f)
		{
			switch (target)
			{
				case CountTarget.Equal:
					objects = objects.Where(x => f(x) == count);
					break;
				case CountTarget.Below:
					objects = objects.Where(x => f(x) < count);
					break;
				case CountTarget.Above:
					objects = objects.Where(x => f(x) > count);
					break;
			}
			return objects;
		}
	}
}
