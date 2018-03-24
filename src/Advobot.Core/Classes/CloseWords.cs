using Advobot.Core.Classes.Punishments;
using Advobot.Core.Utilities;
using AdvorangesUtils;
using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Advobot.Core.Classes.CloseWords
{
	/// <summary>
	/// Container of close words which is intended to be removed after the time has passed.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public abstract class CloseWords<T> : RemovableMessage
	{
		/// <summary>
		/// The max allowed closeness before a word will not be added.
		/// </summary>
		public int MaxAllowedCloseness { get; set; } = 4;
		/// <summary>
		/// The max allowed output to add to <see cref="List"/>.
		/// </summary>
		public int MaxOutput { get; set; } = 5;
		/// <summary>
		/// The gathered words.
		/// </summary>
		public List<CloseWord> List { get; set; }

		/// <summary>
		/// Initializes the object. Parameterless constructor is used for the database.
		/// </summary>
		protected CloseWords() : base() { }
		/// <summary>
		/// Initializes the object with the supplied values.
		/// </summary>
		/// <param name="time"></param>
		/// <param name="context"></param>
		/// <param name="objects"></param>
		/// <param name="search"></param>
		protected CloseWords(TimeSpan time, ICommandContext context, IEnumerable<T> objects, string search)
			: base(time, context)
		{
			var closeWords = new List<CloseWord>();
			//First loop around to find words that are similar
			foreach (var word in objects)
			{
				if (!IsCloseWord(word, search, out var closeWord))
				{
					continue;
				}
				closeWords.Add(closeWord);
				if (closeWords.Count > MaxOutput)
				{
					closeWords = closeWords.OrderBy(x => x.Closeness).ToList();
					closeWords.RemoveRange(MaxOutput, closeWords.Count - MaxOutput);
				}
			}
			//Then loop around for words that have the search term simply inside them
			for (int i = closeWords.Count - 1; i < MaxOutput; ++i)
			{
				if (!TryGetCloseWord(objects, closeWords.Select(x => x.Name), search, out var closeWord))
				{
					break;
				}
				closeWords.Add(closeWord);
			}
			List = closeWords.Where(x => x != null && x.Closeness > -1 && x.Closeness <= MaxAllowedCloseness).ToList();
		}

		/// <summary>
		/// Sends the bots response to let the user know what options they can pick from.
		/// </summary>
		/// <param name="channel"></param>
		/// <returns></returns>
		public async Task SendBotMessageAsync(IMessageChannel channel)
		{
			var text = $"Did you mean any of the following:\n{List.FormatNumberedList(x => x.Name)}";
			MessageIds.Add((await MessageUtils.SendMessageAsync(channel, text).CAF()).Id);
		}
		/// <summary>
		/// Determines if an object has a similar name to the search term.
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="search"></param>
		/// <param name="closeWord"></param>
		/// <returns></returns>
		protected abstract bool IsCloseWord(T obj, string search, out CloseWord closeWord);
		/// <summary>
		/// Finds an object with the search term in directly in their name.
		/// </summary>
		/// <param name="objs"></param>
		/// <param name="used"></param>
		/// <param name="search"></param>
		/// <param name="closeWord"></param>
		/// <returns></returns>
		protected abstract bool TryGetCloseWord(
			IEnumerable<T> objs,
			IEnumerable<string> used,
			string search,
			out CloseWord closeWord);
		/// <summary>
		/// Returns a value gotten from using Damerau Levenshtein distance to compare the source and target.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="target"></param>
		/// <param name="threshold"></param>
		/// <returns></returns>
		protected int FindCloseness(string source, string target, int threshold = 10)
		{
			void Swap<T2>(ref T2 arg1, ref T2 arg2)
			{
				var temp = arg1;
				arg1 = arg2;
				arg2 = temp;
			}

			/* Damerau Levenshtein Distance: https://en.wikipedia.org/wiki/Damerau–Levenshtein_distance
			 * Copied nearly verbatim from: https://stackoverflow.com/a/9454016 
			 */
			var length1 = source.Length;
			var length2 = target.Length;

			// Return trivial case - difference in string lengths exceeds threshhold
			if (Math.Abs(length1 - length2) > threshold) { return int.MaxValue; }

			// Ensure arrays [i] / length1 use shorter length 
			if (length1 > length2)
			{
				Swap(ref target, ref source);
				Swap(ref length1, ref length2);
			}

			var maxi = length1;
			var maxj = length2;

			var dCurrent = new int[maxi + 1];
			var dMinus1 = new int[maxi + 1];
			var dMinus2 = new int[maxi + 1];
			int[] dSwap;

			for (var i = 0; i <= maxi; i++) { dCurrent[i] = i; }

			int jm1 = 0, im1 = 0, im2 = -1;

			for (var j = 1; j <= maxj; j++)
			{

				// Rotate
				dSwap = dMinus2;
				dMinus2 = dMinus1;
				dMinus1 = dCurrent;
				dCurrent = dSwap;

				// Initialize
				var minDistance = int.MaxValue;
				dCurrent[0] = j;
				im1 = 0;
				im2 = -1;

				for (var i = 1; i <= maxi; i++)
				{

					var cost = source[im1] == target[jm1] ? 0 : 1;

					var del = dCurrent[im1] + 1;
					var ins = dMinus1[i] + 1;
					var sub = dMinus1[im1] + cost;

					//Fastest execution for min value of 3 integers
					var min = (del > ins) ? (ins > sub ? sub : ins) : (del > sub ? sub : del);

					if (i > 1 && j > 1 && source[im2] == target[jm1] && source[im1] == target[j - 2])
					{
						min = Math.Min(min, dMinus2[im2] + cost);
					}

					dCurrent[i] = min;
					if (min < minDistance) { minDistance = min; }
					im1++;
					im2++;
				}
				jm1++;
				if (minDistance > threshold) { return int.MaxValue; }
			}

			var result = dCurrent[maxi];
			return (result > threshold) ? int.MaxValue : result;
		}

		/// <summary>
		/// Holds an object which has a name and text and its closeness.
		/// </summary>
		public class CloseWord
		{
			/// <summary>
			/// How close the name is to the search term.
			/// </summary>
			public int Closeness { get; set; }
			/// <summary>
			/// The name of the object.
			/// </summary>
			public string Name { get; set; }
			/// <summary>
			/// The text of the object.
			/// </summary>
			public string Text { get; set; }

			/// <summary>
			/// Initializes the object. Parameterless constructor is used for the database.
			/// </summary>
			public CloseWord() { }
			/// <summary>
			/// Initializes the object with the supplied values.
			/// </summary>
			/// <param name="closeness"></param>
			/// <param name="name"></param>
			/// <param name="text"></param>
			public CloseWord(int closeness, string name, string text)
			{
				Closeness = closeness;
				Name = name;
				Text = text;
			}
		}
	}
}