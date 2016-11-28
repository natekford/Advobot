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
	public class Actions
	{
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
			if (user == null)
			{
				//TODO: fix
			}
			return user;
		}

		//
		public static ulong getUlong(String inputString)
		{
			ulong number = 0;
			if (UInt64.TryParse(inputString, out number))
			{
				return number;
			}
			return 0;
		}

		//
		public static async Task giveRole(IGuildUser user, IRole role)
		{
			if (null == role)
				return;
			await user.AddRolesAsync(role);
		}
	}
}
