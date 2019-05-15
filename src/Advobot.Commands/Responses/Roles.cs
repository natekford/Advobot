using Advobot.Classes;
using Advobot.Classes.Results;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using System.Collections.Generic;
using System.Linq;

namespace Advobot.Commands.Responses
{
	public sealed class Roles : CommandResponses
	{
		private Roles() { }

		public static AdvobotResult Gave(IReadOnlyCollection<IRole> roles, IUser user)
			=> Success(Default.FormatInterpolated($"Successfully gave {roles} to {user}."));
		public static AdvobotResult Took(IReadOnlyCollection<IRole> roles, IUser user)
			=> Success(Default.FormatInterpolated($"Successfully took {roles} from {user}."));
		public static AdvobotResult Moved(IRole role, int position)
			=> Success(Default.FormatInterpolated($"Successfully moved {role} to position {position}."));
		public static AdvobotResult Display(IEnumerable<IRole> roles)
		{
			return Success(new EmbedWrapper
			{
				Title = "Roles",
				Description = BigBlock.FormatInterpolated($"{roles.Join("\n", x => $"{x.Position.ToString("00")}. {x.Name}")}"),
			});
		}
		public static AdvobotResult ModifiedPermissions(IRole role, GuildPermission permissions, bool allow)
			=> Success(Default.FormatInterpolated($"Successfully {GetAllowed(allow)} {EnumUtils.GetFlagNames(permissions)} for {role}."));
		public static AdvobotResult DisplayPermissions(IRole role)
		{
			var values = GuildPermissions.All.ToList().Select(x => (Name: x.ToString(), Value: GetAllowed(role.Permissions.Has(x))));
			var padLen = values.Max(x => x.Name.Length);
			return Success(new EmbedWrapper
			{
				Title = Default.FormatInterpolated($"Permissions For {role}"),
				Description = BigBlock.FormatInterpolated($"{values.Join("\n", x => $"{x.Name.PadRight(padLen)} {x.Value}")}"),
			});
		}
		public static AdvobotResult CopiedPermissions(IRole input, IRole output, GuildPermission permissions)
			=> Success(Default.FormatInterpolated($"Successfully copied as many permissions as possible from {input}. {output} now has the following permissions {EnumUtils.GetFlagNames(permissions)}."));
		public static AdvobotResult ClearedPermissions(IRole role, GuildPermission permissions)
			=> Success(Default.FormatInterpolated($"Successfully cleared as many permissions as possible. {role} now has the following permissions {EnumUtils.GetFlagNames(permissions)}."));
		public static AdvobotResult ModifiedColor(IRole role, Color color)
			=> Success(Default.FormatInterpolated($"Successfully changed the color of {role} to {"#" + color.RawValue.ToString("X6")}.")); //X6 to get hex
		public static AdvobotResult ModifiedHoistStatus(IRole role, bool hoisted)
			=> Success(Default.FormatInterpolated($"Successfully {GetHoisted(hoisted)} {role}."));
		public static AdvobotResult ModifiedMentionability(IRole role, bool mentionability)
			=> Success(Default.FormatInterpolated($"Successfully made {role} {GetMentionability(mentionability)}."));
	}
}
