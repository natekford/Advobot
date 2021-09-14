using System.Diagnostics;

namespace Advobot.Classes.CloseWords
{
	/// <summary>
	/// Holds an object which has a name and text and its closeness.
	/// </summary>
	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	public readonly struct CloseWord<T> : IComparable<CloseWord<T>>
	{
		/// <summary>
		/// The Damerau Levenshtein distance between <see cref="Name"/> and <see cref="Search"/>.
		/// </summary>
		public int Distance { get; }
		/// <summary>
		/// The name being compared.
		/// </summary>
		public string Name { get; }
		/// <summary>
		/// The search term.
		/// </summary>
		public string Search { get; }
		/// <summary>
		/// The object this is coming from.
		/// </summary>
		public T Value { get; }
		private string DebuggerDisplay
			=> $"Name = {Name}, Distance = {Distance}";

		/// <summary>
		/// Creates an instance of <see cref="CloseWord{T}"/>.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="search"></param>
		/// <param name="distance"></param>
		/// <param name="value"></param>
		public CloseWord(string name, string search, int distance, T value)
		{
			Name = name;
			Search = search;
			Distance = distance;
			Value = value;
		}

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
}