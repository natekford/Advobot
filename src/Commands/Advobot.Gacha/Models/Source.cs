using System;
using System.Collections.Generic;

namespace Advobot.Gacha.Models
{
	public class Source
	{
		public int SourceId { get; }

		public string Name { get; } = "";

		public IList<Character> Characters { get; set; } = Array.Empty<Character>();
	}
}
