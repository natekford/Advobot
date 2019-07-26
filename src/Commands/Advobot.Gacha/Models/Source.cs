using Advobot.Gacha.ReadOnlyModels;
using Advobot.Gacha.Utils;

namespace Advobot.Gacha.Models
{
	public class Source : IReadOnlySource
	{
		public long SourceId { get; set; }
		public string? Name { get; set; }
		public string? ThumbnailUrl { get; set; }
		public long TimeCreated { get; set; } = TimeUtils.Now();
	}
}
