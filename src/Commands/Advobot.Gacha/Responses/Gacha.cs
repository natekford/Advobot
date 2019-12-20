using System.Collections.Generic;

using Advobot.Gacha.ReadOnlyModels;
using Advobot.Gacha.Trading;
using Advobot.Modules;
using Advobot.Utilities;

using Discord;

namespace Advobot.Gacha.Responses
{
	public sealed class Gacha : AdvobotResult
	{
		private Gacha() : base(null, "")
		{
		}

		public static AdvobotResult Exchange(
			ExchangeMethod method,
			IGuildUser user,
			IGuildUser receiver,
			bool accepted)
		{
			var type = method.ToString();
			var action = accepted ? "accepted" : "rejected";
			return Success($"{user.Format()}, {receiver.Format()} {action} your {type}.");
		}

		public static AdvobotResult OtherSideTrade(IReadOnlyUser user)
		{
			var mention = MentionUtils.MentionUser(user.UserId);
			return Success($"{mention}, use the trade command to offer up your side of the trade.");
		}

		public static AdvobotResult Timeout()
			=> Success("Time has ran out.");

		public static AdvobotResult Trade(
			IGuildUser user,
			IGuildUser receiver,
			bool accepted)
		{
			var action = accepted ? "accepted" : "rejected";
			return Success($"The trade between {user.Format()} and {receiver.Format()} has been {action}.");
		}
	}
}