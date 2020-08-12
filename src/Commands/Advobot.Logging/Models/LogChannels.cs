using Advobot.Logging.ReadOnlyModels;

namespace Advobot.Logging.Models
{
	public sealed class LogChannels : IReadOnlyLogChannels
	{
		public ulong ImageLogId { get; set; }
		public ulong ModLogId { get; set; }
		public ulong ServerLogId { get; set; }
	}
}