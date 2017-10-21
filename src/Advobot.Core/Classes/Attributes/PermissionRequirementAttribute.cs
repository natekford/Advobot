using Advobot.Core.Actions.Formatting;
using Advobot.Core.Classes.Permissions;
using Advobot.Core.Interfaces;
using Discord;
using Discord.Commands;
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
		private ulong _AllFlags;
		private ulong _AnyFlags;

		//This doesn't have default values for the parameters since that makes it harder to potentially provide the wrong permissions
		public PermissionRequirementAttribute(GuildPermission[] anyOfTheListedPerms, GuildPermission[] allOfTheListedPerms)
		{
			_AnyFlags |= (1UL << (int)GuildPermission.Administrator);
			foreach (var perm in anyOfTheListedPerms ?? Enumerable.Empty<GuildPermission>())
			{
				_AnyFlags |= (1UL << (int)perm);
			}
			foreach (var perm in allOfTheListedPerms ?? Enumerable.Empty<GuildPermission>())
			{
				_AllFlags |= (1UL << (int)perm);
			}
		}
		/* For when/if GuildPermission values get put as bits
		public PermissionRequirementAttribute(GuildPermission anyOfTheListedPerms, GuildPermission allOfTheListedPerms)
		{
			_AnyFlags |= GuildPermission.Administrator;
			foreach (var perm in anyOfTheListedPerms ?? Enumerable.Empty<GuildPermission>())
			{
				_AnyFlags |= perm;
			}
			foreach (var perm in allOfTheListedPerms ?? Enumerable.Empty<GuildPermission>())
			{
				_AllFlags |= perm;
			}
		}
		*/

		public override Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IServiceProvider map)
		{
			if (context is IAdvobotCommandContext advobotCommandContext)
			{
				var user = context.User as IGuildUser;
				var guildBits = user.GuildPermissions.RawValue;
				var botBits = advobotCommandContext.GuildSettings.BotUsers.FirstOrDefault(x => x.UserId == user.Id)?.Permissions ?? 0;
				var userPerms = guildBits | botBits;

				var all = _AllFlags != 0 && (userPerms & _AllFlags) == _AllFlags;
				var any = userPerms != 0 && _AnyFlags != 0 && (userPerms & _AnyFlags) != 0;
				if (all || any)
				{
					return Task.FromResult(PreconditionResult.FromSuccess());
				}
			}
			return Task.FromResult(PreconditionResult.FromError(Constants.IGNORE_ERROR));
		}

		public string AllText => String.Join(" & ", GuildPerms.ConvertValueToNames(_AllFlags));
		public string AnyText => String.Join(" | ", GuildPerms.ConvertValueToNames(_AnyFlags));

		public override string ToString()
		{
			return $"[{GeneralFormatting.JoinNonNullStrings(" | ", AllText, AnyText)}]";
		}
	}
}
