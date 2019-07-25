using Advobot.Gacha.ReadOnlyModels;
using Advobot.Gacha.Utils;
using System.Collections.Generic;

namespace Advobot.Gacha.Models
{
	public class Source : IReadOnlySource
	{
		public int SourceId { get; set; }
		public string Name { get; set; }
		public string? ThumbnailUrl { get; set; }
		public IList<Character> Characters { get; set; } = new List<Character>();

		public long TimeCreated { get; set; } = TimeUtils.Now();

		IReadOnlyList<IReadOnlyCharacter> IReadOnlySource.Characters => (IReadOnlyList<IReadOnlyCharacter>)Characters;
	}
}
