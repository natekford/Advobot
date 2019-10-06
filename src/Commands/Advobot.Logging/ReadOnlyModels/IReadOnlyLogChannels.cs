namespace Advobot.Logging.ReadOnlyModels
{
	public interface IReadOnlyLogChannels
	{
		ulong ImageLogId { get; }
		ulong ModLogId { get; }
		ulong ServerLogId { get; }
	}
}