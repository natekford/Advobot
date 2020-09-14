using System.Collections.Generic;

using Discord;

namespace Advobot.AutoMod.Models
{
	public sealed class SelfRoleState
	{
		public IEnumerable<IRole>? ConflictingRoles { get; }
		public int Group { get; }
		public IRole Role { get; }

		public SelfRoleState(int group, IRole role, IEnumerable<IRole>? conflictingRoles)
		{
			Group = group;
			Role = role;
			ConflictingRoles = conflictingRoles;
		}
	}
}