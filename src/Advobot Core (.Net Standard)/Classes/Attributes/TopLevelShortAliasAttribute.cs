using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Advobot.Classes.Attributes
{
	/// <summary>
	/// Shortens any non nested command class' name to a unique initialism or other short name.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public class TopLevelShortAliasAttribute : AliasAttribute
	{
		private static List<InitialismHolder> _AlreadyUsedInUpperMostClasses = new List<InitialismHolder>();

		//TODO: make require type instead of name since type provides name and other useful? info
		public TopLevelShortAliasAttribute(Type classType, params string[] otherAliases) : base(Shorten(classType, otherAliases)) { }

		private static string[] Shorten(Type classType, string[] otherAliases)
		{
			var alreadyCreated = _AlreadyUsedInUpperMostClasses.SingleOrDefault(x => x.Original == classType.Name);
			if (alreadyCreated != null)
			{
				return alreadyCreated.Aliases;
			}
			else if (classType.IsNested)
			{
				throw new ArgumentException($"The nested class {classType.FullName} needs to not have the {nameof(TopLevelShortAliasAttribute)} attribute.");
			}

			var initialism = new InitialismHolder(classType.Name, otherAliases, true);
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
}
