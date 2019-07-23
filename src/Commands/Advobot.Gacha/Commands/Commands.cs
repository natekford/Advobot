using Advobot.Classes.Attributes;
using Advobot.Classes.Attributes.Preconditions.Permissions;
using Advobot.Classes.Modules;
using Advobot.Classes.Results;
using Advobot.Gacha.Database;
using Advobot.Gacha.Models;
using Discord;
using Discord.Commands;
using System.Threading.Tasks;

namespace Advobot.Gacha.Commands
{
	public sealed class Gacha : ModuleBase
	{
		[Group(nameof(SeedGachaData)), ModuleInitialismAlias(typeof(SeedGachaData))]
		[Summary("temp")]
		[UserPermissionRequirement(GuildPermission.Administrator)]
		[EnabledByDefault(true)]
		public sealed class SeedGachaData : AdvobotModuleBase
		{
#pragma warning disable CS8618 // Non-nullable field is uninitialized.
			public GachaDatabase Database { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized.

			[Command]
			public Task<RuntimeResult> Command()
			{
				var char1 = new Character
				{

				};
				return AdvobotResult.Success("Successfully seeded gacha data.");
			}
		}

		[Group(nameof(GachaRoll)), ModuleInitialismAlias(typeof(GachaRoll))]
		[Summary("temp")]
		[EnabledByDefault(true)]
		public sealed class GachaRoll : AdvobotModuleBase
		{
#pragma warning disable CS8618 // Non-nullable field is uninitialized.
			public GachaDatabase Database { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized.

			[Command]
			public Task<RuntimeResult> Command()
			{
				return AdvobotResult.Success("Successfully seeded gacha data.");
			}
		}

		public sealed class GetCharacterImages : AdvobotModuleBase
		{
			[Command]
			public Task<RuntimeResult> Command(Character character)
			{
				return AdvobotResult.Success("Successfully seeded gacha data.");
			}
		}

		public sealed class GachaTrade : AdvobotModuleBase
		{
#pragma warning disable CS8618 // Non-nullable field is uninitialized.
			public GachaDatabase Database { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized.

			public Task Command(IGuildUser user, params Character[] characters)
			{

			}
		}

		public sealed class GachaGive : AdvobotModuleBase
		{
#pragma warning disable CS8618 // Non-nullable field is uninitialized.
			public GachaDatabase Database { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized.

			public Task Command(IGuildUser user, params Character[] characters)
			{

			}
		}
	}
}
