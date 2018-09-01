using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdvorangesUtils;
using Discord.Commands;

namespace Advobot.Classes.Attributes
{
	/// <summary>
	/// Shortens any non nested command class' name to a unique initialism or other short name.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	public sealed class TopLevelShortAliasAttribute : AliasAttribute
	{
		private static Dictionary<Type, Initialism> _AlreadyUsedInUpperMostClasses = new Dictionary<Type, Initialism>();

		/// <summary>
		/// Generates the aliases to use for the alias attribute.
		/// </summary>
		/// <param name="classType"></param>
		/// <param name="otherAliases"></param>
		public TopLevelShortAliasAttribute(Type classType, params string[] otherAliases) : base(Shorten(classType, otherAliases)) { }

		/// <summary>
		/// Shortens the class type's name and returns an array with the shortened name and the other aliases.
		/// </summary>
		/// <param name="classType"></param>
		/// <param name="otherAliases"></param>
		/// <returns></returns>
		public static string[] Shorten(Type classType, string[] otherAliases)
		{
			if (_AlreadyUsedInUpperMostClasses.TryGetValue(classType, out var alreadyCreated))
			{
				return alreadyCreated.Aliases.ToArray();
			}
			if (classType.IsNested)
			{
				throw new ArgumentException($"Needs to not have the {nameof(TopLevelShortAliasAttribute)} attribute.", classType.FullName);
			}

			var initialism = new Initialism(classType.Name, otherAliases, true);
			if (string.IsNullOrWhiteSpace(initialism.Edited))
			{
				throw new ArgumentException("Name must have at least one capital letter.", classType.FullName);
			}

			//Example with:
			//ChangeChannelPosition
			//ChangeChannelPerms
			var matchingInitialisms = _AlreadyUsedInUpperMostClasses.Values
				.Where(x => x.Edited.CaseInsEquals(initialism.Edited)).ToList();
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
				}).Where(x => x.MatchingStartPartsIndex > -1).ToList();

				//ChangeChannel is 2 parts, so this would return 2. Add 1 to start adding from Perms instead of Channel.
				var indexToAddAt = matchingStarts.Any() ? matchingStarts.Max(x => x.MatchingStartPartsIndex) + 1 : 1;

				//Would do one loop and change ChangeChannelPerms' initialism from ccp to ccpe
				var length = 1;
				while (_AlreadyUsedInUpperMostClasses.Values.Select(x => x.Edited).CaseInsContains(initialism.Edited))
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

			_AlreadyUsedInUpperMostClasses.Add(classType, initialism);
			return initialism.Aliases.ToArray();
		}
	}
}
