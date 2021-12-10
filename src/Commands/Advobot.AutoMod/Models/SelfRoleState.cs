using Discord;

namespace Advobot.AutoMod.Models;

public sealed class SelfRoleState
{
	public IReadOnlyList<IRole> ConflictingRoles { get; }
	public int Group { get; }
	public IRole Role { get; }

	public SelfRoleState(int group, IRole role, IReadOnlyList<IRole> conflictingRoles)
	{
		Group = group;
		Role = role;
		ConflictingRoles = conflictingRoles;
	}
}