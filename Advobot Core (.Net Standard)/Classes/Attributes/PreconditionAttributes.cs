using Advobot.Actions;
using Advobot.Classes;
using Advobot.Enums;
using Advobot.Formatting;
using Advobot.Permissions;
using Discord;
using Discord.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Advobot.Attributes
{
	/// <summary>
	/// Checks if the user has all of the permissions supplied for allOfTheListedPerms or if the user has any of the permissions supplied for anyOfTheListedPerms.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public class PermissionRequirementAttribute : PreconditionAttribute
	{
		private uint _AllFlags;
		private uint _AnyFlags;

		//This doesn't have default values for the parameters since that makes it harder to potentially provide the wrong permissions
		public PermissionRequirementAttribute(GuildPermission[] anyOfTheListedPerms, GuildPermission[] allOfTheListedPerms)
		{
			_AnyFlags |= (1U << (int)GuildPermission.Administrator);
			foreach (var perm in anyOfTheListedPerms ?? Enumerable.Empty<GuildPermission>())
			{
				_AnyFlags |= (1U << (int)perm);
			}
			foreach (var perm in allOfTheListedPerms ?? Enumerable.Empty<GuildPermission>())
			{
				_AllFlags |= (1U << (int)perm);
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
			if (context is MyCommandContext myContext)
			{
				var user = context.User as IGuildUser;
				var guildBits = user.GuildPermissions.RawValue;
				var botBits = myContext.GuildSettings.BotUsers.FirstOrDefault(x => x.UserId == user.Id)?.Permissions ?? 0;
				var userPerms = guildBits | botBits;

				var all = _AllFlags != 0 && (userPerms & _AllFlags) == _AllFlags;
				var any = (userPerms & _AnyFlags) != 0;
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

	/// <summary>
	/// Checks if a user has any permissions that would generally be needed for a command, if the user is the guild owner, if the user if the bot owner, or if the user is a trusted user.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public class OtherRequirementAttribute : PreconditionAttribute
	{
		private const uint PERMISSION_BITS = 0
			| (1U << (int)GuildPermission.Administrator)
			| (1U << (int)GuildPermission.BanMembers)
			| (1U << (int)GuildPermission.DeafenMembers)
			| (1U << (int)GuildPermission.KickMembers)
			| (1U << (int)GuildPermission.ManageChannels)
			| (1U << (int)GuildPermission.ManageEmojis)
			| (1U << (int)GuildPermission.ManageGuild)
			| (1U << (int)GuildPermission.ManageMessages)
			| (1U << (int)GuildPermission.ManageNicknames)
			| (1U << (int)GuildPermission.ManageRoles)
			| (1U << (int)GuildPermission.ManageWebhooks)
			| (1U << (int)GuildPermission.MoveMembers)
			| (1U << (int)GuildPermission.MuteMembers);
		public readonly Precondition Requirements;

		public OtherRequirementAttribute(Precondition requirements)
		{
			Requirements = requirements;
		}

		public override async Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IServiceProvider map)
		{
			if (context is MyCommandContext myContext)
			{
				var user = context.User as IGuildUser;
				var permissions = (Requirements & Precondition.UserHasAPerm) != 0;
				var guildOwner = (Requirements & Precondition.GuildOwner) != 0;
				var trustedUser = (Requirements & Precondition.TrustedUser) != 0;
				var botOwner = (Requirements & Precondition.BotOwner) != 0;

				if (permissions)
				{
					var guildBits = user.GuildPermissions.RawValue;
					var botBits = myContext.GuildSettings.BotUsers.FirstOrDefault(x => x.UserId == user.Id)?.Permissions ?? 0;

					var userPerms = guildBits | botBits;
					if ((userPerms & PERMISSION_BITS) != 0)
					{
						return PreconditionResult.FromSuccess();
					}
				}
				if (guildOwner && myContext.Guild.OwnerId == user.Id)
				{
					return PreconditionResult.FromSuccess();
				}
				if (trustedUser && myContext.BotSettings.TrustedUsers.Contains(user.Id))
				{
					return PreconditionResult.FromSuccess();
				}
				if (botOwner && (await UserActions.GetBotOwner(myContext.Client)).Id == user.Id)
				{
					return PreconditionResult.FromSuccess();
				}
			}
			return PreconditionResult.FromError(Constants.IGNORE_ERROR);
		}

		public override string ToString()
		{
			var text = new System.Collections.Generic.List<string>();
			if ((Requirements & Precondition.UserHasAPerm) != 0)
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

	/// <summary>
	/// Checks to make sure the bot has admin, the bot is loaded, the guild is loaded, the channel isn't ignored from commands, and the command is enabled for the user.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public class CommandRequirementsAttribute : PreconditionAttribute
	{
		public override async Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IServiceProvider services)
		{
			if (context is MyCommandContext myContext)
			{
				var user = context.User as IGuildUser;

				if (!(await myContext.Guild.GetCurrentUserAsync()).GuildPermissions.Administrator)
				{
					return PreconditionResult.FromError($"This bot will not function without the `{nameof(GuildPermission.Administrator)}` permission.");
				}
				else if (!myContext.GuildSettings.Loaded)
				{
					return PreconditionResult.FromError("Wait until the guild is loaded.");
				}
				else if (myContext.GuildSettings.IgnoredCommandChannels.Contains(context.Channel.Id) || !CheckIfCommandIsEnabled(myContext, command, user))
				{
					return PreconditionResult.FromError(Constants.IGNORE_ERROR);
				}
				else
				{
					return PreconditionResult.FromSuccess();
				}
			}
			return PreconditionResult.FromError(Constants.IGNORE_ERROR);
		}

		private bool CheckIfCommandIsEnabled(MyCommandContext context, CommandInfo command, IGuildUser user)
		{
			//Use the first alias since that's what group gets set as (could use any alias since GetCommand works for aliases too)
			//Doing a split since subcommands (in this bot's case) are simply easy to use options on a single command
			var cmd = context.GuildSettings.GetCommand(command.Aliases[0].Split(' ')[0]);
			if (!cmd.Value)
			{
				return false;
			}

			//If any of user/role/channel are set that means they are ignored (unignored things will not be set)

			var userOverrides = context.GuildSettings.CommandsDisabledOnUser;
			var userOverride = userOverrides.FirstOrDefault(x => x.Id == context.User.Id && cmd.Name.CaseInsEquals(x.Name));
			if (userOverride != null)
			{
				return false;
			}

			var roleOverrides = context.GuildSettings.CommandsDisabledOnRole;
			var roleOverride = roleOverrides.Where(x => user.RoleIds.Contains(x.Id) && cmd.Name.CaseInsEquals(x.Name)).OrderBy(x => context.Guild.GetRole(x.Id).Position).LastOrDefault();
			if (roleOverride != null)
			{
				return false;
			}

			var channelOverrides = context.GuildSettings.CommandsDisabledOnChannel;
			var channelOverride = channelOverrides.FirstOrDefault(x => x.Id == context.Channel.Id && cmd.Name.CaseInsEquals(x.Name));
			if (channelOverride != null)
			{
				return false;
			}

			return true;
		}
	}

	/// <summary>
	/// Specifies a command is broken. Will provide an error each time a user tries to invoke the command.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
	public class BrokenCommandAttribute : PreconditionAttribute
	{
		public override Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IServiceProvider services)
		{
			return Task.FromResult(PreconditionResult.FromError("This command does not work."));
		}
	}
}
