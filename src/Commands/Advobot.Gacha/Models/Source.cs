
using Advobot.Gacha.Relationships;
using Advobot.Gacha.Utilities;

namespace Advobot.Gacha.Models
{
	public record Source(
		string Name,
		long SourceId,
		string? ThumbnailUrl
	) : ITimeCreated
	{
		public Source() : this("", SourceId: TimeUtils.UtcNowTicks, default) { }

		public DateTimeOffset GetTimeCreated()
			=> SourceId.ToTime();
	}
}