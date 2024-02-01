using Advobot.Modules;
using Advobot.Quotes.Database;

namespace Advobot.Quotes;

public abstract class RuleModuleBase : AdvobotModuleBase
{
	public RuleDatabase Db { get; set; } = null!;
}