using Advobot.Attributes;
using Advobot.AutoMod;
using Advobot.AutoMod.Database.Models;
using Advobot.Modules;
using Advobot.ParameterPreconditions.Discord.Users;
using Advobot.Preconditions;
using Advobot.Preconditions.Permissions;
using Advobot.Resources;
using Advobot.Utilities;

using Discord;

using YACCS.Commands.Attributes;
using YACCS.Commands.Models;
using YACCS.Localization;
using YACCS.Results;

namespace Advobot.MyCommands.Commands;

[LocalizedCategory(nameof(Names.MyCommandsCategory))]
public sealed class MyCommands
{
	[Command(nameof(Names.Spanitch))]
	[LocalizedSummary(nameof(Summaries.SpanitchSummary))]
	[Meta("0c96c96b-5d11-41cd-941b-8864b7542349", IsEnabled = true)]
	[RequireGuildPermissionsOrMickezoor(GuildPermission.ManageRoles)]
	[RequireGuild(199339772118827008)]
	[SpanitchRolesExist]
	public sealed class Spanitch : AutoModModuleBase
	{
		private const ulong MIJE_ID = 107770708142067712;
		private const ulong MUTE_ID = 328101628664086528;
		private const ulong SPAN_ID = 741058143450300487;

		private RequestOptions Options => GetOptions("spanitch");
		private IRole[] Roles => field ??=
		[
			Context.Guild.GetRole(MUTE_ID),
			Context.Guild.GetRole(SPAN_ID)
		];

		[Command]
		public async Task<AdvobotResult> Give([CanModifyUser] IGuildUser user)
		{
			await user.ModifyRolesAsync(
				rolesToAdd: Roles,
				rolesToRemove: [],
				Options
			).ConfigureAwait(false);
			return AdvobotResult.Success("they have been spanitched");
		}

		[Command(nameof(Names.Hard))]
		[LocalizedSummary(nameof(Summaries.SpanitchHardSummary))]
		[Priority(1)]
		public async Task<AdvobotResult> Permanent([CanModifyUser] IGuildUser user)
		{
			await Give(user).ConfigureAwait(false);
			return await Permanent(user.Id).ConfigureAwait(false);
		}

		[Command(nameof(Names.Hard))]
		[LocalizedSummary(nameof(Summaries.SpanitchHardSummary))]
		[Priority(0)]
		public async Task<AdvobotResult> Permanent(ulong user)
		{
			foreach (var pRole in CreatePersistentRoles(user))
			{
				await Db.AddPersistentRoleAsync(pRole).ConfigureAwait(false);
			}

			return AdvobotResult.Success("they have been spanitched hard");
		}

		[Command(nameof(Names.Unspanitch))]
		[LocalizedSummary(nameof(Summaries.UnspanitchSummary))]
		public async Task<AdvobotResult> Remove([CanModifyUser] IGuildUser user)
		{
			await user.ModifyRolesAsync(
				rolesToAdd: [],
				rolesToRemove: Roles,
				Options
			).ConfigureAwait(false);

			foreach (var pRole in CreatePersistentRoles(user.Id))
			{
				await Db.DeletePersistentRoleAsync(pRole).ConfigureAwait(false);
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

		public sealed class RequireGuildPermissionsOrMickezoor(params GuildPermission[] permissions)
			: RequireGuildPermissions(permissions)
		{
			public override string Summary => base.Summary + " or you are Mickezoor";

			public override async ValueTask<IResult> CheckAsync(
				IImmutableCommand command,
				IGuildContext context)
			{
				var result = await base.CheckAsync(command, context).ConfigureAwait(false);
				if (result.IsSuccess)
				{
					return result;
				}
				if (context.User.Id == MIJE_ID)
				{
					return Result.EmptySuccess;
				}
				return result;
			}
		}

		public sealed class SpanitchRolesExist : AdvobotPrecondition
		{
			public override string Summary => "spanitch role and mute role must exist.";

			public override ValueTask<IResult> CheckAsync(
				IImmutableCommand command,
				IGuildContext context)
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

				if (!hasMute && !hasSpan)
				{
					return new(Result.Failure("both roles don't exist anymore"));
				}
				else if (!hasMute)
				{
					return new(Result.Failure("mute role doesn't exist anymore"));
				}
				else if (!hasSpan)
				{
					return new(Result.Failure("spanitch role doesn't exist anymore"));
				}
				else
				{
					return new(Result.EmptySuccess);
				}
			}
		}
	}
}