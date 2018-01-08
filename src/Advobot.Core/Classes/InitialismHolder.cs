using Advobot.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Advobot.Core.Classes
{
	/// <summary>
	/// Creates an initialism out of the passed in name. Keeps track of the parts and original.
	/// </summary>
	public class InitialismHolder
	{
		private static Dictionary<string, string> _ShortenedPhrases = new Dictionary<string, string>
		{
			{ "clear", "clr" },
		};

		public string Original { get; private set; }
		public ImmutableList<string> Parts { get; private set; }
		public string Initialism { get; private set; }
		private string[] _OtherAliases;
		public string[] Aliases => _OtherAliases.Concat(new[] { Initialism }).ToArray();

		public InitialismHolder(string name, string[] otherAliases, bool topLevel)
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
			Initialism = initialism.ToString().ToLower();
			_OtherAliases = otherAliases;
		}

		public void AppendToInitialismByPart(int index, int length)
		{
			var newInitialism = new StringBuilder();
			for (int i = 0; i < Parts.Count; ++i)
			{
				var p = Parts[i];
				var l = i == index ? length : 1;
				newInitialism.Append(p.Substring(0, l));
			}
			Initialism = newInitialism.ToString().ToLower();
		}

		public override string ToString() => Initialism;
	}
}
