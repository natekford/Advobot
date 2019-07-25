using Advobot.Classes.Attributes;
using Advobot.Classes.Attributes.ParameterPreconditions.NumberValidation;
using Advobot.Classes.Attributes.Preconditions.Permissions;
using Advobot.Classes.Results;
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
		public sealed class SeedGachaData : GachaModuleBase
		{
			[Command(RunMode = RunMode.Async)]
			public async Task<RuntimeResult> Command([ValidatePositiveNumber] int amt)
			{
				await Task.Yield();
				return AdvobotResult.Ignore;
				/*
				var source = new Source
				{
					Name = Guid.NewGuid().ToString(),
				};
				for (var i = 0; i < amt; ++i)
				{
					var character = new Character
					{
						IsFakeCharacter = true,
						Name = Guid.NewGuid().ToString(),
						GenderIcon = "gender icon",
						Source = source,
					};
					character.Images.AddRange(new[]
					{
						new Image
						{
							Character = character,
							Url = "https://cdn.discordapp.com/attachments/367092372636434443/597957772763594772/8ds6xte9dz831.jpg",
						},
						new Image
						{
							Character = character,
							Url = "https://cdn.discordapp.com/attachments/367092372636434443/597957777599496202/hgs5xuhnf2931.png",
						}
					});
					source.Characters.Add(character);
				}
				await Database.AddAndSaveAsync(source).CAF();
				return AdvobotResult.Success($"Successfully added {amt} fake characters.");*/
			}
		}

		[Group(nameof(GachaRoll)), ModuleInitialismAlias(typeof(GachaRoll))]
		[Summary("temp")]
		[EnabledByDefault(true)]
		public sealed class GachaRoll : GachaModuleBase
		{
			[Command(RunMode = RunMode.Async)]
			public Task Command()
				=> CreateRollDisplayAsync();
		}

		[Group(nameof(DisplayCharacter)), ModuleInitialismAlias(typeof(DisplayCharacter))]
		[Summary("temp")]
		[EnabledByDefault(true)]
		public sealed class DisplayCharacter : GachaModuleBase
		{
			[Command(RunMode = RunMode.Async)]
			public Task Command(Character character)
				=> CreateCharacterDisplayAsync(character);
		}

		[Group(nameof(DisplaySource)), ModuleInitialismAlias(typeof(DisplaySource))]
		[Summary("temp")]
		[EnabledByDefault(true)]
		public sealed class DisplaySource : GachaModuleBase
		{
			[Command(RunMode = RunMode.Async)]
			public Task Command(Source source)
				=> CreateSourceDisplayAsync(source);
		}

		[Group(nameof(DisplayHarem)), ModuleInitialismAlias(typeof(DisplayHarem))]
		[Summary("temp")]
		[EnabledByDefault(true)]
		public sealed class DisplayHarem : GachaModuleBase
		{
			[Command(RunMode = RunMode.Async)]
			public Task Command(User user)
				=> CreateHaremDisplayAsync(user);
		}

		/*
		public sealed class GachaTrade : GachaModuleBase
		{
			public Task Command(User user, params Character[] characters)
			{

			}
		}

		public sealed class GachaGive : GachaModuleBase
		{
			public Task Command(User user, params Character[] characters)
			{

			}
		}*/
	}
}
