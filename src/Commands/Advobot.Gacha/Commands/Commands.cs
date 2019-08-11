using System;
using System.Linq;
using System.Threading.Tasks;
using Advobot.Attributes;
using Advobot.Gacha.Displays;
using Advobot.Gacha.Models;
using Advobot.Gacha.ParameterPreconditions;
using Advobot.Gacha.Trading;
using AdvorangesUtils;
using Discord.Commands;

namespace Advobot.Gacha.Commands
{
	public sealed class Gacha : ModuleBase
	{
		[Group(nameof(GachaRoll)), ModuleInitialismAlias(typeof(GachaRoll))]
		[Summary("temp")]
		[CommandMeta("ea1f45fd-d9e1-43df-bd9b-46c31b4ec221")]
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
		[CommandMeta("23e41fce-8760-4f5a-8f68-154bb8ce1bc8")]
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
		[CommandMeta("12827e74-4ba1-439c-9c39-9e2d2b7f2cfb")]
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
		[CommandMeta("cdd5d2e6-e26e-4d1b-85d2-28b3778b6c2c")]
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
		[CommandMeta("dfd7e368-5a03-4af7-8054-4eb156a5e4fb")]
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
		[CommandMeta("db62db89-d645-4bdd-9794-2945ca8dde9c")]
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
