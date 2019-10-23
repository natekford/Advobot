using Advobot.Logging.ReadOnlyModels;
using Advobot.Logging.Utilities;

namespace Advobot.Logging.Models
{
	public sealed class LogChannels : IReadOnlyLogChannels
	{
		public string? ImageLogId { get; set; }
		public string? ModLogId { get; set; }
		public string? ServerLogId { get; set; }
		ulong IReadOnlyLogChannels.ImageLogId => ImageLogId.ToId();
		ulong IReadOnlyLogChannels.ModLogId => ModLogId.ToId();
		ulong IReadOnlyLogChannels.ServerLogId => ServerLogId.ToId();
	}
}