using Advobot.Gacha.ActionLimits;
using Advobot.Gacha.Models;
using Advobot.Gacha.Trading;
using Advobot.Gacha.TryParsers;
using Advobot.Interactivity;
using Advobot.Interactivity.Criterions;
using Advobot.Modules;

using AdvorangesUtils;

using Discord;
using Discord.Commands;

namespace Advobot.Gacha;

public abstract class GachaExchangeModuleBase : AdvobotModuleBase
{
	protected static readonly AcceptTryParser AcceptTryParser = new();
	protected static readonly TimeSpan Timeout = TimeSpan.FromMinutes(3);

	public ExchangeManager Exchanges { get; set; } = null!;
	public ITokenHolderService TokenHolder { get; set; } = null!;
	protected abstract ExchangeMethod Method { get; }

	protected bool AddExchange(User user, IEnumerable<Character> characters)
		=> Exchanges.AddExchange(Method, user, characters);

	protected IReadOnlyList<ICriterion<IMessage>> GetUserCriteria(ulong id)
	{
		return
		[
				new EnsureSourceChannelCriterion(),
				new EnsureFromUserCriterion(id),
		];
	}

	protected async Task<RuntimeResult> HandleExchange(User user)
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