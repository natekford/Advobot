using System.Collections.Generic;
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
	public sealed class SelfAssignableRoles : IGuildFormattable
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
		public ICollection<ulong> Roles { get; } = new List<ulong>();

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
		public void RemoveRoles(IEnumerable<ulong> roles)
		{
			foreach (var role in roles)
			{
				Roles.Remove(role);
			}
		}
		/// <inheritdoc />
		public string Format(SocketGuild guild = null)
		{
			var roles = guild == null ? string.Join("`, `", Roles) : Roles.Join("`, `", x => guild.GetRole(x)?.Format());
			return $"**Roles:**\n{Roles.Join("\n", x => guild.GetRole(x)?.Format())}";
		}
		/// <inheritdoc />
		public override string ToString()
			=> Format();
	}
}