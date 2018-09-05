using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Advobot.Enums;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Advobot.Classes.Attributes
{
	/*
	/// <summary>
	/// Checks if a user has any permissions that would generally be needed for a command, if the user is the guild owner, if the user if the bot owner, or if the user is a trusted user.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	public sealed class OtherRequirementAttribute : PreconditionAttribute
	{
		/// <summary>
		/// Preconditions that need to be met before the command fires successfully.
		/// </summary>
		public Precondition Requirements { get; }

		/// <summary>
		/// Creates an instance of <see cref="OtherRequirementAttribute"/>.
		/// </summary>
		/// <param name="requirements"></param>
		public OtherRequirementAttribute(Precondition requirements)
		{
			Requirements = requirements;
		}

		/// <summary>
		/// Checks each precondition. If any fail, returns an error.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="command"></param>
		/// <param name="map"></param>
		/// <returns></returns>
		public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider map)
		{
			var (Context, Invoker) = context.InternalCastContext();

			if ((Requirements & Precondition.GenericPerms) != 0)
			{
				var guildBits = Invoker.GuildPermissions.RawValue;
				var botBits = Context.GuildSettings.BotUsers.FirstOrDefault(x => x.UserId == Invoker.Id)?.Permissions ?? 0;
				if (((guildBits | botBits) & (ulong)PermissionRequirementAttribute.GenericPerms) != 0)
				{
					return PreconditionResult.FromSuccess();
				}
			}
			if ((Requirements & Precondition.GuildOwner) != 0 && Context.Guild.OwnerId == Invoker.Id)
			{
				return PreconditionResult.FromSuccess();
			}
			if ((Requirements & Precondition.TrustedUser) != 0 && Context.BotSettings.TrustedUsers.Contains(Invoker.Id))
			{
				return PreconditionResult.FromSuccess();
			}
			if ((Requirements & Precondition.BotOwner) != 0 && await ClientUtils.GetOwnerIdAsync(Context.Client).CAF() == Invoker.Id)
			{
				return PreconditionResult.FromSuccess();
			}
			//Return null string so these errors won't spam channels.
			return PreconditionResult.FromError(default(string));
		}
		/// <summary>
		/// Returns the preconditions in a readable format.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			var text = new List<string>();
			if ((Requirements & Precondition.GenericPerms) != 0)
			{
				text.Add("Administrator | Any perm ending with 'Members' | Any perm starting with 'Manage'");
			}
			if ((Requirements & Precondition.GuildOwner) != 0)
			{
				text.Add("Guild Owner");
			}
			if ((Requirements & Precondition.TrustedUser) != 0)
			{
				text.Add("Trusted User");
			}
			if ((Requirements & Precondition.BotOwner) != 0)
			{
				text.Add("Bot Owner");
			}
			return $"[{string.Join(" | ", text)}]";
		}
	}*/
}
