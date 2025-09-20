using System.Runtime.InteropServices;

namespace Advobot.Similar;

/// <summary>
/// Gathers objects with similar names to the passed in input.
/// </summary>
/// <typeparam name="T"></typeparam>
/// <param name="source"></param>
public abstract class Similar<T>(IEnumerable<T> source)
{
	/// <summary>
	/// How many matches can be found.
	/// </summary>
	public int MaxOutput { get; set; } = 5;
	/// <summary>
	/// How dissimilar a string can be to be considered a match.
	/// </summary>
	public int Threshold { get; set; } = 4;
	/// <summary>
	/// What to search through.
	/// </summary>
	protected IEnumerable<T> Source { get; } = source;

	/// <summary>
	/// Returns matches.
	/// </summary>
	/// <param name="search"></param>
	/// <returns></returns>
	public virtual IReadOnlyList<Similarity<T>> FindSimilar(string search)
	{
		var list = new List<Similarity<T>>(MaxOutput);
		foreach (var item in Source)
		{
			var similar = FindSimilarity(search, item);
			if (similar.Distance > Threshold)
			{
				continue;
			}
			if (MaxOutput > list.Count)
			{
				list.Add(similar);
				continue;
			}

			for (var j = 0; j < list.Count; ++j)
			{
				if (similar.CompareTo(list[j]) < 0 && j < MaxOutput)
				{
					list.RemoveAt(MaxOutput - 1);
					list.Insert(j, similar);
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
	/// <param name="search"></param>
	/// <param name="threshold"></param>
	/// <returns></returns>
	protected static int FindCloseness(string source, string search, int threshold)
	{
		static void Swap<T2>(ref T2 arg1, ref T2 arg2)
			=> (arg2, arg1) = (arg1, arg2);

		static bool CharsEqual(char a, char b)
		{
			var spanA = MemoryMarshal.CreateReadOnlySpan(ref a, 1);
			var spanB = MemoryMarshal.CreateReadOnlySpan(ref b, 1);
			return spanA.CompareTo(spanB, StringComparison.OrdinalIgnoreCase) == 0;
		}

		// Damerau Levenshtein Distance: https://en.wikipedia.org/wiki/Damerau–Levenshtein_distance
		// Copied nearly verbatim from: https://stackoverflow.com/a/9454016
		var length1 = source.Length;
		var length2 = search.Length;

		// Return trivial case - difference in string lengths exceeds threshhold
		if (Math.Abs(length1 - length2) > threshold)
		{
			return int.MaxValue;
		}

		// Ensure arrays [i] / length1 use shorter length
		if (length1 > length2)
		{
			Swap(ref search, ref source);
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
				var cost = CharsEqual(source[im1], search[jm1]) ? 0 : 1;

				var del = dCurrent[im1] + 1;
				var ins = dMinus1[i] + 1;
				var sub = dMinus1[im1] + cost;

				//Fastest execution for min value of 3 integers
				var min = del > ins ? ins > sub ? sub : ins : del > sub ? sub : del;

				if (i > 1 && j > 1 && CharsEqual(source[im2], search[jm1]) && CharsEqual(source[im1], search[j - 2]))
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
	/// <param name="item"></param>
	/// <returns></returns>
	protected abstract Similarity<T> FindSimilarity(string search, T item);
}