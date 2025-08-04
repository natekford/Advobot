using Advobot.Attributes;
using Advobot.AutoMod;
using Advobot.AutoMod.Models;
using Advobot.Modules;
using Advobot.ParameterPreconditions.DiscordObjectValidation.Users;
using Advobot.Preconditions;
using Advobot.Preconditions.Permissions;
using Advobot.Utilities;

using AdvorangesUtils;

using Discord;
using Discord.Commands;

namespace Advobot.MyCommands.Commands;

[Category("spanitch")]
[Group("spanitch")]
[Summary("spanitches a user")]
[Meta("0c96c96b-5d11-41cd-941b-8864b7542349", IsEnabled = true)]
[RequireGuildPermissionsOrMickezoor(GuildPermission.ManageRoles)]
[RequireGuild(199339772118827008)]
[SpanitchRolesExist]
public sealed class SpanitchModule : AutoModModuleBase
{
	private const ulong MIJE_ID = 107770708142067712;
	private const ulong MUTE_ID = 328101628664086528;
	private const ulong SPAN_ID = 741058143450300487;
	private IRole[]? _Roles;

	private RequestOptions Options => GetOptions("spanitch");
	private IRole[] Roles => _Roles ??=
	[
		Context.Guild.GetRole(MUTE_ID),
		Context.Guild.GetRole(SPAN_ID)
	];

	[Command]
	public async Task<RuntimeResult> Command([CanModifyUser] IGuildUser user)
	{
		await user.ModifyRolesAsync(
			rolesToAdd: Roles,
			rolesToRemove: [],
			Options
		).CAF();
		return AdvobotResult.Success("they have been spanitched");
	}

	[Command("hard")]
	[Summary("makes it so if they leave the server and rejoin they are still spanitched")]
	[Priority(1)]
	public async Task<RuntimeResult> Hard([CanModifyUser] IGuildUser user)
	{
		await Command(user).CAF();
		return await Hard(user.Id).CAF();
	}

	[Command("hard")]
	[Summary("makes it so if they leave the server and rejoin they are still spanitched")]
	[Priority(0)]
	public async Task<RuntimeResult> Hard(ulong user)
	{
		foreach (var pRole in CreatePersistentRoles(user))
		{
			await Db.AddPersistentRoleAsync(pRole).CAF();
		}

		return AdvobotResult.Success("they have been spanitched hard");
	}

	[Command("unspanitch")]
	[Summary("unspanitches a user")]
	public async Task<RuntimeResult> Unspanitch([CanModifyUser] IGuildUser user)
	{
		await user.ModifyRolesAsync(
			rolesToAdd: [],
			rolesToRemove: Roles,
			Options
		).CAF();

		foreach (var pRole in CreatePersistentRoles(user.Id))
		{
			await Db.DeletePersistentRoleAsync(pRole).CAF();
		}

		return AdvobotResult.Success("they have been unspanitched");
	}

	private PersistentRole[] CreatePersistentRoles(ulong user)
	{
		return Array.ConvertAll(Roles, x =>
		{
			return new PersistentRole
			{
				GuildId = Context.Guild.Id,
				UserId = user,
				RoleId = x.Id,
			};
		});
	}

	public sealed class RequireGuildPermissionsOrMickezoor(params GuildPermission[] permissions) : RequireGuildPermissions(permissions)
	{
		public override string Summary => base.Summary + " or you are Mickezoor";

		public override async Task<PreconditionResult> CheckPermissionsAsync(
			ICommandContext context,
			CommandInfo command,
			IServiceProvider services)
		{
			var result = await base.CheckPermissionsAsync(context, command, services).CAF();
			if (result.IsSuccess)
			{
				return result;
			}
			if (context.User.Id == MIJE_ID)
			{
				return PreconditionResult.FromSuccess();
			}
			return result;
		}
	}

	public sealed class SpanitchRolesExist : PreconditionAttribute
	{
		public override Task<PreconditionResult> CheckPermissionsAsync(
			ICommandContext context,
			CommandInfo command,
			IServiceProvider services)
		{
			var hasMute = false;
			var hasSpan = false;
			foreach (var role in context.Guild.Roles)
			{
				if (role.Id == MUTE_ID)
				{
					hasMute = true;
				}
				else if (role.Id == SPAN_ID)
				{
					hasSpan = true;
				}

				if (hasMute && hasSpan)
				{
					break;
				}
			}

			if (hasMute && hasSpan)
			{
				return this.FromSuccess().AsTask();
			}
			return PreconditionResult.FromError("one of the roles doesn't exist anymore.").AsTask();
		}
	}
}