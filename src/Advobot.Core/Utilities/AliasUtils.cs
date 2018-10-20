using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using AdvorangesUtils;

namespace Advobot.Utilities
{
	internal static class AliasUtils
	{
		private static Dictionary<string, Type> _AlreadyUsed { get; } = new Dictionary<string, Type>();
		private static List<Initialism> _Initialisms { get; } = new List<Initialism>();

		public static string[] Concat(Type moduleType, string name, string[] aliases)
		{
			var topLevel = moduleType != null;
			var initialism = new Initialism(name, topLevel);
			if (topLevel)
			{
				//Example with:
				//ChangeChannelPosition
				//ChangeChannelPerms
				var matchingInitialisms = _Initialisms.Where(x => x.Edited.CaseInsEquals(initialism.Edited)).ToArray();
				if (matchingInitialisms.Any())
				{
					//ChangeChannel is in both at the start, so would match with ChangeChannelPosition.
					var matchingStarts = matchingInitialisms.Select(x =>
					{
						var index = -1;
						for (var i = 0; i < Math.Min(x.Parts.Count, initialism.Parts.Count); ++i)
						{
							if (!x.Parts[i].CaseInsEquals(initialism.Parts[i]))
							{
								break;
							}
							++index;
						}
						return (Holder: x, MatchingStartPartsIndex: index);
					}).Where(x => x.MatchingStartPartsIndex > -1).ToArray();

					//ChangeChannel is 2 parts, so this would return 2. Add 1 to start adding from Perms instead of Channel.
					var indexToAddAt = matchingStarts.Any() ? matchingStarts.Max(x => x.MatchingStartPartsIndex) + 1 : 1;

					//Would do one loop and change ChangeChannelPerms' initialism from ccp to ccpe
					var length = 1;
					while (_Initialisms.TryGetFirst(x => x.Edited.CaseInsEquals(initialism.Edited), out _))
					{
						var newInitialism = new StringBuilder();
						for (var i = 0; i < initialism.Parts.Count; ++i)
						{
							var p = initialism.Parts[i];
							var l = i == indexToAddAt ? length : 1;
							newInitialism.Append(p.Substring(0, l));
						}
						initialism.Edited = newInitialism.ToString().ToLower();
						++length;
					}

					ConsoleUtils.DebugWrite($"Changed the alias of {initialism.Original} to {initialism.Edited}.");
				}
				_Initialisms.Add(initialism);
			}

			Array.Resize(ref aliases, aliases.Length + 1);
			aliases[aliases.Length - 1] = initialism.Edited;

			if (topLevel)
			{
				foreach (var alias in aliases)
				{
					if (_AlreadyUsed.TryGetValue(alias, out var owner))
					{
						throw new InvalidOperationException($"{owner.Name} already has registered the alias {alias}.");
					}
					_AlreadyUsed[alias] = moduleType;
				}
			}

			return aliases;
		}

		/// <summary>
		/// Creates an initialism out of the passed in name. Keeps track of the parts and original.
		/// </summary>
		public sealed class Initialism
		{
			private static readonly ImmutableDictionary<string, string> _ShortenedPhrases = new Dictionary<string, string>
			{
				{ "clear", "clr" }
			}.ToImmutableDictionary();

			/// <summary>
			/// The original supplied name.
			/// </summary>
			public string Original { get; }
			/// <summary>
			/// The edited name which has been checked to remove any duplicate conflicts with other initialisms.
			/// </summary>
			public string Edited { get; set; }
			/// <summary>
			/// The parts of the original.
			/// </summary>
			public ImmutableList<string> Parts { get; }

			/// <summary>
			/// Creates an instance of <see cref="Initialism"/>.
			/// </summary>
			/// <param name="name"></param>
			/// <param name="topLevel"></param>
			public Initialism(string name, bool topLevel)
			{
				var editedName = name;
				var parts = new List<StringBuilder>();
				var initialism = new StringBuilder();

				if (topLevel)
				{
					foreach (var kvp in _ShortenedPhrases)
					{
						editedName = editedName.CaseInsReplace(kvp.Key, kvp.Value.ToUpper());
					}
					if (name.EndsWith("s"))
					{
						editedName = editedName.Substring(0, editedName.Length - 1) + "S";
					}
				}

				for (int i = 0; i < editedName.Length; ++i)
				{
					var c = editedName[i];
					if (char.IsUpper(c))
					{
						initialism.Append(c);
						//ToString HAS to be called here or else it uses the capacity int constructor
						parts.Add(new StringBuilder(c.ToString()));
						continue;
					}
					if (i == 0)
					{
						throw new ArgumentException("Name must start with a capital letter.", nameof(name));
					}
					parts[parts.Count - 1].Append(c);
				}

				Original = name;
				Parts = parts.Select(x => x.ToString()).ToImmutableList();
				Edited = initialism.ToString().ToLower();
			}
		}
	}
}
