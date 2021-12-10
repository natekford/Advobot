namespace Advobot.Logging.Models;

public sealed record LogChannels(
	ulong ImageLogId,
	ulong ModLogId,
	ulong ServerLogId
)
{
	public LogChannels() : this(default, default, default) { }
}