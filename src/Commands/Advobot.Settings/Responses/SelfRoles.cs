using System.Collections.Generic;
using System.Linq;

using Advobot.Classes;
using Advobot.Modules;
using Advobot.Services.GuildSettings.Settings;
using Advobot.Utilities;

using AdvorangesUtils;

using Discord;

namespace Advobot.Settings.Responses
{
	public sealed class SelfRoles : CommandResponses
	{
		private SelfRoles()
		{
		}

		public static AdvobotResult AddedRole(IRole role)
			=> Success(Default.FormatInterpolated($"Successfully added {role}."));

		public static AdvobotResult AddedRoleAndRemovedOthers(IRole role, IEnumerable<IRole> others)
			=> Success(Default.FormatInterpolated($"Successfully added {role} and removed {others}."));

		public static AdvobotResult CreatedGroup(int number)
							=> Success(Default.FormatInterpolated($"Successfully created self assignable role group {number}."));

		public static AdvobotResult DeletedGroup(SelfAssignableRoles group)
			=> Success(Default.FormatInterpolated($"Successfully deleted self assignable role group {group.Group}."));

		public static AdvobotResult DisplayGroup(IGuild guild, SelfAssignableRoles group)
		{
			var validRoles = group.GetValidRoles(guild);
			return Success(new EmbedWrapper
			{
				Title = Title.FormatInterpolated($"Self Assignable Roles Group {group.Group}"),
				Description = BigBlock.FormatInterpolated($"{validRoles.Join(x => x.Format(), "\n")}"),
			});
		}

		public static AdvobotResult DisplayGroups(IEnumerable<SelfAssignableRoles> groups)
		{
			return Success(new EmbedWrapper
			{
				Title = "Self Assignable Role Groups",
				Description = BigBlock.FormatInterpolated($"{groups.OrderBy(x => x.Group).Join(x => x.Group.ToString(), "\n")}"),
			});
		}

		public static AdvobotResult ModifiedGroup(SelfAssignableRoles group, IReadOnlyCollection<IRole> roles, bool added)
							=> Success(Default.FormatInterpolated($"Successfully {GetAdded(added)} the following roles to self assignable role group {group.Group}: {roles}"));

		public static AdvobotResult RemovedRole(IRole role)
			=> Success(Default.FormatInterpolated($"Successfully removed {role}."));
	}
}