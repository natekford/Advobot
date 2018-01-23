using System;
using System.Collections.Generic;
using System.Linq;
using Advobot.Core.Utilities;
using Discord.Commands;

namespace Advobot.Core.Classes.Attributes
{
	/// <summary>
	/// Shortens any non nested command class' name to a unique initialism or other short name.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public class TopLevelShortAliasAttribute : AliasAttribute
	{
		private static Dictionary<Type, Initialism> _AlreadyUsedInUpperMostClasses = new Dictionary<Type, Initialism>();

		public TopLevelShortAliasAttribute(Type classType, params string[] otherAliases) : base(Shorten(classType, otherAliases)) { }

		private static string[] Shorten(Type classType, string[] otherAliases)
		{
			if (_AlreadyUsedInUpperMostClasses.TryGetValue(classType, out var alreadyCreated))
			{
				return alreadyCreated.Aliases.ToArray();
			}

			if (classType.IsNested)
			{
				throw new ArgumentException($"needs to not have the {nameof(TopLevelShortAliasAttribute)} attribute", classType.FullName);
			}

			var initialism = new Initialism(classType.Name, otherAliases, true);
			if (String.IsNullOrWhiteSpace(initialism.Edited))
			{
				throw new ArgumentException("name must have at least one capital letter", classType.FullName);
			}

			//Example with:
			//ChangeChannelPosition
			//ChangeChannelPerms
			var matchingInitialisms = _AlreadyUsedInUpperMostClasses.Values.Where(x => x.Edited.CaseInsEquals(initialism.Edited)).ToList();
			if (matchingInitialisms.Any())
			{
				//ChangeChannel is in both at the start, so would match with ChangeChannelPosition.
				var matchingStarts = matchingInitialisms.Select(x =>
				{
					var matchingStartPartsIndex = -1;
					for (var i = 0; i < Math.Min(x.Parts.Count, initialism.Parts.Count); ++i)
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

					return (Holder: x, matchingStartPartsIndex);
				}).Where(x => x.matchingStartPartsIndex > -1).ToList();

				//ChangeChannel is 2 parts, so this would return 2. Add 1 to start adding from Perms instead of Channel.
				var indexToAddAt = matchingStarts.Any() ? matchingStarts.Max(x => x.matchingStartPartsIndex) + 1 : 1;

				//Would do one loop and change ChangeChannelPerms' initialism from ccp to ccpe
				var length = 1;
				while (_AlreadyUsedInUpperMostClasses.Values.Select(x => x.Edited).CaseInsContains(initialism.Edited))
				{
					initialism.AppendToInitialismByPart(indexToAddAt, length);
					++length;
				}

#if DEBUG
				ConsoleUtils.WriteLine($"Changed the alias of {initialism.Original} to {initialism.Edited}.", color: ConsoleColor.DarkYellow);
#endif
			}

			_AlreadyUsedInUpperMostClasses.Add(classType, initialism);
			return initialism.Aliases.ToArray();
		}
	}
}
