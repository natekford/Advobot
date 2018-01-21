using Advobot.Core.Enums;
using Advobot.Core.Interfaces;
using Advobot.Core.Utilities;
using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Advobot.Core.Classes.Attributes
{
	/// <summary>
	/// Checks if a user has any permissions that would generally be needed for a command, if the user is the guild owner, if the user if the bot owner, or if the user is a trusted user.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public class OtherRequirementAttribute : PreconditionAttribute
	{
		public Precondition Requirements { get; }

		public OtherRequirementAttribute(Precondition requirements)
		{
			Requirements = requirements;
		}

		public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider map)
		{
			if (context is IAdvobotCommandContext advobotCommandContext)
			{
				var user = context.User as IGuildUser;
				var permissions = (Requirements & Precondition.GenericPerms) != 0;
				var guildOwner = (Requirements & Precondition.GuildOwner) != 0;
				var trustedUser = (Requirements & Precondition.TrustedUser) != 0;
				var botOwner = (Requirements & Precondition.BotOwner) != 0;

				if (permissions)
				{
					var guildBits = user.GuildPermissions.RawValue;
					var botBits = advobotCommandContext.GuildSettings.BotUsers.FirstOrDefault(x => x.UserId == user.Id)?.Permissions ?? 0;

					var userPerms = guildBits | botBits;
					if ((userPerms & (ulong)GuildPermsUtils.USER_HAS_A_PERMISSION_PERMS) != 0)
					{
						return PreconditionResult.FromSuccess();
					}
				}
				if (guildOwner && advobotCommandContext.Guild.OwnerId == user.Id)
				{
					return PreconditionResult.FromSuccess();
				}
				if (trustedUser && advobotCommandContext.BotSettings.TrustedUsers.Contains(user.Id))
				{
					return PreconditionResult.FromSuccess();
				}
				if (botOwner && (await ClientUtils.GetBotOwnerAsync(advobotCommandContext.Client).CAF()).Id == user.Id)
				{
					return PreconditionResult.FromSuccess();
				}
			}
			return PreconditionResult.FromError(Constants.IGNORE_ERROR);
		}

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
			return $"[{String.Join(" | ", text)}]";
		}
	}
}
