using System.Collections.Generic;
using System.Linq;
using System.Text;

using Advobot.Modules;
using Advobot.Utilities;

using AdvorangesUtils;

using Discord;

using static Advobot.Resources.Responses;
using static Advobot.Utilities.FormattingUtils;

namespace Advobot.AutoMod.Responses
{
	public sealed class PersistentRoles : AdvobotResult
	{
		private PersistentRoles() : base(null, "")
		{
		}

		public static AdvobotResult DisplayPersistentRoles(
			IEnumerable<IGrouping<string, IRole>> roles)
		{
			var sb = new StringBuilder("```\n");
			foreach (var group in roles)
			{
				sb.AppendLineFeed(group.Key);
				foreach (var role in group)
				{
					sb.AppendLineFeed("\t" + role.Format());
				}
				sb.AppendLineFeed();
			}
			sb.Append("```");
			return Success(sb.ToString());
		}

		public static AdvobotResult GavePersistentRole(IGuildUser user, IRole role)
		{
			return Success(PersistentRolesGave.Format(
				user.Format().WithBlock(),
				role.Format().WithBlock()
			));
		}

		public static AdvobotResult RemovedPersistentRole(IGuildUser user, IRole role)
		{
			return Success(PersistentRolesRemoved.Format(
				role.Format().WithBlock(),
				user.Format().WithBlock()
			));
		}
	}
}