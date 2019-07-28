using Advobot.Classes.Attributes;
using Advobot.Gacha.Displays;
using Advobot.Gacha.Models;
using Advobot.Gacha.ParameterPreconditions;
using Advobot.Gacha.Trading;
using AdvorangesUtils;
using Discord.Commands;
using System;
using System.Linq;
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
			public async Task Command()
			{
				var checker = Checkers.GetClaimChecker(Context.Guild);
				var character = await Database.GetUnclaimedCharacter(Context.Guild.Id).CAF();
				var source = await Database.GetSourceAsync(character.SourceId).CAF();
				var wishes = await Database.GetWishesAsync(Context.Guild.Id, character).CAF();
				var images = await Database.GetImagesAsync(character).CAF();
				var display = new RollDisplay(Context.Client, Database, checker, character, source, wishes, images);
				await display.SendAsync(Context.Channel).CAF();
			}
		}

		[Group(nameof(DisplayCharacter)), ModuleInitialismAlias(typeof(DisplayCharacter))]
		[Summary("temp")]
		[EnabledByDefault(true)]
		public sealed class DisplayCharacter : GachaModuleBase
		{
			[Command(RunMode = RunMode.Async)]
			public async Task Command(Character character)
			{
				var metadata = await Database.GetCharacterMetadataAsync(character).CAF();
				var images = await Database.GetImagesAsync(character).CAF();
				var claim = await Database.GetClaimAsync(Context.Guild.Id, character).CAF();
				var display = new CharacterDisplay(Context.Client, Database, metadata, images, claim);
				await display.SendAsync(Context.Channel).CAF();
			}
		}

		[Group(nameof(DisplaySource)), ModuleInitialismAlias(typeof(DisplaySource))]
		[Summary("temp")]
		[EnabledByDefault(true)]
		public sealed class DisplaySource : GachaModuleBase
		{
			[Command(RunMode = RunMode.Async)]
			public async Task Command(Source source)
			{
				var characters = await Database.GetCharactersAsync(source).CAF();
				var display = new SourceDisplay(Context.Client, Database, source, characters);
				await display.SendAsync(Context.Channel).CAF();
			}
		}

		[Group(nameof(DisplayHarem)), ModuleInitialismAlias(typeof(DisplayHarem))]
		[Summary("temp")]
		[EnabledByDefault(true)]
		public sealed class DisplayHarem : GachaModuleBase
		{
			[Command(RunMode = RunMode.Async)]
			public async Task Command(User user)
			{
				var marriages = await Database.GetClaimsAsync(user).CAF();
				var display = new HaremDisplay(Context.Client, Database, marriages);
				await display.SendAsync(Context.Channel).CAF();
			}
		}

		[Group(nameof(GachaTrade)), ModuleInitialismAlias(typeof(GachaTrade))]
		[Summary("temp")]
		[EnabledByDefault(true)]
		public sealed class GachaTrade : GachaModuleBase
		{
			[Command(RunMode = RunMode.Async)]
			public Task Command([NotSelf] User user, [OwnsCharacters] params Character[] characters)
			{
				throw new NotImplementedException();
			}
		}

		[Group(nameof(GachaGive)), ModuleInitialismAlias(typeof(GachaGive))]
		[Summary("temp")]
		[EnabledByDefault(true)]
		public sealed class GachaGive : GachaModuleBase
		{
			[Command(RunMode = RunMode.Async)]
			public Task Command([NotSelf] User user, [OwnsCharacters] params Character[] characters)
			{
				var trades = new TradeCollection(Context.Guild);
				trades.AddRange(characters.Select(x => new Trade(user, x)));

				throw new NotImplementedException();
			}
		}
	}
}
