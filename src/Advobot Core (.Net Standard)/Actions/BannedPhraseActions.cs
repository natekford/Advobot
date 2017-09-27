using Advobot.Classes;
using Advobot.Enums;
using System.Collections.Generic;
using System.Linq;
using Advobot.Interfaces;

namespace Advobot.Actions
{
	public static class BannedPhraseActions
	{
		/// <summary>
		/// Adds nonduplicate strings to the list of banned phrases.
		/// </summary>
		/// <param name="bannedPhrases"></param>
		/// <param name="inputPhrases"></param>
		/// <param name="success"></param>
		/// <param name="failure"></param>
		public static void AddBannedPhrases(List<BannedPhrase> bannedPhrases, IEnumerable<string> inputPhrases, out List<string> success, out List<string> failure)
		{
			success = new List<string>();
			failure = new List<string>();

			foreach (var str in inputPhrases)
			{
				//Don't add duplicate words
				if (!bannedPhrases.Any(x => x.Phrase.CaseInsEquals(str)))
				{
					success.Add(str);
					bannedPhrases.Add(new BannedPhrase(str, default(PunishmentType)));
				}
				else
				{
					failure.Add(str);
				}
			}
		}
		/// <summary>
		/// Removes banned phrases by position or matching text.
		/// </summary>
		/// <param name="bannedPhrases"></param>
		/// <param name="inputPhrases"></param>
		/// <param name="success"></param>
		/// <param name="failure"></param>
		public static void RemoveBannedPhrases(List<BannedPhrase> bannedPhrases, IEnumerable<string> inputPhrases, out List<string> success, out List<string> failure)
		{
			success = new List<string>();
			failure = new List<string>();

			var positions = new List<int>();
			foreach (var potentialPosition in inputPhrases)
			{
				if (int.TryParse(potentialPosition, out int temp) && temp < bannedPhrases.Count)
				{
					positions.Add(temp);
				}
			}

			//Removing by index
			if (positions.Any())
			{
				//Put them in descending order so as to not delete low values before high ones
				foreach (var position in positions.OrderByDescending(x => x))
				{
					if (bannedPhrases.Count - 1 <= position)
					{
						success.Add(bannedPhrases[position]?.Phrase ?? "null");
						bannedPhrases.RemoveAt(position);
					}
					else
					{
						failure.Add("String at position " + position);
					}
				}
				return;
			}

			//Removing by text matching
			foreach (var str in inputPhrases)
			{
				if (bannedPhrases.Remove(bannedPhrases.FirstOrDefault(x => x.Phrase.Equals(str))))
				{
					success.Add(str);
				}
				else
				{
					failure.Add(str);
				}
			}
		}
	}
}