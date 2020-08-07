using Discord;

namespace Advobot.AutoMod.Context
{
	public interface IAutoModMessageContext : IAutoModContext
	{
		public ITextChannel Channel { get; }
		public IUserMessage Message { get; }
	}
}