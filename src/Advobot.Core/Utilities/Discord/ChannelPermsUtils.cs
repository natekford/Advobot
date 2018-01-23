using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Discord;

namespace Advobot.Core.Utilities
{
	/// <summary>
	/// Helper class for channel permissions.
	/// </summary>
	public class ChannelPermsUtils
	{
		private const ChannelPermission GENERAL_BITS = 0
			| ChannelPermission.ViewChannel
			| ChannelPermission.CreateInstantInvite
			| ChannelPermission.ManageChannels
			| ChannelPermission.ManageRoles
			| ChannelPermission.ManageWebhooks;
		private const ChannelPermission TEXT_BITS = 0
			| ChannelPermission.SendMessages
			| ChannelPermission.SendTTSMessages
			| ChannelPermission.ManageMessages
			| ChannelPermission.EmbedLinks
			| ChannelPermission.AttachFiles
			| ChannelPermission.ReadMessageHistory
			| ChannelPermission.MentionEveryone
			| ChannelPermission.UseExternalEmojis
			| ChannelPermission.AddReactions;
		private const ChannelPermission VOICE_BITS = 0
			| ChannelPermission.Connect
			| ChannelPermission.Speak
			| ChannelPermission.MuteMembers
			| ChannelPermission.DeafenMembers
			| ChannelPermission.MoveMembers
			| ChannelPermission.UseVAD;

		public const ChannelPermission MUTE_ROLE_TEXT_PERMS = 0
			| ChannelPermission.CreateInstantInvite
			| ChannelPermission.ManageChannels
			| ChannelPermission.ManageRoles
			| ChannelPermission.ManageWebhooks
			| ChannelPermission.SendMessages
			| ChannelPermission.ManageMessages
			| ChannelPermission.AddReactions;
		public const ChannelPermission MUTE_ROLE_VOICE_PERMS = 0
			| ChannelPermission.CreateInstantInvite
			| ChannelPermission.ManageChannels
			| ChannelPermission.ManageRoles
			| ChannelPermission.ManageWebhooks
			| ChannelPermission.Speak
			| ChannelPermission.MuteMembers
			| ChannelPermission.DeafenMembers
			| ChannelPermission.MoveMembers;

		public static ImmutableList<ChannelPerm> Permissions = ImmutableList.Create(CreatePermList());

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
			return Permissions.FirstOrDefault(x => (ulong)x.Value == value);
		}
		/// <summary>
		/// Returns the first <see cref="ChannelPerm"/> to not have its value ANDed together with the argument equal zero.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static ChannelPerm GetByIncludedValue(ulong value)
		{
			return Permissions.FirstOrDefault(x => ((ulong)x.Value & value) != 0);
		}
		/// <summary>
		/// Returns the first <see cref="ChannelPerm"/> to equal 1 shifted to the left with the passed in number.
		/// </summary>
		/// <param name="bit"></param>
		/// <returns></returns>
		public static ChannelPerm GetByBit(int bit)
		{
			return Permissions.FirstOrDefault(x => (ulong)x.Value == (1UL << bit));
		}
		/// <summary>
		/// Returns the channel permissions that are set within the passed in ulong.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static ChannelPerm[] ConvertToPermissions(ulong value)
		{
			return Permissions.Where(x => ((ulong)x.Value & value) != 0).ToArray();
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
		public static ChannelPermission ConvertToValue(IEnumerable<ChannelPerm> permissions)
		{
			ChannelPermission value = 0UL;
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
		public static ChannelPermission ConvertToValue(IEnumerable<string> permissionNames)
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
		public static bool TryGetValidPermissionNamesFromInputString(string input, out IEnumerable<string> validPerms, out IEnumerable<string> invalidPerms)
		{
			var permissions = input.Split('/', ' ').Select(x => x.Trim(',')).ToList();
			validPerms = permissions.Where(x => Permissions.Select(y => y.Name).CaseInsContains(x));
			invalidPerms = permissions.Where(x => !Permissions.Select(y => y.Name).CaseInsContains(x));
			return !invalidPerms.Any();
		}

		private static ChannelPerm[] CreatePermList()
		{
			var temp = new List<ChannelPerm>();
			for (var i = 0; i < 64; ++i)
			{
				var val = (ChannelPermission)(1UL << i);
				var name = Enum.GetName(typeof(ChannelPermission), val);
				if (name == null)
				{
					continue;
				}

				if ((GENERAL_BITS & val) != 0)
				{
					temp.Add(new ChannelPerm(name, val, gen: true));
				}
				if ((TEXT_BITS & val) != 0)
				{
					temp.Add(new ChannelPerm(name, val, text: true));
				}
				if ((VOICE_BITS & val) != 0)
				{
					temp.Add(new ChannelPerm(name, val, voice: true));
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
			public ChannelPermission Value { get; }
			public bool General { get; }
			public bool Text { get; }
			public bool Voice { get; }

			public ChannelPerm(string name, ChannelPermission value, bool gen = false, bool text = false, bool voice = false)
			{
				Name = name;
				Value = value;
				General = gen;
				Text = text;
				Voice = voice;
			}
		}
	}
}
