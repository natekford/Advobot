using Discord;

namespace Advobot.AutoMod.Database.Models;

public sealed class SelfRoleState(int group, IRole role, IReadOnlyList<IRole> conflictingRoles)
{
	public IReadOnlyList<IRole> ConflictingRoles { get; } = conflictingRoles;
	public int Group { get; } = group;
	public IRole Role { get; } = role;
}