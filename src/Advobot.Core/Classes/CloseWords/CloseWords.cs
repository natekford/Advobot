using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Advobot.Interfaces;
using AdvorangesUtils;

namespace Advobot.Classes.CloseWords
{
	/// <summary>
	/// Gathers objects with similar names to the passed in input.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class CloseWords<T> where T : INameable
	{
		/// <summary>
		/// What to search through.
		/// </summary>
		protected IReadOnlyList<T> Source { get; }
		/// <summary>
		/// How similar a string has to be to match.
		/// </summary>
		public int MaxAllowedCloseness { get; set; }
		/// <summary>
		/// How many closewords can be found.
		/// </summary>
		public int MaxOutput { get; set; }

		/// <summary>
		/// Creates an instance of <see cref="CloseWords{T}"/>.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="maxAllowedCloseness"></param>
		/// <param name="maxOutput"></param>
		public CloseWords(IEnumerable<T> source, int maxAllowedCloseness = 4, int maxOutput = 5)
		{
			Source = source.ToImmutableArray();
			MaxAllowedCloseness = maxAllowedCloseness;
			MaxOutput = maxOutput;
		}

		/// <summary>
		/// Returns matches.
		/// </summary>
		/// <param name="search"></param>
		/// <returns></returns>
		public IReadOnlyList<CloseWord<T>> FindMatches(string search)
		{
			var list = new List<CloseWord<T>>();
			foreach (var item in Source)
			{
				if (IsCloseWord(search, item, out var closeWord) && closeWord != null)
				{
					list.Add(closeWord);
				}
			}
			return list.OrderBy(x => x.Closeness).ThenBy(x => x.Name.Length).Take(MaxOutput).ToArray();
		}
		/// <summary>
		/// Determines whether this is a close word.
		/// </summary>
		/// <param name="search"></param>
		/// <param name="obj"></param>
		/// <param name="closeWord"></param>
		/// <returns></returns>
		protected virtual bool IsCloseWord(string search, T obj, out CloseWord<T>? closeWord)
		{
			var closeness = FindCloseness(obj.Name, search);
			var success = closeness < MaxAllowedCloseness || obj.Name.CaseInsContains(search);
			closeWord = success ? new CloseWord<T>(closeness, obj.Name, obj) : null;
			return success;
		}
		/// <summary>
		/// Returns a value gotten from using Damerau Levenshtein distance to compare the source and target.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="target"></param>
		/// <param name="threshold"></param>
		/// <returns></returns>
		public static int FindCloseness(string source, string target, int threshold = 10)
		{
			static void Swap<T2>(ref T2 arg1, ref T2 arg2)
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

			int jm1 = 0, im1, im2;

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