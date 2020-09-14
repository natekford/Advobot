using System.Threading.Tasks;

using Advobot.Attributes;
using Advobot.Gacha.ParameterPreconditions;
using Advobot.Gacha.Preconditions;
using Advobot.Gacha.ReadOnlyModels;
using Advobot.Gacha.Trading;
using Advobot.Localization;
using Advobot.Resources;

using AdvorangesUtils;

using Discord.Commands;

using static Advobot.Gacha.Responses.Gacha;

namespace Advobot.Gacha.Commands
{
	[Category(nameof(Gacha))]
	[LocalizedGroup(nameof(Groups.Gacha))]
	[LocalizedAlias(nameof(Aliases.Gacha))]
	public sealed class Gacha : ModuleBase
	{
		[LocalizedGroup(nameof(Groups.Character))]
		[LocalizedAlias(nameof(Aliases.Character))]
		[Summary("temp")]
		[Meta("23e41fce-8760-4f5a-8f68-154bb8ce1bc8")]
		public sealed class GachaCharacter : GachaModuleBase
		{
			[Command(RunMode = RunMode.Async)]
			public async Task<RuntimeResult> Command(IReadOnlyCharacter character)
			{
				var display = await Displays.CreateCharacterDisplayAsync(Context.Guild, character).CAF();
				return await display.SendAsync(Context.Channel).CAF();
			}
		}

		[LocalizedGroup(nameof(Groups.Give))]
		[LocalizedAlias(nameof(Aliases.Give))]
		[Summary("temp")]
		[Meta("db62db89-d645-4bdd-9794-2945ca8dde9c")]
		[CancelPreviousTrades]
		public sealed class GachaGive : GachaExchangeModuleBase
		{
			protected override ExchangeMethod Method => ExchangeMethod.Gift;

			[Command(RunMode = RunMode.Async)]
			public Task<RuntimeResult> Command(
				[NotSelf, InGuild]
				IReadOnlyUser user,
				[OwnsCharacters]
				params IReadOnlyCharacter[] characters)
			{
				AddExchange(user, characters);
				return HandleExchange(user);
			}
		}

		[LocalizedGroup(nameof(Groups.Harem))]
		[LocalizedAlias(nameof(Aliases.Harem))]
		[Summary("temp")]
		[Meta("cdd5d2e6-e26e-4d1b-85d2-28b3778b6c2c")]
		public sealed class GachaHarem : GachaModuleBase
		{
			[Command(RunMode = RunMode.Async)]
			public async Task<RuntimeResult> Command(IReadOnlyUser user)
			{
				var display = await Displays.CreateHaremDisplayAsync(Context.Guild, user).CAF();
				return await display.SendAsync(Context.Channel).CAF();
			}
		}

		[LocalizedGroup(nameof(Groups.Roll))]
		[LocalizedAlias(nameof(Aliases.Roll))]
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

		[LocalizedGroup(nameof(Groups.Source))]
		[LocalizedAlias(nameof(Aliases.Source))]
		[Summary("temp")]
		[Meta("12827e74-4ba1-439c-9c39-9e2d2b7f2cfb")]
		public sealed class GachaSource : GachaModuleBase
		{
			[Command(RunMode = RunMode.Async)]
			public async Task<RuntimeResult> Command(IReadOnlySource source)
			{
				var display = await Displays.CreateSourceDisplayAsync(Context.Guild, source).CAF();
				return await display.SendAsync(Context.Channel).CAF();
			}
		}

		[LocalizedGroup(nameof(Groups.Trade))]
		[LocalizedAlias(nameof(Aliases.Trade))]
		[Summary("temp")]
		[Meta("dfd7e368-5a03-4af7-8054-4eb156a5e4fb")]
		[CancelPreviousTrades]
		public sealed class GachaTrade : GachaExchangeModuleBase
		{
			protected override ExchangeMethod Method => ExchangeMethod.Trade;

			[Command(RunMode = RunMode.Async)]
			public Task<RuntimeResult> Command(
				[NotSelf, InGuild]
				IReadOnlyUser user,
				[OwnsCharacters]
				params IReadOnlyCharacter[] characters
			)
			{
				var valid = AddExchange(user, characters);
				if (!valid)
				{
					return OtherSideTrade(user);
				}
				return HandleExchange(user);
			}
		}
	}
}