using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Advobot.Classes.Attributes
{
	[AttributeUsage(AttributeTargets.Class)]
	public class TopLevelShortAliasAttribute : AliasAttribute
	{
		private static List<InitialismHolder> _AlreadyUsedInUpperMostClasses = new List<InitialismHolder>();

		public TopLevelShortAliasAttribute(string name, params string[] otherAliases) : base(Shorten(name, otherAliases)) { }

		private static string[] Shorten(string name, string[] otherAliases)
		{
			var alreadyCreated = _AlreadyUsedInUpperMostClasses.SingleOrDefault(x => x.Original == name);
			if (alreadyCreated != null)
			{
				return alreadyCreated.Aliases;
			}

			var initialism = new InitialismHolder(name, otherAliases, true);
			if (String.IsNullOrWhiteSpace(initialism.ToString()))
			{
				throw new ArgumentException("Invalid alias provided. Must have at least one capital letter.");
			}

			//Example with:
			//ChangeChannelPosition
			//ChangeChannelPerms
			var matchingInitialisms = _AlreadyUsedInUpperMostClasses.Where(x => x.Initialism.CaseInsEquals(initialism.ToString()));
			if (matchingInitialisms.Any())
			{
				//ChangeChannel is in both at the start, so would match with ChangeChannelPosition.
				var matchingStarts = matchingInitialisms.Select(x =>
				{
					var matchingStartPartsIndex = -1;
					for (int i = 0; i < Math.Min(x.Parts.Count, initialism.Parts.Count); ++i)
					{
						if (x.Parts[i].CaseInsEquals(initialism.Parts[i]))
						{
							++matchingStartPartsIndex;
						}
						else
						{
							break;
						}
					}

					return (Holder: x, matchingStartPartsIndex: matchingStartPartsIndex);
				}).Where(x => x.matchingStartPartsIndex > -1);

				//ChangeChannel is 2 parts, so this would return 2. Add 1 to start adding from Perms instead of Channel.
				var indexToAddAt = matchingStarts.Any() ? matchingStarts.Max(x => x.matchingStartPartsIndex) + 1 : 1;

				//Would do one loop and change ChangeChannelPerms' initialism from ccp to ccpe
				var length = 1;
				while (_AlreadyUsedInUpperMostClasses.Select(x => x.Initialism).CaseInsContains(initialism.Initialism))
				{
					initialism.AppendToInitialismByPart(indexToAddAt, length);
					++length;
				}

#if false
				ConsoleActions.WriteLine($"Changed the alias of {initialism.Original} to {initialism.Initialism}.", color: ConsoleColor.DarkYellow);
#endif
			}

			_AlreadyUsedInUpperMostClasses.Add(initialism);
			return initialism.Aliases;
		}
	}

	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
	public class ShortAliasAttribute : AliasAttribute 
	{
		public ShortAliasAttribute(string name, params string[] otherAliases) : base(Shorten(name, otherAliases)) { }

		private static string[] Shorten(string name, string[] otherAliases)
		{
			var initialism = new InitialismHolder(name, otherAliases, false);
			if (String.IsNullOrWhiteSpace(initialism.ToString()))
			{
				throw new ArgumentException("Invalid alias provided. Must have at least one capital letter.");
			}

			return initialism.Aliases;
		}
	}

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

		public override string ToString()
		{
			return Initialism;
		}
	}
}
