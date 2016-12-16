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

namespace Advobot
{
	//Use this for testing for either of two types of role
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
	public class PermissionRequirementsAttribute : PreconditionAttribute
	{
		public PermissionRequirementsAttribute(uint needed, uint optional)
		{
			mNeeded = needed;
			mOptional = optional;
		}

		public override async Task<PreconditionResult> CheckPermissions(CommandContext context, CommandInfo command, IDependencyMap map)
		{
			IGuildUser user = await context.Guild.GetUserAsync(context.User.Id);
			GuildPermissions perms = user.GuildPermissions;
			PreconditionResult result;
			if ((perms.RawValue & mNeeded) == mNeeded)
				result = PreconditionResult.FromSuccess();
			else if ((perms.RawValue & mOptional) != 0)
				result = PreconditionResult.FromSuccess();
			else
				result = PreconditionResult.FromError(Constants.IGNORE_ERROR);
			return result;
		}

		public String Text
		{
			get { return String.Join(", ", Actions.getPermissionNames(mNeeded)); }
		}

		private uint mNeeded;
		private uint mOptional;
	}

	//Use for testing if the person is the bot owner
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public class BotOwnerRequirementAttribute : PreconditionAttribute
	{
		public override async Task<PreconditionResult> CheckPermissions(CommandContext context, CommandInfo command, IDependencyMap map)
		{
			IGuildUser user = await context.Guild.GetUserAsync(context.User.Id);
			if (user.Id.Equals(Constants.OWNER_ID))
				return PreconditionResult.FromSuccess();
			return PreconditionResult.FromError(Constants.IGNORE_ERROR);
		}
	}

	//Make the usage attribute
	public class UsageAttribute : Attribute
	{
		public UsageAttribute(String str)
		{
			mUsage = str;
		}

		private String mUsage;

		public String Text
		{
			get { return mUsage; }
		}
	}

	//Make a list of help information
	public class HelpEntry
	{
		public HelpEntry(String name, String[] aliases, String usage, String basePerm, String text)
		{
			mName = name;
			mAliases = aliases;
			mUsage = usage;
			mBasePerm = basePerm;
			mText = text;
		}

		public String Name
		{
			get { return mName; }
		}
		public String Aliases
		{
			get { return string.Join(", ", mAliases); }
		}
		public String Usage
		{
			get { return mUsage; }
		}
		public String basePerm
		{
			get { return mBasePerm; }
		}
		public String Text
		{
			get { return mText; }
		}

		private String mName;
		private String[] mAliases;
		private String mUsage;
		private String mBasePerm;
		private String mText;
	}

	//Categories for preferences
	public class PreferenceCategory
	{
		public PreferenceCategory(String name)
		{
			mName = name;
		}
		public String mName;
		public List<PreferenceSetting> mSettings = new List<PreferenceSetting>();
	}

	//Storing the settings for preferences
	public class PreferenceSetting
	{
		public PreferenceSetting(String name, String value)
		{
			mName = name;
			mValue = value;
		}
		public String mName;
		private String mValue;

		//Return the value as a boolean
		public bool asBoolean()
		{
			String[] trueMatches = { "true", "on", "yes", "1" };
			//String[] falseMatches = { "false", "off", "no", "0" };
			return trueMatches.Any(x => String.Equals(mValue.Trim(), x, StringComparison.OrdinalIgnoreCase));
		}

		//Return the value as a string
		public String asString()
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
}
