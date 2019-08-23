using System;
using System.Linq;
using System.Threading.Tasks;
using Advobot.Attributes;
using Advobot.Gacha.Models;
using Advobot.Gacha.ParameterPreconditions;
using Advobot.Gacha.Trading;
using AdvorangesUtils;
using Discord.Commands;

namespace Advobot.Gacha.Commands
{
	[Category(nameof(Gacha))]
	public sealed class Gacha : ModuleBase
	{
		[Group(nameof(GachaRoll)), ModuleInitialismAlias(typeof(GachaRoll))]
		[Summary("temp")]
		[Meta("ea1f45fd-d9e1-43df-bd9b-46c31b4ec221")]
		public sealed class GachaRoll : GachaModuleBase
		{
			[Command(RunMode = RunMode.Async)]
			public async Task<RuntimeResult> Command()
			{
				var display = await Displays.CreateRollDisplayAsync(Context.Guild).CAF();
				return await display.SendAsync(Context.Channel).CAF();
			}
		}

		[Group(nameof(DisplayCharacter)), ModuleInitialismAlias(typeof(DisplayCharacter))]
		[Summary("temp")]
		[Meta("23e41fce-8760-4f5a-8f68-154bb8ce1bc8")]
		public sealed class DisplayCharacter : GachaModuleBase
		{
			[Command(RunMode = RunMode.Async)]
			public async Task<RuntimeResult> Command(Character character)
			{
				var display = await Displays.CreateCharacterDisplayAsync(Context.Guild, character).CAF();
				return await display.SendAsync(Context.Channel).CAF();
			}
		}

		[Group(nameof(DisplaySource)), ModuleInitialismAlias(typeof(DisplaySource))]
		[Summary("temp")]
		[Meta("12827e74-4ba1-439c-9c39-9e2d2b7f2cfb")]
		public sealed class DisplaySource : GachaModuleBase
		{
			[Command(RunMode = RunMode.Async)]
			public async Task<RuntimeResult> Command(Source source)
			{
				var display = await Displays.CreateSourceDisplayAsync(Context.Guild, source).CAF();
				return await display.SendAsync(Context.Channel).CAF();
			}
		}

		[Group(nameof(DisplayHarem)), ModuleInitialismAlias(typeof(DisplayHarem))]
		[Summary("temp")]
		[Meta("cdd5d2e6-e26e-4d1b-85d2-28b3778b6c2c")]
		public sealed class DisplayHarem : GachaModuleBase
		{
			[Command(RunMode = RunMode.Async)]
			public async Task<RuntimeResult> Command(User user)
			{
				var display = await Displays.CreateHaremDisplayAsync(Context.Guild, user).CAF();
				return await display.SendAsync(Context.Channel).CAF();
			}
		}

		[Group(nameof(GachaTrade)), ModuleInitialismAlias(typeof(GachaTrade))]
		[Summary("temp")]
		[Meta("dfd7e368-5a03-4af7-8054-4eb156a5e4fb")]
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
		[Meta("db62db89-d645-4bdd-9794-2945ca8dde9c")]
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
