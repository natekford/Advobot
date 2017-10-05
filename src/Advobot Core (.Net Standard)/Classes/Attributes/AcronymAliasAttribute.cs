using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Advobot.Classes.Attributes
{
	public class AcronymAliasAttribute : AliasAttribute
	{
		private static Dictionary<Type, List<string>> _Aliases = new Dictionary<Type, List<string>>();

		public AcronymAliasAttribute(Type classType, params string[] aliases) : base(ConvertToAcronym(classType, aliases))
		{
		}

		private static string[] ConvertToAcronym(Type classType, string[] aliases)
		{
			if (!_Aliases.TryGetValue(classType, out var usedAliases))
			{
				_Aliases.Add(classType, usedAliases = new List<string>());
			}

			var tempAliases = new List<string>();
			foreach (var alias in aliases)
			{
				var acronym = CreateAcronym(alias);
				if (String.IsNullOrWhiteSpace(acronym))
				{
					throw new ArgumentException("Invalid alias provided. Must have at least one capital letter.");
				}

				while (usedAliases.Contains(acronym))
				{
					//Say command is softban and sb is already used
					//Try to add a o, then f, then t, etc. until it's not used anymore
					var placeToPutNewChar = acronym.Length - 1;
					acronym = acronym.Insert(placeToPutNewChar, alias.ElementAt(placeToPutNewChar).ToString());
				}

				tempAliases.Add(acronym);
			}

			usedAliases.AddRange(tempAliases);
			return tempAliases.ToArray();
		}
		private static string CreateAcronym(string alias)
		{
			var sb = new StringBuilder();
			foreach (var c in alias)
			{
				if (Char.IsUpper(c))
				{
					sb.Append(c);
				}
			}
			return sb.ToString().ToLower();
		}
	}
}
