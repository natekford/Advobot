using Advobot.Logging.Relationships;

namespace Advobot.Logging.ReadOnlyModels
{
	public interface IReadOnlyCustomNotification : IReadOnlyCustomEmbed, IChannelChild
	{
		string? Content { get; }
	}
}