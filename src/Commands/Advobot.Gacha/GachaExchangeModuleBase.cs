using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Advobot.Gacha.ActionLimits;
using Advobot.Gacha.ReadOnlyModels;
using Advobot.Gacha.Trading;
using Advobot.Gacha.TryParsers;
using Advobot.Interactivity;
using Advobot.Interactivity.Criterions;
using Advobot.Modules;

using AdvorangesUtils;

using Discord;
using Discord.Commands;

namespace Advobot.Gacha
{
	public abstract class GachaExchangeModuleBase : AdvobotModuleBase
	{
		protected static readonly TimeSpan Timeout = TimeSpan.FromMinutes(3);
		protected static readonly AcceptTryParser AcceptTryParser = new AcceptTryParser();

		protected abstract ExchangeMethod Method { get; }

#pragma warning disable CS8618 // Non-nullable field is uninitialized.
		public ExchangeManager Exchanges { get; set; }
		public ITokenHolderService TokenHolder { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized.

		protected bool AddExchange(IReadOnlyUser user, IEnumerable<IReadOnlyCharacter> characters)
			=> Exchanges.AddExchange(Method, user, characters);

		protected IReadOnlyList<ICriterion<IMessage>> GetUserCriteria(ulong id)
		{
			return new ICriterion<IMessage>[]
			{
				new EnsureSourceChannelCriterion(),
				new EnsureFromUserCriterion(id),
			};
		}

		protected async Task<RuntimeResult> HandleExchange(IReadOnlyUser user)
		{
			var type = Method.ToString();
			var mention = MentionUtils.MentionUser(user.UserId);
			await ReplyAsync($"{mention}, would you like to confirm the {type} y/n?").CAF();

			var options = new InteractivityOptions
			{
				Timeout = Timeout,
				Token = TokenHolder.Get(Context.User),
				Criteria = GetUserCriteria(user.UserId),
			};
			var response = await NextValueAsync(AcceptTryParser, options).CAF();
			if (response.HasTimedOut)
			{
				return Responses.Gacha.Timeout();
			}
			else if (response.HasBeenCanceled)
			{
				return AdvobotResult.IgnoreFailure;
			}
			else if (response.Value)
			{
				await Exchanges.FinalizeAsync(Context.User).CAF();
			}

			var receiver = Context.Guild.GetUser(user.UserId);
			return Responses.Gacha.Exchange(Method, Context.User, receiver, response.Value);
		}
	}
}