using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Advobot.Interfaces;
using Advobot.Utilities;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace Advobot.Classes.Settings
{
	/// <summary>
	/// Groups self assignable roles together.
	/// </summary>
	public class SelfAssignableRoles : IGuildSetting
	{
		/// <summary>
		/// The group number all the roles belong to.
		/// </summary>
		[JsonProperty]
		public int Group { get; private set; }
		/// <summary>
		/// The ids of the roles.
		/// </summary>
		[JsonProperty("Roles")]
		public IList<ulong> Roles { get; } = new List<ulong>();

		/// <summary>
		/// Creates an instance of <see cref="SelfAssignableRoles"/>.
		/// </summary>
		public SelfAssignableRoles() { }
		/// <summary>
		/// Creates an instance of <see cref="SelfAssignableRoles"/>.
		/// </summary>
		/// <param name="group"></param>
		public SelfAssignableRoles(int group)
		{
			Group = group;
		}

		/// <summary>
		/// Adds the roles to the group.
		/// </summary>
		/// <param name="roles"></param>
		public void AddRoles(IEnumerable<IRole> roles)
		{
			foreach (var role in roles)
			{
				if (!Roles.Contains(role.Id))
				{
					Roles.Add(role.Id);
				}
			}
		}
		/// <summary>
		/// Removes the roles from the group.
		/// </summary>
		/// <param name="roles"></param>
		public void RemoveRoles(IEnumerable<IRole> roles)
		{
			foreach (var role in roles)
			{
				Roles.Remove(role.Id);
			}
		}
		/// <summary>
		/// Uses the guild to get all the roles.
		/// </summary>
		/// <param name="guild"></param>
		public void PostDeserialize(SocketGuild guild)
		{
			foreach (var roleId in Roles.ToList())
			{
				if (!(guild.GetRole(roleId) is IRole role))
				{
					Roles.Remove(roleId);
				}
			}
		}
		/// <inheritdoc />
		public override string ToString()
			=> $"**Role Ids:**\n{string.Join("\n", Roles)}";
		/// <inheritdoc />
		public string ToString(SocketGuild guild)
			=> $"**Role Ids:**\n{Roles.Join("\n", x => guild.GetRole(x)?.Format())}";
	}
}