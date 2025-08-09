using Advobot.Embeds;
using Advobot.Modules;
using Advobot.Utilities;

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
			permissions.ToString("F").WithBlock(),
			input.Format().WithBlock(),
			output.Format().WithBlock()
		));
	}

	public static AdvobotResult Display(IEnumerable<IRole> roles)
	{
		var description = roles
			.Select(x => $"{x.Position:00}. {x.Name}")
			.Join(Environment.NewLine)
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
			.ToDictionary(
				keySelector: x => x.ToString(),
				elementSelector: x => role.Permissions.Has(x) ? PermValue.Allow : PermValue.Deny
			)
			.FormatPermissionList()
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
			roles.Select(x => x.Format()).Join().WithBlock(),
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
			permissions.ToString("F").WithBlock(),
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
			roles.Select(x => x.Format()).Join().WithBlock(),
			user.Format().WithBlock()
		));
	}
}