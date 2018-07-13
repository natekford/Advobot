using AdvorangesUtils;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Advobot.Classes
{
	/// <summary>
	/// Creates an initialism out of the passed in name. Keeps track of the parts and original.
	/// </summary>
	public sealed class Initialism
	{
		private static Dictionary<string, string> _ShortenedPhrases = new Dictionary<string, string>
		{
			{ "clear", "clr" }
		};

		/// <summary>
		/// The original supplied name.
		/// </summary>
		public string Original { get; }
		/// <summary>
		/// The edited name which has been checked to remove any duplicate conflicts with other initialisms.
		/// </summary>
		public string Edited { get; internal set; }
		/// <summary>
		/// The parts of the original.
		/// </summary>
		public ImmutableList<string> Parts { get; }
		/// <summary>
		/// The other supplied aliases plus the edited initialism.
		/// </summary>
		public ImmutableList<string> Aliases => _OtherAliases.Concat(new[] { Edited }).ToImmutableList();

		private string[] _OtherAliases;

		internal Initialism(string name, string[] otherAliases, bool topLevel)
		{
			var edittingName = name;
			var parts = new List<StringBuilder>();
			var initialism = new StringBuilder();

			if (topLevel)
			{
				foreach (var kvp in _ShortenedPhrases)
				{
					edittingName = edittingName.CaseInsReplace(kvp.Key, kvp.Value.ToUpper());
				}
				if (name.EndsWith("s"))
				{
					edittingName = edittingName.Substring(0, edittingName.Length - 1) + "S";
				}
			}

			foreach (var c in edittingName)
			{
				if (Char.IsUpper(c))
				{
					initialism.Append(c);
					//ToString HAS to be called here or else it uses the capacity int constructor
					parts.Add(new StringBuilder(c.ToString()));
				}
				else
				{
					parts[parts.Count - 1].Append(c);
				}
			}

			Original = name;
			Parts = parts.Select(x => x.ToString()).ToImmutableList();
			Edited = initialism.ToString().ToLower();
			_OtherAliases = otherAliases;
		}
	}
}
