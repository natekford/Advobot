using Advobot.Modules;
using Advobot.Quotes.Database;

namespace Advobot.Quotes;

public abstract class QuoteModuleBase : AdvobotModuleBase
{
	public QuoteDatabase Db { get; set; } = null!;
}