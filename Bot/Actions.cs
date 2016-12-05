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
using static Advobot.Constants;

namespace Advobot
{
	public class Actions
	{
		//Get the information from the commands
		public static void loadCommandInformation()
		{
			var classTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes()).Where(type => type.IsSubclassOf(typeof(ModuleBase)));
			foreach (var classType in classTypes)
			{
				List<MethodInfo> methods = classType.GetMethods(BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic).ToList();
				foreach (var method in methods)
				{
					String name = "N/A";
					String[] aliases = { "N/A" };
					String usage = "N/A";
					String basePerm = "N/A";
					String text = "N/A";
					//Console.WriteLine(classType.Name + "." + method.Name);
					{
						CommandAttribute attr = (CommandAttribute)method.GetCustomAttribute(typeof(CommandAttribute));
						if (null != attr)
						{
							//Console.WriteLine(classType.Name + "." + method.Name + ": " + attr.Text);
							name = attr.Text;
						}
						else
						{
							continue;
						}
					}
					{
						AliasAttribute attr = (AliasAttribute)method.GetCustomAttribute(typeof(AliasAttribute));
						if (null != attr)
						{
							//Console.WriteLine(classType.Name + "." + method.Name + ": " + attr.Text);
							aliases = attr.Aliases;
						}
					}
					{
						UsageAttribute attr = (UsageAttribute)method.GetCustomAttribute(typeof(UsageAttribute));
						if (null != attr)
						{
							//Console.WriteLine(classType.Name + "." + method.Name + ": " + attr.Text);
							usage = attr.Text;
						}
					}
					{
						PermissionRequirementsAttribute attr = (PermissionRequirementsAttribute)method.GetCustomAttribute(typeof(PermissionRequirementsAttribute));
						if (null != attr)
						{
							//Console.WriteLine(classType.Name + "." + method.Name + ": " + attr.Text);
							basePerm = attr.Text;
						}
					}
					{
						SummaryAttribute attr = (SummaryAttribute)method.GetCustomAttribute(typeof(SummaryAttribute));
						if (null != attr)
						{
							//Console.WriteLine(classType.Name + "." + method.Name + ": " + attr.Text);
							text = attr.Text;
						}
					}
					Variables.HelpList.Add(new HelpEntry(name, aliases, usage, basePerm, text));
				}
			}
		}

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
					result = PreconditionResult.FromError(IGNORE_ERROR);
				return result;
			}

			public String Text
			{
				get { return String.Join(", ", getPermissionNames(mNeeded)); }
			}

			private uint mNeeded;
			private uint mOptional;
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

		//Get the permission names to an array
		public static String[] getPermissionNames(uint flags)
		{
			List<String> result = new List<String>();
			for (int i = 0; i < 32; ++i)
			{
				if ((flags & (1 << i)) != 0)
				{
					result.Add(Variables.mPermissionNames[i]);
				}
			}
			return result.ToArray();
		}

		//Find the permission names
		public static void loadPermissionNames()
		{
			for (int i = 0; i < 32; ++i)
			{
				String name = "";
				try
				{
					name = Enum.GetName(typeof(GuildPermission), (GuildPermission)i);
				}
				catch (Exception)
				{
					Console.WriteLine("Bad enum for GuildPermission: " + i);
				}
				Variables.mPermissionNames.Add(name);
			}
		}

		//Find a role on the server
		public static IRole getRole(IGuild guild, String roleName)
		{
			List<IRole> roles = guild.Roles.ToList();
			foreach (IRole role in roles)
			{
				if (role.Name.Equals(roleName))
				{
					return role;
				}
			}
			return null;
		}

		//Create a role on the server if it's not found
		public static async Task<IRole> createRoleIfNotFound(IGuild guild, String roleName)
		{
			if (getRole(guild, roleName) == null)
			{
				IRole role = await guild.CreateRoleAsync(roleName);
				return role;
			}
			return getRole(guild, roleName);
		} 

		//Get top position of a user
		public static int getPosition(IGuild guild, IGuildUser user)
		{
			int position = 0;
			user.RoleIds.ToList().ForEach(x => position = Math.Max(position, guild.GetRole(x).Position));
			return position;
		}

		//Get a user
		public static async Task<IGuildUser> getUser(IGuild guild, String userName)
		{
			IGuildUser user = await guild.GetUserAsync(getUlong(userName.Trim(new char[] { '<', '>', '@', '!' })));
			return user;
		}

		//Convert the input to a ulong
		public static ulong getUlong(String inputString)
		{
			ulong number = 0;
			if (UInt64.TryParse(inputString, out number))
			{
				return number;
			}
			return 0;
		}

		//Give the user the role
		public static async Task giveRole(IGuildUser user, IRole role)
		{
			if (null == role)
				return;
			await user.AddRolesAsync(role);
		}

		public static async Task<IRole> getRoleEditAbility(IGuild guild, IMessageChannel channel, IUserMessage message, IGuildUser user, IGuildUser bot, String input)
		{
			//Check if valid role
			IRole inputRole = getRole(guild, input);
			if (inputRole == null)
			{
				await makeAndDeleteSecondaryMessage(channel, message, ERROR(ROLE_ERROR), WAIT_TIME);
				return null;
			}

			//Determine if the user can edit the role
			if ((guild.OwnerId == user.Id ? OWNER_POSITION : getPosition(guild, user)) <= inputRole.Position)
			{
				await makeAndDeleteSecondaryMessage(channel, message, 
					ERROR(String.Format("`{0}` has a higher position than you are allowed to edit or use.", inputRole.Name)), WAIT_TIME);
				return null;
			}

			//Determine if the bot can edit the role
			if (getPosition(guild, bot) <= inputRole.Position)
			{
				await makeAndDeleteSecondaryMessage(channel, message, 
					ERROR(String.Format("`{0}` has a higher position than the bot is allowed to edit or use.", inputRole.Name)), WAIT_TIME);
				return null;
			}

			return inputRole;
		}

		//Remove secondary messages
		public static async Task makeAndDeleteSecondaryMessage(IMessageChannel channel, IUserMessage curMsg, String secondStr, Int32 time)
		{
			IUserMessage secondMsg = await channel.SendMessageAsync(ZERO_LENGTH_CHAR + secondStr);
			removeCommandMessages(channel, new IUserMessage[] { secondMsg, curMsg }, time);
		}

		//Remove commands
		public static void removeCommandMessages(IMessageChannel channel, IUserMessage[] messages, Int32 time)
		{
			Task t = Task.Run(async () =>
			{
				Thread.Sleep(time);
				await channel.DeleteMessagesAsync(messages);
			});
		}

		//Format the error message
		public static String ERROR(String message)
		{
			return ZERO_LENGTH_CHAR + ERROR_MESSAGE + " " + message;
		}

		//Send a message with a zero length char at the front
		public static async Task<IMessage> sendChannelMessage(IMessageChannel channel, String message)
		{
			return await channel.SendMessageAsync(ZERO_LENGTH_CHAR + message);
		}

		//Remove messages
		//private async Task removeMessages(IMessageChannel channel, int requestCount)
		//{
		//	//To remove the command itself
		//	++requestCount;

		//	while (requestCount > 0)
		//	{
		//		int deleteCount = Math.Min(MAX_MESSAGES_TO_GATHER, requestCount);
		//		IMessage[] messages = channel.GetMessagesAsync(deleteCount);
		//		if (messages.Length == 0)
		//			break;
		//		await channel.DeleteMessages(messages);
		//		requestCount -= messages.Length;
		//	}
		//}
	}
}
