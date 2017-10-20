using Discord;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Advobot.Classes.Permissions
{
	/// <summary>
	/// Helper class for channel permissions.
	/// </summary>
	public static class ChannelPerms
	{
		private const ulong GENERAL_BITS = 0
			| (1UL << (int)ChannelPermission.CreateInstantInvite)
			| (1UL << (int)ChannelPermission.ManageChannel)
			| (1UL << (int)ChannelPermission.ManagePermissions)
			| (1UL << (int)ChannelPermission.ManageWebhooks);
		private const ulong TEXT_BITS = 0
			| (1UL << (int)ChannelPermission.ReadMessages)
			| (1UL << (int)ChannelPermission.SendMessages)
			| (1UL << (int)ChannelPermission.SendTTSMessages)
			| (1UL << (int)ChannelPermission.ManageMessages)
			| (1UL << (int)ChannelPermission.EmbedLinks)
			| (1UL << (int)ChannelPermission.AttachFiles)
			| (1UL << (int)ChannelPermission.ReadMessageHistory)
			| (1UL << (int)ChannelPermission.MentionEveryone)
			| (1UL << (int)ChannelPermission.UseExternalEmojis)
			| (1UL << (int)ChannelPermission.AddReactions);
		private const ulong VOICE_BITS = 0
			| (1UL << (int)ChannelPermission.Connect)
			| (1UL << (int)ChannelPermission.Speak)
			| (1UL << (int)ChannelPermission.MuteMembers)
			| (1UL << (int)ChannelPermission.DeafenMembers)
			| (1UL << (int)ChannelPermission.MoveMembers)
			| (1UL << (int)ChannelPermission.UseVAD);

		public static ImmutableList<ChannelPerm> Permissions = ImmutableList.Create(CreateChannelPermList());

		/// <summary>
		/// Returns the first <see cref="ChannelPerm"/> to have the given name. (Case insensitive)
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public static ChannelPerm GetByName(string name)
		{
			return Permissions.FirstOrDefault(x => x.Name.CaseInsEquals(name));
		}
		/// <summary>
		/// Returns the first <see cref="ChannelPerm"/> to have the given value.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static ChannelPerm GetByExactValue(ulong value)
		{
			return Permissions.FirstOrDefault(x => x.Value == value);
		}
		/// <summary>
		/// Returns the first <see cref="ChannelPerm"/> to not have its value ANDed together with the argument equal zero.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static ChannelPerm GetByIncludedValue(ulong value)
		{
			return Permissions.FirstOrDefault(x => (x.Value & value) != 0);
		}
		/// <summary>
		/// Returns the first <see cref="ChannelPerm"/> to equal 1 shifted to the left with the passed in number.
		/// </summary>
		/// <param name="bit"></param>
		/// <returns></returns>
		public static ChannelPerm GetByBit(int bit)
		{
			return Permissions.FirstOrDefault(x => x.Value == (1UL << bit));
		}

		/// <summary>
		/// Returns the channel permissions that are set within the passed in ulong.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static ChannelPerm[] ConvertToPermissions(ulong value)
		{
			return Permissions.Where(x => (x.Value & value) != 0).ToArray();
		}
		/// <summary>
		/// Returns the channel permissions which can be found with the passed in names.
		/// </summary>
		/// <param name="permissionNames"></param>
		/// <returns></returns>
		public static ChannelPerm[] ConvertToPermissions(IEnumerable<string> permissionNames)
		{
			return Permissions.Where(x => permissionNames.CaseInsContains(x.Name)).ToArray();
		}

		/// <summary>
		/// Returns a ulong which is every permission ORed together.
		/// </summary>
		/// <param name="permissions"></param>
		/// <returns></returns>
		public static ulong ConvertToValue(IEnumerable<ChannelPerm> permissions)
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
		/// Returns the names of channel permissions that are set within the passed in ulong.
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
		public static bool TryGetValidChannelPermissionNamesFromInputString(string input, out IEnumerable<string> validPerms, out IEnumerable<string> invalidPerms)
		{
			var permissions = input.Split('/', ' ').Select(x => x.Trim(','));
			validPerms = permissions.Where(x => Permissions.Select(y => y.Name).CaseInsContains(x));
			invalidPerms = permissions.Where(x => !Permissions.Select(y => y.Name).CaseInsContains(x));
			return !invalidPerms.Any();
		}

		private static ChannelPerm[] CreateChannelPermList()
		{
			var temp = new List<ChannelPerm>();
			for (int i = 0; i < 64; ++i)
			{
				var name = Enum.GetName(typeof(ChannelPermission), i);
				if (name == null)
				{
					continue;
				}

				if ((GENERAL_BITS & (1UL << i)) != 0)
				{
					temp.Add(new ChannelPerm(name, i, gen: true));
				}
				if ((TEXT_BITS & (1UL << i)) != 0)
				{
					temp.Add(new ChannelPerm(name, i, text: true));
				}
				if ((VOICE_BITS & (1UL << i)) != 0)
				{
					temp.Add(new ChannelPerm(name, i, voice: true));
				}
			}
			return temp.ToArray();
		}

		/// <summary>
		/// Holds a channel permission name and value. Also holds booleans describing whether or not the permissions is on text/voice/both channels.
		/// </summary>
		public struct ChannelPerm
		{
			public string Name { get; }
			public ulong Value { get; }
			public bool General { get; }
			public bool Text { get; }
			public bool Voice { get; }

			public ChannelPerm(string name, int position, bool gen = false, bool text = false, bool voice = false)
			{
				Name = name;
				Value = (1UL << position);
				General = gen;
				Text = text;
				Voice = voice;
			}
		}
	}
}
