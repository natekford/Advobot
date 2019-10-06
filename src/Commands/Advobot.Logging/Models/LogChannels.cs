using Advobot.Logging.ReadOnlyModels;

namespace Advobot.Logging.Models
{
	public sealed class LogChannels : IReadOnlyLogChannels
	{
		public string? ImageLogId { get; set; }
		public string? ModLogId { get; set; }
		public string? ServerLogId { get; set; }
		ulong IReadOnlyLogChannels.ImageLogId => Parse(ImageLogId);
		ulong IReadOnlyLogChannels.ModLogId => Parse(ModLogId);
		ulong IReadOnlyLogChannels.ServerLogId => Parse(ServerLogId);

		private ulong Parse(string? value)
		{
			if (value == null)
			{
				return 0;
			}
			return ulong.Parse(value);
		}
	}
}