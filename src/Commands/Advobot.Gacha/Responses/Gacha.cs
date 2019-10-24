using System;
using System.Collections.Generic;
using System.Text;
using Advobot.Gacha.Trading;
using Advobot.Modules;

namespace Advobot.Gacha.Responses
{
	public sealed class Gacha : AdvobotResult
	{
		private Gacha() : base(null, "")
		{
		}

		public static AdvobotResult GiveAccepted(TradeCollection trade)
			=> Success("meme" + trade.ToString());

		public static AdvobotResult GiveRejected(TradeCollection trade)
			=> Success("not meme" + trade.ToString());

		public static AdvobotResult Timeout()
			=> Success("Out of time.");
	}
}