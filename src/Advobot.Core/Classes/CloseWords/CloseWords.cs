using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Advobot.Core.Interfaces;
using Advobot.Core.Utilities;

namespace Advobot.Core.Classes.CloseWords
{
	/// <summary>
	/// Container of close words which is intended to be removed after the time returns a value less than the current time.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public abstract class CloseWords<T> : ITime where T : IDescription
	{
		public ImmutableList<CloseWord<T>> List { get; }
		public DateTime Time { get; }

		private int _MaxAllowedCloseness = 4;
		private int _MaxOutput = 5;

		protected CloseWords(IEnumerable<T> objects, string input, TimeSpan time = default)
		{
			List = GetObjectsWithSimilarNames(objects.ToList(), input).ToImmutableList();
			Time = DateTime.UtcNow.Add(time.Equals(default) ? Constants.DEFAULT_WAIT_TIME : time);
		}

		protected abstract int FindCloseness(T obj, string input);
		protected int FindCloseName(string source, string target, int threshold = 10)
		{
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
		private List<CloseWord<T>> GetObjectsWithSimilarNames(List<T> suppliedObjects, string input)
		{
			var closeWords = new List<CloseWord<T>>();
			//First loop around to find words that are similar
			foreach (var word in suppliedObjects)
			{
				var closeness = FindCloseness(word, input);
				if (closeness > _MaxAllowedCloseness)
				{
					continue;
				}

				closeWords.Add(new CloseWord<T>(word, closeness));
				if (closeWords.Count > _MaxOutput)
				{
					closeWords = closeWords.OrderBy(x => x.Closeness).ToList();
					closeWords.RemoveRange(_MaxOutput, closeWords.Count - _MaxOutput);
				}
			}

			//Second loop around to find words that have the search term in them
			foreach (var word in suppliedObjects.Where(x => x.Name.CaseInsContains(input)))
			{
				if (closeWords.Count >= _MaxOutput)
				{
					break;
				}

				if (!closeWords.Any(x => x.Word.Name.CaseInsEquals(word.Name)))
				{
					closeWords.Add(new CloseWord<T>(word, _MaxAllowedCloseness + 1));
				}
			}

			return closeWords;
		}
		private void Swap<T2>(ref T2 arg1, ref T2 arg2)
		{
			var temp = arg1;
			arg1 = arg2;
			arg2 = temp;
		}
	}
}