using Advobot.Services.GuildSettings.Settings;

using Discord;

namespace Advobot.Classes
{
	/// <summary>
	/// Holds a role and the group it belongs to.
	/// </summary>
	public sealed class SelfAssignableRole
	{
		/// <summary>
		/// Creates an instance of <see cref="SelfAssignableRole"/>.
		/// </summary>
		/// <param name="group"></param>
		/// <param name="role"></param>
		public SelfAssignableRole(SelfAssignableRoles group, IRole role)
		{
			Role = role;
			Group = group;
		}

		/// <summary>
		/// The group which this role belongs to.
		/// </summary>
		public SelfAssignableRoles Group { get; }

		/// <summary>
		/// The role which can be assigned.
		/// </summary>
		public IRole Role { get; }
	}
}