using Discord;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Advobot.Permissions
{
	/// <summary>
	/// Holds a guild permission name and value.
	/// </summary>
	public struct GuildPerm
	{
		public string Name { get; }
		public ulong Value { get; }

		public GuildPerm(string name, int position)
		{
			Name = name;
			Value = (1U << position);
		}
	}

	/// <summary>
	/// Helper class for guild permissions.
	/// </summary>
	public static class GuildPerms
	{
		public static IReadOnlyCollection<GuildPerm> Permissions = CreateGuildPermList();

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
			return Permissions.FirstOrDefault(x => x.Value == value);
		}
		/// <summary>
		/// Returns the first <see cref="GuildPerm"/> to not have its value ANDed together with the argument equal zero.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static GuildPerm GetByIncludedValue(ulong value)
		{
			return Permissions.FirstOrDefault(x => (x.Value & value) != 0);
		}
		/// <summary>
		/// Returns the first <see cref="GuildPerm"/> to equal 1 shifted to the left with the passed in number.
		/// </summary>
		/// <param name="bit"></param>
		/// <returns></returns>
		public static GuildPerm GetByBit(int bit)
		{
			return Permissions.FirstOrDefault(x => x.Value == (1U << bit));
		}

		/// <summary>
		/// Returns the guild permissions that are set within the passed in ulong.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static GuildPerm[] ConvertToPermissions(ulong value)
		{
			return Permissions.Where(x => (x.Value & value) != 0).ToArray();
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
		public static ulong ConvertToValue(IEnumerable<GuildPerm> permissions)
		{
			var value = 0UL;
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
		public static ulong ConvertToValue(IEnumerable<string> permissionNames)
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
		public static bool TryGetValidGuildPermissionNamesFromInputString(string input, out IEnumerable<string> validPerms, out IEnumerable<string> invalidPerms)
		{
			var permissions = input.Split('/', ' ').Select(x => x.Trim(','));
			validPerms = permissions.Where(x => Permissions.Select(y => y.Name).CaseInsContains(x));
			invalidPerms = permissions.Where(x => !Permissions.Select(y => y.Name).CaseInsContains(x));
			return !invalidPerms.Any();
		}

		private static IReadOnlyCollection<GuildPerm> CreateGuildPermList()
		{
			var temp = new List<GuildPerm>();
			for (int i = 0; i < 64; ++i)
			{
				var name = Enum.GetName(typeof(GuildPermission), i);
				if (name == null)
				{
					continue;
				}

				temp.Add(new GuildPerm(name, i));
			}
			return temp.AsReadOnly();
		}
	}
}
