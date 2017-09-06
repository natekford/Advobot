using Advobot.Classes;
using Advobot.Enums;
using System.Collections.Generic;
using System.Linq;

namespace Advobot.Actions
{
	public static class BannedPhraseActions
	{
		public static void HandleBannedPhraseModification(List<BannedPhrase> bannedPhrases, IEnumerable<string> inputPhrases, bool add, out List<string> success, out List<string> failure)
		{
			success = new List<string>();
			failure = new List<string>();
			if (add)
			{
				//Don't add duplicate words
				foreach (var str in inputPhrases)
				{
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
				return;
			}

			var positions = new List<int>();
			foreach (var potentialPosition in inputPhrases)
			{
				if (int.TryParse(potentialPosition, out int temp) && temp < bannedPhrases.Count)
				{
					positions.Add(temp);
				}
			}

			//Removing by position
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

			//Removing by index
			foreach (var str in inputPhrases)
			{
				var temp = bannedPhrases.FirstOrDefault(x => x.Phrase.Equals(str));
				if (temp != null)
				{
					success.Add(str);
					bannedPhrases.Remove(temp);
				}
				else
				{
					failure.Add(str);
				}
			}
		}
	}
}