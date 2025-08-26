using Advobot.Embeds;
using Advobot.Modules;
using Advobot.Utilities;

using Discord;

using static Advobot.Resources.Responses;

namespace Advobot.Standard.Responses;

public sealed class Roles : AdvobotResult
{
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

	public static AdvobotResult Moved(IRole role, int position)
	{
		return Success(RoleMoved.Format(
			role.Format().WithBlock(),
			position.ToString().WithBlock()
		));
	}
}