using System;
using System.Linq;
using System.Threading.Tasks;

using Advobot.Attributes;
using Advobot.Gacha.Localization;
using Advobot.Gacha.ParameterPreconditions;
using Advobot.Gacha.ReadOnlyModels;
using Advobot.Gacha.Resources;
using Advobot.Gacha.Trading;
using Advobot.Gacha.Utilities;
using Advobot.Interactivity;
using Advobot.Interactivity.Criterions;
using Advobot.Interactivity.TryParsers;
using Advobot.Modules;
using AdvorangesUtils;

using Discord;
using Discord.Commands;

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
		public sealed class GachaGive : GachaModuleBase
		{
			private static readonly TimeSpan _Timeout = TimeSpan.FromMinutes(3);

			[Command(RunMode = RunMode.Async)]
			public async Task<RuntimeResult> Command(
				[NotSelf, InGuild]
				IReadOnlyUser user,
				[OwnsCharacters]
				params IReadOnlyCharacter[] characters)
			{
				var trades = new TradeCollection(Context.Guild);
				trades.AddRange(characters.Select(x => new Trade(user, x)));

				//TODO: reset when user reinvokes command
				var criteria = new ICriterion<IMessage>[]
				{
					new EnsureSourceChannelCriterion(),
					new EnsureFromUserCriterion(user.UserId),
				};

				var tryParser = new AcceptTryParser();

				var options = new InteractivityOptions
				{
					Timeout = _Timeout,
					Token = default,
				};

				var response = await NextValueAsync(criteria, tryParser, options).CAF();
				if (response.HasBeenCanceled)
				{
					return AdvobotResult.IgnoreFailure;
				}
				else if (response.HasTimedOut)
				{
					return Responses.Gacha.Timeout();
				}
				else if (response.HasValue && response.Value)
				{
					return Responses.Gacha.GiveAccepted(trades);
				}
				return Responses.Gacha.GiveRejected(trades);
			}

			public sealed class AcceptTryParser : IMessageTryParser<bool>
			{
				public ValueTask<Optional<bool>> TryParseAsync(IMessage message)
				{
					if (message.Content.CaseInsEquals("y"))
					{
						return new ValueTask<Optional<bool>>(true);
					}
					else if (message.Content.CaseInsEquals("n"))
					{
						return new ValueTask<Optional<bool>>(false);
					}
					return new ValueTask<Optional<bool>>(Optional<bool>.Unspecified);
				}
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
		public sealed class GachaTrade : GachaModuleBase
		{
			[Command(RunMode = RunMode.Async)]
			public Task Command(
				[NotSelf]
				IReadOnlyUser user,
				[OwnsCharacters]
				params IReadOnlyCharacter[] characters
			)
			{
				var trades = new TradeCollection(Context.Guild);
				trades.AddRange(characters.Select(x => new Trade(user, x)));

				throw new NotImplementedException();
			}
		}
	}
}