using System.Diagnostics;

namespace Advobot.Similar;

/// <summary>
/// Holds an object which has a name and text and its closeness.
/// </summary>
/// <typeparam name="T"></typeparam>
/// <param name="Name">The name being compared.</param>
/// <param name="Search">The search term.</param>
/// <param name="Distance">
/// The Damerau Levenshtein distance between <see cref="Name"/> and <see cref="Search"/>.
/// </param>
/// <param name="Value">The object this is coming from.</param>
[DebuggerDisplay("{DebuggerDisplay,nq}")]
public readonly record struct Similarity<T>(
	string Name,
	string Search,
	int Distance,
	T Value
) : IComparable<Similarity<T>>
{
	private string DebuggerDisplay => $"Name = {Name}, Distance = {Distance}";

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