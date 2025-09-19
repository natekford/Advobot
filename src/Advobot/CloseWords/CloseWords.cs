using Advobot.Utilities;

using System.Diagnostics.CodeAnalysis;

namespace Advobot.CloseWords;

/// <summary>
/// Gathers objects with similar names to the passed in input.
/// </summary>
/// <typeparam name="T"></typeparam>
/// <param name="source"></param>
/// <param name="getName"></param>
public class CloseWords<T>(IEnumerable<T> source, Func<T, string> getName)
{
	/// <summary>
	/// Mark as close if the supplied text is within searched text.
	/// </summary>
	public bool IncludeWhenContains { get; set; } = true;
	/// <summary>
	/// How similar a string has to be to match.
	/// </summary>
	public int MaxAllowedCloseness { get; set; } = 4;
	/// <summary>
	/// How many closewords can be found.
	/// </summary>
	public int MaxOutput { get; set; } = 5;
	/// <summary>
	/// Gets the name of the object.
	/// </summary>
	protected Func<T, string> GetName { get; } = getName;
	/// <summary>
	/// What to search through.
	/// </summary>
	protected IEnumerable<T> Source { get; } = source;

	/// <summary>
	/// Returns matches.
	/// </summary>
	/// <param name="search"></param>
	/// <returns></returns>
	public virtual IReadOnlyList<CloseWord<T>> FindMatches(string search)
	{
		var list = new List<CloseWord<T>>(MaxOutput);
		foreach (var item in Source)
		{
			if (!IsCloseWord(search, item, out var closeWord))
			{
				continue;
			}
			if (MaxOutput > list.Count)
			{
				list.Add(closeWord);
				continue;
			}

			for (var j = 0; j < list.Count; ++j)
			{
				if (closeWord.CompareTo(list[j]) < 0 && j < MaxOutput)
				{
					list.RemoveAt(MaxOutput - 1);
					list.Insert(j, closeWord);
					break;
				}
			}
		}
		return list;
	}

	/// <summary>
	/// Returns a value gotten from using Damerau Levenshtein distance to compare the source and target.
	/// </summary>
	/// <param name="source"></param>
	/// <param name="target"></param>
	/// <param name="threshold"></param>
	/// <returns></returns>
	protected static int FindCloseness(string source, string target, int threshold = 10)
	{
		static void Swap<T2>(ref T2 arg1, ref T2 arg2)
			=> (arg2, arg1) = (arg1, arg2);

		// Damerau Levenshtein Distance: https://en.wikipedia.org/wiki/Damerau–Levenshtein_distance
		// Copied nearly verbatim from: https://stackoverflow.com/a/9454016
		var length1 = source.Length;
		var length2 = target.Length;

		// Return trivial case - difference in string lengths exceeds threshhold
		if (Math.Abs(length1 - length2) > threshold)
		{
			return int.MaxValue;
		}

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

		for (var i = 0; i <= maxi; i++)
		{
			dCurrent[i] = i;
		}

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
				var min = del > ins ? ins > sub ? sub : ins : del > sub ? sub : del;

				if (i > 1 && j > 1 && source[im2] == target[jm1] && source[im1] == target[j - 2])
				{
					min = Math.Min(min, dMinus2[im2] + cost);
				}

				dCurrent[i] = min;
				if (min < minDistance)
				{
					minDistance = min;
				}
				im1++;
				im2++;
			}
			jm1++;
			if (minDistance > threshold)
			{
				return int.MaxValue;
			}
		}

		var result = dCurrent[maxi];
		return result > threshold ? int.MaxValue : result;
	}

	/// <summary>
	/// Finds the closeness of this object to the search string.
	/// </summary>
	/// <param name="search"></param>
	/// <param name="obj"></param>
	/// <returns></returns>
	protected virtual CloseWord<T> FindCloseness(string search, T obj)
	{
		var name = GetName(obj);
		var distance = FindCloseness(name, search);
		return new(name, search, distance, obj);
	}

	/// <summary>
	/// Determines whether this is a close word.
	/// </summary>
	/// <param name="search"></param>
	/// <param name="obj"></param>
	/// <param name="closeWord"></param>
	/// <returns></returns>
	protected bool IsCloseWord(string search, T obj, [NotNullWhen(true)] out CloseWord<T> closeWord)
	{
		var closeness = FindCloseness(search, obj);
		var success = closeness.Distance < MaxAllowedCloseness
			|| (IncludeWhenContains && closeness.Name.CaseInsContains(search));
		closeWord = success ? closeness : default;
		return success;
	}
}