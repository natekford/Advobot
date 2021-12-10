using Advobot.Classes;
using Advobot.Modules;
using Advobot.Utilities;

using AdvorangesUtils;

using Discord;

using static Advobot.Resources.Responses;

namespace Advobot.Standard.Responses;

public sealed class Roles : AdvobotResult
{
	private static readonly List<GuildPermission> _AllPerms = GuildPermissions.All.ToList();

	private Roles() : base(null, "")
	{
	}

	public static AdvobotResult ClearedPermissions(IRole role)
	{
		return Success(RolesClearedPermissions.Format(
			role.Format().WithBlock()
		));
	}

	public static AdvobotResult CopiedPermissions(
		IRole input,
		IRole output,
		GuildPermission permissions)
	{
		return Success(RolesCopiedPermissions.Format(
			EnumUtils.GetFlagNames(permissions).Join().WithBlock(),
			input.Format().WithBlock(),
			output.Format().WithBlock()
		));
	}

	public static AdvobotResult Display(IEnumerable<IRole> roles)
	{
		var description = roles
			.Join(x => $"{x.Position:00}. {x.Name}", Environment.NewLine)
			.WithBigBlock()
			.Value;
		return Success(new EmbedWrapper
		{
			Title = RolesTitleDisplay,
			Description = description,
		});
	}

	public static AdvobotResult DisplayPermissions(IRole role)
	{
		var title = RolesTitleDisplayPermissions.Format(
			role.Format().WithBlock()
		);
		var description = _AllPerms
			.ToDictionary(x => x, x => role.Permissions.Has(x) ? PermValue.Allow : PermValue.Deny)
			.FormatPermissionValues(x => x.ToString(), out var padLen)
			.Join(x => $"{x.Key.PadRight(padLen)} {x.Value}", "\n")
			.WithBigBlock()
			.Value;
		return Success(new EmbedWrapper
		{
			Title = title,
			Description = description,
		});
	}

	public static AdvobotResult Gave(IReadOnlyCollection<IRole> roles, IUser user)
	{
		return Success(RolesGave.Format(
			roles.Join(x => x.Format()).WithBlock(),
			user.Format().WithBlock()
		));
	}

	public static AdvobotResult ModifiedColor(IRole role, Color color)
	{
		return Success(RolesModifiedColor.Format(
			role.Format().WithBlock(),
			color.RawValue.ToString("X6").WithBlock() //X6 to get hex
		));
	}

	public static AdvobotResult ModifiedHoistStatus(IRole role, bool hoisted)
	{
		return Success(RolesModifiedHoistedStatus.Format(
			role.Format().WithBlock(),
			hoisted.ToString().WithBlock()
		));
	}

	public static AdvobotResult ModifiedMentionability(IRole role, bool mentionability)
	{
		return Success(RolesModifiedMentionability.Format(
			role.Format().WithBlock(),
			mentionability.ToString().WithBlock()
		));
	}

	public static AdvobotResult ModifiedPermissions(
		IRole role,
		GuildPermission permissions,
		bool allow)
	{
		var format = allow ? RolesModifiedPermissionsAllow : RolesModifiedPermissionsDeny;
		return Success(format.Format(
			EnumUtils.GetFlagNames(permissions).Join().WithBlock(),
			role.Format().WithBlock()
		));
	}

	public static AdvobotResult Moved(IRole role, int position)
	{
		return Success(RoleMoved.Format(
			role.Format().WithBlock(),
			position.ToString().WithBlock()
		));
	}

	public static AdvobotResult Took(IReadOnlyCollection<IRole> roles, IUser user)
	{
		return Success(RolesTook.Format(
			roles.Join(x => x.Format()).WithBlock(),
			user.Format().WithBlock()
		));
	}
}