using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using Discord;
using Discord.Commands;
using Discord.Modules;
using Discord.WebSocket;
using System.Net;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Advobot
{
	//If the user has all the perms required for the first arg then success, any of the second arg then success. Nothing means only Administrator works.
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public class PermissionRequirementsAttribute : PreconditionAttribute
	{
		public PermissionRequirementsAttribute(uint anyOfTheListedPerms = 0, uint allOfTheListedPerms = 0)
		{
			mAllFlags = allOfTheListedPerms;
			mAnyFlags = anyOfTheListedPerms | (1U << (int)GuildPermission.Administrator);
		}

		public override async Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IDependencyMap map)
		{
			if (context.Guild != null)
			{
				IGuildUser user = await context.Guild.GetUserAsync(context.User.Id);
				GuildPermissions perms = user.GuildPermissions;
				if (mAllFlags != 0 && (perms.RawValue & mAllFlags) == mAllFlags)
					return PreconditionResult.FromSuccess();
				else if ((perms.RawValue & mAnyFlags) != 0)
					return PreconditionResult.FromSuccess();
			}
			return PreconditionResult.FromError(Constants.IGNORE_ERROR);
		}

		public string AllText
		{
			get { return String.Join(" and ", Actions.getPermissionNames(mAllFlags)); }
		}

		public string AnyText
		{
			get { return String.Join(" or ", Actions.getPermissionNames(mAnyFlags)); }
		}

		private uint mAllFlags;
		private uint mAnyFlags;
	}

	//Testing if the user is the bot owner or the guild owner
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public class BotOwnerOrGuildOwnerRequirementAttribute : PreconditionAttribute
	{
		public override async Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IDependencyMap map)
		{
			return (await Actions.userHasOwner(context.Guild, context.User)) || Actions.userHasBotOwner(context.Guild, context.User) ?
				PreconditionResult.FromSuccess() : PreconditionResult.FromError(Constants.IGNORE_ERROR);
		}
	}

	//Use for testing if the person is the bot owner
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public class BotOwnerRequirementAttribute : PreconditionAttribute
	{
		public override Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IDependencyMap map)
		{
			return Task.Run(() =>
			{
				return Actions.userHasBotOwner(context.Guild, context.User) ? PreconditionResult.FromSuccess() : PreconditionResult.FromError(Constants.IGNORE_ERROR);
			});
		}
	}

	//Testing if the user if the guild owner
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public class GuildOwnerRequirementAttribute : PreconditionAttribute
	{
		public override async Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IDependencyMap map)
		{
			return await Actions.userHasOwner(context.Guild, context.User) ? PreconditionResult.FromSuccess() : PreconditionResult.FromError(Constants.IGNORE_ERROR);
		}
	}

	//Check if the user has any permission that would allow them to use the bot regularly
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
	public class UserHasAPermissionAttribute : PreconditionAttribute
	{
		private const UInt32 PERMISSIONBITS = 0
			| (1U<<(int)GuildPermission.Administrator)
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

		public override async Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IDependencyMap map)
		{
			if (context.Guild != null)
			{
				IGuildUser user = await context.Guild.GetUserAsync(context.User.Id);
				return ((user.GuildPermissions.RawValue & PERMISSIONBITS) != 0) ? PreconditionResult.FromSuccess() : PreconditionResult.FromError(Constants.IGNORE_ERROR);
			}
			return PreconditionResult.FromError(Constants.IGNORE_ERROR);
		}
	}

	//Make the usage attribute
	public class UsageAttribute : Attribute
	{
		public UsageAttribute(string str)
		{
			mUsage = str;
		}

		private string mUsage;

		public string Text
		{
			get { return mUsage; }
		}
	}

	//Make a list of help information
	public class HelpEntry
	{
		public HelpEntry(string name, string[] aliases, string usage, string basePerm, string text)
		{
			mName = name;
			mAliases = aliases;
			mUsage = usage;
			mBasePerm = basePerm;
			mText = text;
		}

		public string Name
		{
			get { return mName; }
		}
		public string[] Aliases
		{
			get { return mAliases; }
		}
		public string Usage
		{
			get { return mUsage; }
		}
		public string basePerm
		{
			get { return mBasePerm; }
		}
		public string Text
		{
			get { return mText; }
		}

		private string mName;
		private string[] mAliases;
		private string mUsage;
		private string mBasePerm;
		private string mText;
	}

	//Categories for preferences
	public class PreferenceCategory
	{
		public PreferenceCategory(string name)
		{
			mName = name;
		}
		public string mName;
		public List<PreferenceSetting> mSettings = new List<PreferenceSetting>();
	}

	//Storing the settings for preferences
	public class PreferenceSetting
	{
		public PreferenceSetting(string name, string value)
		{
			mName = name;
			mValue = value;
		}
		public string mName;
		private string mValue;

		//Return the value as a boolean
		public bool asBoolean()
		{
			string[] trueMatches = { "true", "on", "yes", "1" };
			//string[] falseMatches = { "false", "off", "no", "0" };
			return trueMatches.Any(x => String.Equals(mValue.Trim(), x, StringComparison.OrdinalIgnoreCase));
		}

		//Return the value as a string
		public string asString()
		{
			return mValue;
		}

		//Return the value as an int
		public int asInteger()
		{
			int value;
			if (Int32.TryParse(mValue, out value))
			{
				return value;
			}
			return -1;
		}
	}

	public struct ChannelAndPosition
	{
		public ChannelAndPosition(IGuildChannel channel, int position)
		{
			this.Channel = channel;
			this.Position = position;
		}

		public IGuildChannel Channel;
		public int Position;
	}

	public class SlowmodeUser
	{
		public SlowmodeUser(IGuildUser mUser = null, int mCurrentMessagesLeft = 1, int mBaseMessages = 1, int mTime = 5)
		{
			this.User = mUser;
			this.Current_Messages_Left = mCurrentMessagesLeft;
			this.Base_Messages = mBaseMessages;
			this.Time = mTime;
		}

		public IGuildUser User;
		public int Current_Messages_Left;
		public int Base_Messages;
		public int Time;
	}

	public class SlowmodeChannel
	{
		public SlowmodeChannel(ulong mChannelID, ulong mGuildID)
		{
			this.Channel_ID = mChannelID;
			this.Guild_ID = mGuildID;
		}

		public ulong Channel_ID;
		public ulong Guild_ID;
	}
}