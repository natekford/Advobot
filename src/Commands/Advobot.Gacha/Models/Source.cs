using System;

using Advobot.Gacha.ReadOnlyModels;
using Advobot.Gacha.Utilities;

namespace Advobot.Gacha.Models
{
	public class Source : IReadOnlySource
	{
		public string? Name { get; set; }
		public long SourceId { get; set; } = TimeUtils.UtcNowTicks;
		public string? ThumbnailUrl { get; set; }

		public DateTime GetTimeCreated()
			=> SourceId.ToTime();
	}
}