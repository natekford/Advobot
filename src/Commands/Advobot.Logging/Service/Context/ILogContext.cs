using Discord;

namespace Advobot.Logging.Service.Context;

public interface ILogContext
{
	IGuildUser Bot { get; }
	IGuild Guild { get; }
	ITextChannel? ImageLog { get; }
	ITextChannel? ModLog { get; }
	ITextChannel? ServerLog { get; }
}