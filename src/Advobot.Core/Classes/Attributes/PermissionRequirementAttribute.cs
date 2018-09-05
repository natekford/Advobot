using System;
using System.Linq;
using System.Threading.Tasks;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using Discord.Commands;

namespace Advobot.Classes.Attributes
{
	/// <summary>
	/// Checks if the user has all of the permissions supplied for all or if the user has any of the permissions supplied for any.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	public sealed class PermissionRequirementAttribute : PreconditionAttribute
	{
		/// <summary>
		/// Indicates this user has a permission which should allow them to use basic commands which could potentially be spammy.
		/// </summary>
		public const GuildPermission GenericPerms = 0
			| GuildPermission.Administrator
			| GuildPermission.BanMembers
			| GuildPermission.DeafenMembers
			| GuildPermission.KickMembers
			| GuildPermission.ManageChannels
			| GuildPermission.ManageEmojis
			| GuildPermission.ManageGuild
			| GuildPermission.ManageMessages
			| GuildPermission.ManageNicknames
			| GuildPermission.ManageRoles
			| GuildPermission.ManageWebhooks
			| GuildPermission.MoveMembers
			| GuildPermission.MuteMembers;

		private GuildPermissions _AnyFlags;
		private GuildPermissions _AllFlags;

		/// <summary>
		/// Returns the names of the flags where all are needed.
		/// </summary>
		public string AllText => string.Join(" & ", _AllFlags.ToList().Select(x => x.ToString()));
		/// <summary>
		/// Returns the names of the flags where any are needed.
		/// </summary>
		public string AnyText => string.Join(" | ", _AnyFlags.ToList().Select(x => x.ToString()));

		/// <summary>
		/// Creates an instance of <see cref="PermissionRequirementAttribute"/>.
		/// </summary>
		/// <param name="any">If the user has any permissions from this list then the command will run.</param>
		/// <param name="all">If the user has all permissions from this list then the command will run.</param>
		public PermissionRequirementAttribute(GuildPermission[] any, GuildPermission[] all)
		{
			var anyFlags = (GuildPermission)0;
			var allFlags = (GuildPermission)0;

			anyFlags |= GuildPermission.Administrator;
			foreach (var perm in any ?? Enumerable.Empty<GuildPermission>())
			{
				anyFlags |= perm;
			}
			foreach (var perm in all ?? Enumerable.Empty<GuildPermission>())
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
		/// <param name="services"></param>
		/// <returns></returns>
		public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
		{
			var (Context, Invoker) = context.InternalCastContext();
			var guildBits = Invoker.GuildPermissions.RawValue;
			var botBits = Context.GuildSettings.BotUsers.FirstOrDefault(x => x.UserId == Invoker.Id)?.Permissions ?? 0;
			var userPerms = guildBits | botBits;

			var any = userPerms != 0 && _AnyFlags.RawValue != 0 && (userPerms & _AnyFlags.RawValue) != 0;
			var all = _AllFlags.RawValue != 0 && (userPerms & _AllFlags.RawValue) == _AllFlags.RawValue;
			//Return a null string if invalid so we can easily ignore it.
			return Task.FromResult(any || all ? PreconditionResult.FromSuccess() : PreconditionResult.FromError(default(string)));
		}
		/// <summary>
		/// Joins <see cref="AnyText"/> and <see cref="AllText"/>.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			//Special case, greatly shortens the output string while retaining what it means
			if (_AnyFlags.RawValue == (ulong)GenericPerms)
			{
				return "Administrator | Any perm ending with 'Members' | Any perm starting with 'Manage'";
			}
			return $"[{new[] { AllText, AnyText }.JoinNonNullStrings(" | ")}]";
		}
	}
}
