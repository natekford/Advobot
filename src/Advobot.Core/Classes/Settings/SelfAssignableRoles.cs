using System.Collections.Generic;
using System.Linq;
using Advobot.Classes.Formatting;
using Discord;
using Newtonsoft.Json;

namespace Advobot.Classes.Settings
{
	/// <summary>
	/// Groups self assignable roles together.
	/// </summary>
	public sealed class SelfAssignableRoles : IGuildFormattable
	{
		/// <summary>
		/// The group number all the roles belong to.
		/// </summary>
		[JsonProperty("Group")]
		public int Group { get; set; }
		/// <summary>
		/// The ids of the roles.
		/// </summary>
		[JsonProperty("Roles")]
		public ISet<ulong> Roles { get; set; } = new HashSet<ulong>();

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
				Roles.Add(role.Id);
			}
		}
		/// <summary>
		/// Removes the roles from the group.
		/// </summary>
		/// <param name="roles"></param>
		public void RemoveRoles(IEnumerable<ulong> roles)
		{
			foreach (var role in roles)
			{
				Roles.Remove(role);
			}
		}
		/// <summary>
		/// Removes the roles from the group
		/// </summary>
		/// <param name="roles"></param>
		public void RemoveRoles(IEnumerable<IRole> roles)
			=> RemoveRoles(roles.Select(x => x.Id));

		/// <inheritdoc />
		public IDiscordFormattableString GetFormattableString()
		{
			return new Dictionary<string, object>
			{
				{ "Group", Group },
				{ "Roles", Roles },
			}.ToDiscordFormattableStringCollection();
		}
	}
}