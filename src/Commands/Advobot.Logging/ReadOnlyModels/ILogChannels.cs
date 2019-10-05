namespace Advobot.Logging.ReadOnlyModels
{
	public interface ILogChannels
	{
		ulong ImageLogId { get; }
		ulong ModLogId { get; }
		ulong ServerLogId { get; }
	}
}