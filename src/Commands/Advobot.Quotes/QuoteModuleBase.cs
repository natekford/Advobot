using Advobot.Modules;
using Advobot.Quotes.Database;

namespace Advobot.Quotes
{
	public abstract class QuoteModuleBase : AdvobotModuleBase
	{
#pragma warning disable CS8618 // Non-nullable field is uninitialized.
		public QuoteDatabase Db { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized.
	}
}