using System.Collections.Generic;
using System.Linq;
using Advobot.Classes;
using Advobot.Formatting;
using Advobot.Modules;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using static Advobot.Standard.Resources.Responses;

namespace Advobot.Standard.Responses
{
	public sealed class Roles : CommandResponses
	{
		private Roles() { }

		public static AdvobotResult Gave(IReadOnlyCollection<IRole> roles, IUser user)
			=> Success(Default.Format(RolesGave, roles, user));
		public static AdvobotResult Took(IReadOnlyCollection<IRole> roles, IUser user)
			=> Success(Default.Format(RolesTook, roles, user));
		public static AdvobotResult Moved(IRole role, int position)
			=> Success(Default.Format(RoleMoved, role, position));
		public static AdvobotResult Display(IEnumerable<IRole> roles)
		{
			var text = roles.Join("\n", x => $"{x.Position.ToString("00")}. {x.Name}");
			return Success(new EmbedWrapper
			{
				Title = RolesTitleDisplay,
				Description = BigBlock.FormatInterpolated($"{text}"),
			});
		}
		public static AdvobotResult ModifiedPermissions(
			IRole role,
			GuildPermission permissions,
			bool allow)
		{
			var flags = EnumUtils.GetFlagNames(permissions);
			return Success(Default.FormatInterpolated($"Successfully {GetAllowed(allow)} {flags} for {role}."));
		}
		public static AdvobotResult DisplayPermissions(IRole role)
		{
			var values = GuildPermissions.All.ToList()
				.Select(x => (Name: x.ToString(), Value: GetAllowed(role.Permissions.Has(x))));
			var padLen = values.Max(x => x.Name.Length);
			var text = values.Join("\n", x => $"{x.Name.PadRight(padLen)} {x.Value}");
			return Success(new EmbedWrapper
			{
				Title = Default.FormatInterpolated($"Permissions For {role}"),
				Description = BigBlock.FormatInterpolated($"{text}"),
			});
		}
		public static AdvobotResult CopiedPermissions(
			IRole input,
			IRole output,
			GuildPermission permissions)
		{
			var flags = EnumUtils.GetFlagNames(permissions);
			return Success(Default.Format(RolesCopiedPermissions, flags, input, output));
		}
		public static AdvobotResult ClearedPermissions(IRole role)
			=> Success(Default.Format(RolesClearedPermissions, role));
		public static AdvobotResult ModifiedColor(IRole role, Color color)
			=> Success(Default.Format(RolesModifiedColor, role, color.RawValue.ToString("X6"))); //X6 to get hex
		public static AdvobotResult ModifiedHoistStatus(IRole role, bool hoisted)
			=> Success(Default.Format(RolesModifiedHoistedStatus, role, hoisted));
		public static AdvobotResult ModifiedMentionability(IRole role, bool mentionability)
			=> Success(Default.Format(RolesModifiedMentionability, role, mentionability));

		private static RuntimeFormattedObject GetAllowed(bool val)
			=> (val ? "allowed" : "denied").NoFormatting();
	}
}
