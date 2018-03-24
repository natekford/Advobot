using AdvorangesUtils;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Advobot.Core.Classes.Attributes
{
	/// <summary>
	/// Checks if the user has all of the permissions supplied for allOfTheListedPerms or if the user has any of the permissions supplied for anyOfTheListedPerms.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public class PermissionRequirementAttribute : PreconditionAttribute
	{
		private GuildPermissions _AllFlags;
		private GuildPermissions _AnyFlags;

		/// <summary>
		/// Returns the names of the flags where all are needed.
		/// </summary>
		public string AllText => String.Join(" & ", _AllFlags.ToList().Select(x => x.ToString()));
		/// <summary>
		/// Returns the names of the flags where any are needed.
		/// </summary>
		public string AnyText => String.Join(" | ", _AnyFlags.ToList().Select(x => x.ToString()));

		/// <summary>
		/// Initializes the attribute.
		/// </summary>
		/// <param name="anyOfTheListedPerms"></param>
		/// <param name="allOfTheListedPerms"></param>
		public PermissionRequirementAttribute(GuildPermission[] anyOfTheListedPerms, GuildPermission[] allOfTheListedPerms)
		{
			var allFlags = (GuildPermission)0;
			var anyFlags = (GuildPermission)0;

			anyFlags |= GuildPermission.Administrator;
			foreach (var perm in anyOfTheListedPerms ?? Enumerable.Empty<GuildPermission>())
			{
				anyFlags |= perm;
			}
			foreach (var perm in allOfTheListedPerms ?? Enumerable.Empty<GuildPermission>())
			{
				allFlags |= perm;
			}

			_AllFlags = new GuildPermissions((ulong)allFlags);
			_AnyFlags = new GuildPermissions((ulong)anyFlags);
		}

		/// <summary>
		/// Checks if the user has the correct permissions. Otherwise returns an error.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="command"></param>
		/// <param name="map"></param>
		/// <returns></returns>
		public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider map)
		{
			if (!(context is AdvobotSocketCommandContext advobotCommandContext && context.User is SocketGuildUser user))
			{
				return Task.FromResult(PreconditionResult.FromError((string)null));
			}

			var guildBits = user.GuildPermissions.RawValue;
			var botBits = advobotCommandContext.GuildSettings.BotUsers.FirstOrDefault(x => x.UserId == user.Id)?.Permissions ?? 0;
			var userPerms = guildBits | botBits;

			var all = _AllFlags.RawValue != 0 && (userPerms & _AllFlags.RawValue) == _AllFlags.RawValue;
			var any = userPerms != 0 && _AnyFlags.RawValue != 0 && (userPerms & _AnyFlags.RawValue) != 0;
			if (all || any)
			{
				return Task.FromResult(PreconditionResult.FromSuccess());
			}
			return Task.FromResult(PreconditionResult.FromError((string)null));
		}

		/// <summary>
		/// Joins <see cref="AnyText"/> and <see cref="AllText"/>.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return $"[{new[] { AllText, AnyText }.JoinNonNullStrings(" | ")}]";
		}
	}
}
