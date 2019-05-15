using Advobot.Classes;
using Advobot.Classes.Results;
using Advobot.Classes.Settings;
using Advobot.Utilities;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Advobot.Commands.Responses
{
#warning give every command a unique number, not disabled/enabled based on name
	public sealed class SelfRoles : CommandResponses
	{
		private SelfRoles() { }

		public static AdvobotResult CreatedGroup(int number)
			=> Success(Default.FormatInterpolated($"Successfully created self assignable role group {number}."));
		public static AdvobotResult DeletedGroup(SelfAssignableRoles group)
			=> Success(Default.FormatInterpolated($"Successfully deleted self assignable role group {group.Group}."));
		public static AdvobotResult ModifiedGroup(SelfAssignableRoles group, IReadOnlyCollection<IRole> roles, bool added)
			=> Success(Default.FormatInterpolated($"Successfully {GetAdded(added)} the following roles to self assignable role group {group.Group}: {roles}"));
		public static AdvobotResult NotSelfAssignable(IRole role)
			=> Failure(Default.FormatInterpolated($"{role} is not a self assignable role.")).WithTime(DefaultTime);
		public static AdvobotResult RemovedRole(IRole role)
			=> Success(Default.FormatInterpolated($"Successfully removed {role}."));
		public static AdvobotResult AddedRole(IRole role)
			=> Success(Default.FormatInterpolated($"Successfully added {role}."));
		public static AdvobotResult AddedRoleAndRemovedOthers(IRole role, IEnumerable<IRole> others)
			=> Success(Default.FormatInterpolated($"Successfully added {role} and removed {others}."));
		public static AdvobotResult DisplayGroups(IEnumerable<SelfAssignableRoles> groups)
		{
			return Success(new EmbedWrapper
			{
				Title = "Self Assignable Role Groups",
				Description = BigBlock.FormatInterpolated($"{groups.OrderBy(x => x.Group).Join("\n", x => x.Group.ToString())}"),
			});
		}
		public static AdvobotResult DisplayGroup(SelfAssignableRoles group)
		{
			/*TODO: pass in guild to get the roles or rewrite self assignable roles?*/
			throw new NotImplementedException();
			return Success(new EmbedWrapper
			{
				Title = Title.FormatInterpolated($"Self Assignable Roles Group {group.Group}"),
				Description = "",
			});
		}
	}
}
