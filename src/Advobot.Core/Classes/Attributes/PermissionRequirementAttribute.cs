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
		private GuildPermission _AllFlags;
		private GuildPermission _AnyFlags;

		//This doesn't have default values for the parameters since that makes it harder to potentially provide the wrong permissions
		public PermissionRequirementAttribute(GuildPermission[] anyOfTheListedPerms, GuildPermission[] allOfTheListedPerms)
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

		public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider map)
		{
			if (!(context is IAdvobotCommandContext advobotCommandContext && context.User is IGuildUser user))
			{
				return Task.FromResult(PreconditionResult.FromError(Constants.IGNORE_ERROR));
			}

			var guildBits = user.GuildPermissions.RawValue;
			var botBits = advobotCommandContext.GuildSettings.BotUsers.FirstOrDefault(x => x.UserId == user.Id)?.Permissions ?? 0;
			var userPerms = guildBits | botBits;

			var all = _AllFlags != 0 && ((GuildPermission)userPerms & _AllFlags) == _AllFlags;
			var any = userPerms != 0 && _AnyFlags != 0 && ((GuildPermission)userPerms & _AnyFlags) != 0;
			if (all || any)
			{
				return Task.FromResult(PreconditionResult.FromSuccess());
			}
			return Task.FromResult(PreconditionResult.FromError(Constants.IGNORE_ERROR));
		}

		public string AllText => String.Join(" & ", GuildPermsUtils.ConvertValueToNames((ulong)_AllFlags));
		public string AnyText => String.Join(" | ", GuildPermsUtils.ConvertValueToNames((ulong)_AnyFlags));

		public override string ToString()
		{
			return $"[{GeneralFormatting.JoinNonNullStrings(" | ", AllText, AnyText)}]";
		}
	}
}
