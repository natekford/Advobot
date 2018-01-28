using System;
using System.Linq;
using System.Threading.Tasks;
using Advobot.Core.Interfaces;
using Advobot.Core.Utilities;
using Advobot.Core.Utilities.Formatting;
using Discord;
using Discord.Commands;

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

		//This doesn't have default values for the parameters since that makes it harder to potentially provide the wrong permissions
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

		public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider map)
		{
			if (!(context is IAdvobotCommandContext advobotCommandContext && context.User is IGuildUser user))
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

		public string AllText => String.Join(" & ", _AllFlags.ToList().Select(x => x.ToString()));
		public string AnyText => String.Join(" | ", _AnyFlags.ToList().Select(x => x.ToString()));

		public override string ToString()
		{
			return $"[{new[] { AllText, AnyText }.JoinNonNullStrings(" | ")}]";
		}
	}
}
