using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Advobot.Similar;

/// <summary>
/// Holds an object which has a name and text and its similarity.
/// </summary>
/// <typeparam name="T"></typeparam>
/// <param name="Name">The name being compared.</param>
/// <param name="Search">The search term.</param>
/// <param name="Distance">
/// The Damerau Levenshtein distance between <see cref="Name"/> and <see cref="Search"/>.
/// </param>
/// <param name="Value">The object this is coming from.</param>
[DebuggerDisplay(Constants.DEBUGGER_DISPLAY)]
public readonly record struct Similarity<T>(
	string Name,
	string Search,
	int Distance,
	T Value
) : IComparable<Similarity<T>>
{
	private string DebuggerDisplay => $"Name = {Name}, Distance = {Distance}";

	/// <summary>
	/// Gets items with similar names to <paramref name="search"/>.
	/// </summary>
	/// <param name="source"></param>
	/// <param name="search"></param>
	/// <param name="getNames"></param>
	/// <param name="threshold"></param>
	/// <param name="maxOutput"></param>
	/// <returns></returns>
	public static IReadOnlyList<Similarity<T>> Get(
		IEnumerable<T> source,
		string search,
		Func<T, IEnumerable<string>> getNames,
		int threshold = 4,
		int maxOutput = 5)
	{
		var list = new List<Similarity<T>>(maxOutput);
		foreach (var item in source)
		{
			var name = "";
			var distance = int.MaxValue;
			foreach (var path in getNames(item))
			{
				var cDistance = GetDistance(path, search, threshold);
				if (distance > cDistance)
				{
					name = path;
					distance = cDistance;
				}
			}

			var similar = new Similarity<T>(name, search, distance, item);
			if (similar.Distance > threshold)
			{
				continue;
			}
			if (maxOutput > list.Count)
			{
				list.Add(similar);
				continue;
			}

			for (var j = 0; j < list.Count; ++j)
			{
				if (similar.CompareTo(list[j]) < 0 && j < maxOutput)
				{
					list.RemoveAt(maxOutput - 1);
					list.Insert(j, similar);
					break;
				}
			}
		}
		return list;
	}

	private static int GetDistance(string source, string search, int threshold)
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

	/// <inheritdoc />
	public int CompareTo(Similarity<T> other)
	{
		var distance = Distance.CompareTo(other.Distance);
		if (distance != 0)
		{
			return distance;
		}

		return Name.CompareTo(other.Name);
	}
}