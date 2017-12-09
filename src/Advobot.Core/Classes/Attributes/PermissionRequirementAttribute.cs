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
		private GuildPermission _AllFlags;
		private GuildPermission _AnyFlags;

		//This doesn't have default values for the parameters since that makes it harder to potentially provide the wrong permissions
		public PermissionRequirementAttribute(GuildPermission[] anyOfTheListedPerms, GuildPermission[] allOfTheListedPerms)
		{
			this._AnyFlags |= GuildPermission.Administrator;
			foreach (var perm in anyOfTheListedPerms ?? Enumerable.Empty<GuildPermission>())
			{
				this._AnyFlags |= perm;
			}
			foreach (var perm in allOfTheListedPerms ?? Enumerable.Empty<GuildPermission>())
			{
				this._AllFlags |= perm;
			}
		}

		public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider map)
		{
			if (context is IAdvobotCommandContext advobotCommandContext)
			{
				var user = context.User as IGuildUser;
				var guildBits = user.GuildPermissions.RawValue;
				var botBits = advobotCommandContext.GuildSettings.BotUsers.FirstOrDefault(x => x.UserId == user.Id)?.Permissions ?? 0;
				var userPerms = guildBits | botBits;

				var all = this._AllFlags != 0 && ((GuildPermission)userPerms & this._AllFlags) == this._AllFlags;
				var any = userPerms != 0 && this._AnyFlags != 0 && ((GuildPermission)userPerms & this._AnyFlags) != 0;
				if (all || any)
				{
					return Task.FromResult(PreconditionResult.FromSuccess());
				}
			}
			return Task.FromResult(PreconditionResult.FromError(Constants.IGNORE_ERROR));
		}

		public string AllText => String.Join(" & ", GuildPerms.ConvertValueToNames((ulong)this._AllFlags));
		public string AnyText => String.Join(" | ", GuildPerms.ConvertValueToNames((ulong)this._AnyFlags));

		public override string ToString() => $"[{GeneralFormatting.JoinNonNullStrings(" | ", this.AllText, this.AnyText)}]";
	}
}
