using System.Diagnostics;

namespace Advobot.Classes.CloseWords;

/// <summary>
/// Holds an object which has a name and text and its closeness.
/// </summary>
[DebuggerDisplay("{DebuggerDisplay,nq}")]
public readonly struct CloseWord<T>(string name, string search, int distance, T value) : IComparable<CloseWord<T>>
{
	/// <summary>
	/// The Damerau Levenshtein distance between <see cref="Name"/> and <see cref="Search"/>.
	/// </summary>
	public int Distance { get; } = distance;
	/// <summary>
	/// The name being compared.
	/// </summary>
	public string Name { get; } = name;
	/// <summary>
	/// The search term.
	/// </summary>
	public string Search { get; } = search;
	/// <summary>
	/// The object this is coming from.
	/// </summary>
	public T Value { get; } = value;
	private string DebuggerDisplay
		=> $"Name = {Name}, Distance = {Distance}";

	/// <inheritdoc />
	public int CompareTo(CloseWord<T> other)
	{
		var distance = Distance.CompareTo(other.Distance);
		if (distance != 0)
		{
			return distance;
		}

		return Name.CompareTo(other.Name);
	}
}