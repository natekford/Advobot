using Advobot.Classes.Attributes;
using Advobot.Gacha.Models;
using Discord.Commands;
using System.Threading.Tasks;

namespace Advobot.Gacha.Commands
{
	public sealed class Gacha : ModuleBase
	{
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
