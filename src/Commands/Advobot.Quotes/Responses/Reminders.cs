using Advobot.Modules;
using Advobot.Utilities;

using static Advobot.Resources.Responses;

namespace Advobot.Quotes.Responses;

public sealed class Reminders : AdvobotResult
{
	private Reminders() : base(null, "")
	{
	}

	public static AdvobotResult AddedRemind(int minutes)
	{
		return Success(RemindersAddedRemind.Format(
			minutes.ToString().WithBlock()
		));
	}
}