using Advobot.Core.Utilities;
using Discord;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Advobot.Core.Classes.Permissions
{
	/// <summary>
	/// Helper class for guild permissions.
	/// </summary>
	public static class GuildPerms
	{
		//Permissions that indicate the user can generally be trusted with semi spammy commands
		public const GuildPermission USER_HAS_A_PERMISSION_PERMS = 0
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

		public static ImmutableArray<GuildPerm> Permissions = ImmutableArray.Create(CreatePermList());

		/// <summary>
		/// Returns the first <see cref="GuildPerm"/> to have the given name. (Case insensitive)
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public static GuildPerm GetByName(string name)
		{
			return Permissions.FirstOrDefault(x => x.Name.CaseInsEquals(name));
		}
		/// <summary>
		/// Returns the first <see cref="GuildPerm"/> to have the given value.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static GuildPerm GetByExactValue(ulong value)
		{
			return Permissions.FirstOrDefault(x => (ulong)x.Value == value);
		}
		/// <summary>
		/// Returns the first <see cref="GuildPerm"/> to not have its value ANDed together with the argument equal zero.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static GuildPerm GetByIncludedValue(ulong value)
		{
			return Permissions.FirstOrDefault(x => ((ulong)x.Value & value) != 0);
		}
		/// <summary>
		/// Returns the first <see cref="GuildPerm"/> to equal 1 shifted to the left with the passed in number.
		/// </summary>
		/// <param name="bit"></param>
		/// <returns></returns>
		public static GuildPerm GetByBit(int bit)
		{
			return Permissions.FirstOrDefault(x => (ulong)x.Value == (1UL << bit));
		}
		/// <summary>
		/// Returns the guild permissions that are set within the passed in ulong.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static GuildPerm[] ConvertToPermissions(ulong value)
		{
			return Permissions.Where(x => ((ulong)x.Value & value) != 0).ToArray();
		}
		/// <summary>
		/// Returns the guild permissions which can be found with the passed in names.
		/// </summary>
		/// <param name="permissionNames"></param>
		/// <returns></returns>
		public static GuildPerm[] ConvertToPermissions(IEnumerable<string> permissionNames)
		{
			return Permissions.Where(x => permissionNames.CaseInsContains(x.Name)).ToArray();
		}
		/// <summary>
		/// Returns a ulong which is every permission ORed together.
		/// </summary>
		/// <param name="permissions"></param>
		/// <returns></returns>
		public static GuildPermission ConvertToValue(IEnumerable<GuildPerm> permissions)
		{
			GuildPermission value = 0UL;
			foreach (var permission in permissions)
			{
				value |= permission.Value;
			}
			return value;
		}
		/// <summary>
		/// Returns a ulong which is every valid permission ORed together.
		/// </summary>
		/// <param name="permissionNames"></param>
		/// <returns></returns>
		public static GuildPermission ConvertToValue(IEnumerable<string> permissionNames)
		{
			return ConvertToValue(ConvertToPermissions(permissionNames));
		}
		/// <summary>
		/// Returns the names of guild permissions that are set within the passed in ulong.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static string[] ConvertValueToNames(ulong value)
		{
			return ConvertToPermissions(value).Select(x => x.Name).ToArray();
		}
		/// <summary>
		/// Returns a bool indicating true if all perms are valid. Out values of valid perms and invalid perms.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="validPerms"></param>
		/// <param name="invalidPerms"></param>
		/// <returns>Boolean representing true if all permissions are valid, false if any are invalid.</returns>
		public static bool TryGetValidPermissionNamesFromInputString(string input, out IEnumerable<string> validPerms, out IEnumerable<string> invalidPerms)
		{
			var permissions = input.Split('/', ' ').Select(x => x.Trim(','));
			validPerms = permissions.Where(x => Permissions.Select(y => y.Name).CaseInsContains(x));
			invalidPerms = permissions.Where(x => !Permissions.Select(y => y.Name).CaseInsContains(x));
			return !invalidPerms.Any();
		}

		private static GuildPerm[] CreatePermList()
		{
			var temp = new List<GuildPerm>();
			for (int i = 0; i < 64; ++i)
			{
				var val = (GuildPermission)(1UL << i);
				var name = Enum.GetName(typeof(GuildPermission), val);
				if (name == null)
				{
					continue;
				}

				temp.Add(new GuildPerm(name, val));
			}
			return temp.ToArray();
		}

		/// <summary>
		/// Holds a guild permission name and value.
		/// </summary>
		public struct GuildPerm
		{
			public string Name { get; }
			public GuildPermission Value { get; }

			public GuildPerm(string name, GuildPermission value)
			{
				Name = name;
				Value = value;
			}
		}
	}
}
