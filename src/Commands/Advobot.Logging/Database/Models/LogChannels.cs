namespace Advobot.Logging.Database.Models;

public sealed record LogChannels(
	ulong ImageLogId,
	ulong ModLogId,
	ulong ServerLogId
)
{
	public LogChannels() : this(default, default, default) { }
}