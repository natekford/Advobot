using System.Collections.Generic;
using System.Linq;
using System.Text;

using Advobot.Modules;
using Advobot.Utilities;

using AdvorangesUtils;

using Discord;

using static Advobot.Resources.Responses;

namespace Advobot.AutoMod.Responses
{
	public sealed class SelfRoles : AdvobotResult
	{
		private SelfRoles() : base(null, "")
		{
		}

		public static AdvobotResult AddedRole(IRole role)
		{
			return Success(SelfRolesAdded.Format(
				role.Format().WithBlock()
			));
		}

		public static AdvobotResult AddedRoleAndRemovedOthers(IRole role)
		{
			return Success(SelfRolesAddedAndRemovedOthers.Format(
				role.Format().WithBlock()
			));
		}

		public static AdvobotResult AddedSelfRoles(int group, int count)
		{
			return Success(SelfRolesAddedToGroup.Format(
				count.ToString().WithBlock(),
				group.ToString().WithBlock()
			));
		}

		public static AdvobotResult ClearedGroup(int group, int count)
		{
			return Success(SelfRolesClearedGroup.Format(
				count.ToString().WithBlock(),
				group.ToString().WithBlock()
			));
		}

		public static AdvobotResult DisplayGroups(IEnumerable<IGrouping<int, IRole>> groups)
		{
			var sb = new StringBuilder("```");
			foreach (var group in groups)
			{
				sb.AppendLineFeed().Append(group.Key).AppendLineFeed();
				foreach (var role in group)
				{
					sb.AppendLineFeed(role.Format());
				}
			}
			sb.AppendLineFeed("```");
			return Success(sb.ToString());
		}

		public static AdvobotResult RemovedRole(IRole role)
		{
			return Success(SelfRolesRemoved.Format(
				role.Format().WithBlock()
			));
		}

		public static AdvobotResult RemovedSelfRoles(int count)
		{
			return Success(SelfRolesRemovedFromGroup.Format(
				count.ToString().WithBlock()
			));
		}
	}
}