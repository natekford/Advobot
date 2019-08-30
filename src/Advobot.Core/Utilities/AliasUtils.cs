using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Advobot.Localization;

using AdvorangesUtils;

namespace Advobot.Utilities
{
	internal static class AliasUtils
	{
		private static readonly AliasManager _Manager = new AliasManager();

		public static string[] ConcatCommandAliases(string name, IReadOnlyList<string> aliases)
			=> ConcatAliases(null, name, aliases);

		public static string[] ConcatModuleAliases(Type module, IReadOnlyList<string> aliases)
			=> ConcatAliases(module, module.Name, aliases);

		private static string[] ConcatAliases(Type? module, string name, IReadOnlyList<string> aliases)
		{
			var array = new string[aliases.Count + 1];
			for (var i = 0; i < aliases.Count; ++i)
			{
				array[i] = aliases[i];
			}
			array[^1] = Shorten(name, module != null);

			if (module == null)
			{
				return array;
			}

			foreach (var alias in aliases)
			{
				_Manager.RegisterAlias(module, alias);
			}
			return array;
		}

		private static Initialism CreateInitialism(string name, bool isModule)
		{
			var parts = new List<StringBuilder>();
			var initialism = new StringBuilder();

			if (isModule)
			{
				if (name.EndsWith("s"))
				{
					name = name[0..^1] + "S";
				}
			}

			for (var i = 0; i < name.Length; ++i)
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
				parts.Last().Append(c);
			}

			return new Initialism(initialism, parts);
		}

		private static string Shorten(string name, bool isModule)
		{
			var initialism = CreateInitialism(name, isModule);
			if (!isModule)
			{
				return initialism.Edited;
			}

			var initialisms = _Manager.GetInitialisms();
			var matching = initialisms.Where(x => x.Edited.CaseInsEquals(initialism.Edited));
			if (!matching.Any())
			{
				initialisms.Add(initialism);
				return initialism.Edited;
			}

			//Example with:
			//ChangeChannelPosition
			//ChangeChannelPerms
			//ChangeChannel is in both at the start, so would match with ChangeChannelPosition.
			//ChangeChannel is 2 parts, so this would return 2. Add 1 to start adding from Perms instead of Channel.
			var offset = 1 + matching.Select(x =>
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
				return index;
			}).Where(x => x > -1).DefaultIfEmpty(0).Max();

			//Would do one loop and change ChangeChannelPerms' initialism from ccp to ccpe
			for (var i = 0; i < name.Length && initialisms.TryGetFirst(x => x.Edited == initialism.Edited, out _); ++i)
			{
				if (i > name.Length)
				{
					throw new InvalidOperationException($"Unable to generate an initialism for {name} which is not already being used.");
				}

				var newInitialism = new StringBuilder();
				for (var j = 0; j < initialism.Parts.Count; ++j)
				{
					var part = initialism.Parts[j];
					var length = j == offset ? i : 1;
					newInitialism.Append(part, 0, length);
				}

				initialism.Edited = newInitialism.ToString().ToLower();
			}

			ConsoleUtils.DebugWrite($"Changed the alias of {name} to {initialism.Edited}.");
			initialisms.Add(initialism);
			return initialism.Edited;
		}

		private sealed class AliasManager
		{
			private readonly Localized<ConcurrentDictionary<string, Type>> _Aliases
				= Localized.Create<ConcurrentDictionary<string, Type>>();

			private readonly Localized<ConcurrentBag<Initialism>> _Initialisms
				= Localized.Create<ConcurrentBag<Initialism>>();

			public ConcurrentBag<Initialism> GetInitialisms()
				=> _Initialisms.Get();

			public void RegisterAlias(Type module, string alias)
			{
				_Aliases.Get().AddOrUpdate(alias, module,
					(key, value) => throw new InvalidOperationException($"{value.Name} already has registered the alias {key}."));
			}
		}

		private sealed class Initialism
		{
			public string Edited { get; set; }

			public IReadOnlyList<string> Parts { get; }

			public Initialism(StringBuilder edited, IEnumerable<StringBuilder> parts)
										: this(edited.ToString().ToLower(), parts.Select(x => x.ToString())) { }

			public Initialism(string edited, IEnumerable<string> parts)
			{
				Edited = edited;
				Parts = parts.ToArray();
			}
		}
	}
}