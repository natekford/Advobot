using System;
using System.Collections.Generic;
using System.Linq;

namespace Advobot.Classes.CloseWords
{
	/// <summary>
	/// Gathers objects with similar names to the passed in input.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public abstract class CloseWords<T>
	{
		/// <summary>
		/// Matching close words.
		/// </summary>
		public IEnumerable<CloseWord> Matches { get; protected set; }

		/// <summary>
		/// What to search through.
		/// </summary>
		protected IEnumerable<T> Source { get; }
		/// <summary>
		/// What to search for.
		/// </summary>
		protected string Search { get; }
		/// <summary>
		/// How similar a string has to be to match.
		/// </summary>
		protected int MaxAllowedCloseness { get; }
		/// <summary>
		/// How many closewords can be found.
		/// </summary>
		protected int MaxOutput { get; }

		/// <summary>
		/// Creates an instance of <see cref="CloseWords{T}"/>.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="search"></param>
		/// <param name="maxAllowedCloseness"></param>
		/// <param name="maxOutput"></param>
		public CloseWords(IEnumerable<T> source, string search, int maxAllowedCloseness = 4, int maxOutput = 5)
		{
			Source = source;
			Search = search;
			MaxAllowedCloseness = maxAllowedCloseness;
			MaxOutput = maxOutput;
		}

		/// <summary>
		/// Returns matches.
		/// </summary>
		/// <returns></returns>
		protected IEnumerable<CloseWord> FindMatches()
		{
			var closeWords = new List<CloseWord>();
			//First loop around to find words that are similar
			foreach (var word in Source)
			{
				if (!IsCloseWord(word, out var closeWord))
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
				if (!TryGetCloseWord(Source, closeWords.Select(x => x.Name), out var closeWord))
				{
					break;
				}
				closeWords.Add(closeWord);
			}
			return closeWords.Where(x => x != null && x.Closeness > -1).Take(MaxOutput);
		}
		/// <summary>
		/// Determines whether this is a close word.
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="closeWord"></param>
		/// <returns></returns>
		protected abstract bool IsCloseWord(T obj, out CloseWord closeWord);
		/// <summary>
		/// Attempts to get a closeword from the source which contains the name.
		/// </summary>
		/// <param name="objs"></param>
		/// <param name="used"></param>
		/// <param name="closeWord"></param>
		/// <returns></returns>
		protected abstract bool TryGetCloseWord(IEnumerable<T> objs, IEnumerable<string> used, out CloseWord closeWord);
		/// <summary>
		/// Returns a value gotten from using Damerau Levenshtein distance to compare the source and target.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="target"></param>
		/// <param name="threshold"></param>
		/// <returns></returns>
		public static int FindCloseness(string source, string target, int threshold = 10)
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
	}
}