using System.Diagnostics;

namespace Advobot.Classes.CloseWords
{
	/// <summary>
	/// Holds an object which has a name and text and its closeness.
	/// </summary>
	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	public sealed class CloseWord<T>
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
		/// The object this is coming from.
		/// </summary>
		public T Value { get; set; }

		/// <summary>
		/// Initializes the object with the supplied values.
		/// </summary>
		/// <param name="closeness"></param>
		/// <param name="name"></param>
		/// <param name="value"></param>
		public CloseWord(int closeness, string name, T value)
		{
			Closeness = closeness;
			Name = name;
			Value = value;
		}

		private string DebuggerDisplay
			=> $"Name = {Name}, Closeness = {Closeness}";
	}
}