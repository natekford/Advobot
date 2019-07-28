using Advobot.Gacha.ReadOnlyModels;
using Advobot.Gacha.Utilities;
using System;

namespace Advobot.Gacha.Models
{
	public class Source : IReadOnlySource
	{
		public long SourceId { get; set; } = TimeUtils.UtcNowTicks;
		public string? Name { get; set; }
		public string? ThumbnailUrl { get; set; }

		public DateTime GetTimeCreated()
			=> SourceId.ToTime();
	}
}
