using Discord;

namespace Advobot.AutoMod.Context
{
	public interface IAutoModContext
	{
		public IGuild Guild { get; }
		public IGuildUser User { get; }
	}
}