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
		private static readonly ImmutableDictionary<string, string> _ShortenedPhrases = new Dictionary<string, string>
		{
			{ "clear", "clr" }
		}.ToImmutableDictionary();
		private static Dictionary<string, Type> _AlreadyUsedModuleAliases { get; } = new Dictionary<string, Type>();
		private static List<(string Edited, ImmutableArray<string> Parts)> _ModuleInitialisms { get; } = new List<(string Edited, ImmutableArray<string> Parts)>();

		public static string[] ConcatCommandAliases(string name, string[] aliases)
			=> ConcatAliases(null, name, aliases);
		public static string[] ConcatModuleAliases(Type module, string[] aliases)
			=> ConcatAliases(module, module.Name, aliases);
		private static string[] ConcatAliases(Type module, string name, string[] aliases)
		{
			var isModule = module != null;

			Array.Resize(ref aliases, aliases.Length + 1);
			aliases[aliases.Length - 1] = Shorten(name, isModule);

			if (isModule)
			{
				foreach (var alias in aliases)
				{
					if (_AlreadyUsedModuleAliases.TryGetValue(alias, out var owner))
					{
						throw new InvalidOperationException($"{owner.Name} already has registered the alias {alias}.");
					}
					_AlreadyUsedModuleAliases[alias] = module;
				}
			}

			return aliases;
		}
		private static string Shorten(string name, bool isModule)
		{
			var initialism = CreateInitialism(name, isModule);
			if (isModule)
			{
				//Example with:
				//ChangeChannelPosition
				//ChangeChannelPerms
				var matchingInitialisms = _ModuleInitialisms.Where(x => x.Edited.CaseInsEquals(initialism.Edited)).ToArray();
				if (matchingInitialisms.Any())
				{
					//ChangeChannel is in both at the start, so would match with ChangeChannelPosition.
					var matchingStarts = matchingInitialisms.Select(x =>
					{
						var index = -1;
						for (var i = 0; i < Math.Min(x.Parts.Length, initialism.Parts.Length); ++i)
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
					while (_ModuleInitialisms.TryGetFirst(x => x.Edited.CaseInsEquals(initialism.Edited), out _))
					{
						var newInitialism = new StringBuilder();
						for (var i = 0; i < initialism.Parts.Length; ++i)
						{
							var p = initialism.Parts[i];
							var l = i == indexToAddAt ? length : 1;
							newInitialism.Append(p.Substring(0, l));
						}
						initialism.Edited = newInitialism.ToString().ToLower();
						++length;
					}

					ConsoleUtils.DebugWrite($"Changed the alias of {name} to {initialism.Edited}.");
				}
				_ModuleInitialisms.Add(initialism);
			}
			return initialism.Edited;
		}
		private static (string Edited, ImmutableArray<string> Parts) CreateInitialism(string name, bool isModule)
		{
			var parts = new List<StringBuilder>();
			var initialism = new StringBuilder();

			if (isModule)
			{
				foreach (var kvp in _ShortenedPhrases)
				{
					name = name.CaseInsReplace(kvp.Key, kvp.Value.ToUpper());
				}
				if (name.EndsWith("s"))
				{
					name = name.Substring(0, name.Length - 1) + "S";
				}
			}

			for (int i = 0; i < name.Length; ++i)
			{
				var c = name[i];
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

			return (initialism.ToString().ToLower(), parts.Select(x => x.ToString()).ToImmutableArray());
		}
	}
}
